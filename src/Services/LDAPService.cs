using CliWrap;
using LAPS_WebUI.Enums;
using LAPS_WebUI.Interfaces;
using LAPS_WebUI.Models;
using LdapForNet;
using LdapForNet.Native;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using static LdapForNet.Native.Native;

namespace LAPS_WebUI.Services
{
    public class LdapService : ILdapService
    {
        private readonly IOptions<List<Domain>> _Domains;
        public LdapService(IOptions<List<Domain>> domains)
        {
            _Domains = domains;

            if (_Domains.Value is null || _Domains.Value.Count == 0)
            {
                Log.Error("No Domains configured! Please check your configuration!");
            }
        }

        public async Task<List<Domain>> GetDomainsAsync()
        {
            return await Task.FromResult(_Domains.Value);
        }

        public async Task<LdapConnection?> CreateBindAsync(string domainName, string username, string password)
        {
            LdapConnection ldapConnection;
            ldapConnection = new LdapConnection();

            try
            {
                Domain domain = _Domains.Value.Single(x => x.Name == domainName);

                ldapConnection.Connect(domain.Ldap.Server, domain.Ldap.Port, domain.Ldap.UseSSL ? LdapSchema.LDAPS : LdapSchema.LDAP);

                if (domain.Ldap.TrustAllCertificates)
                {
                    ldapConnection.TrustAllCertificates();
                }

                await ldapConnection.BindAsync(LdapAuthMechanism.SIMPLE, username, password);

                ldapConnection.SetOption(LdapOption.LDAP_OPT_REFERRALS, IntPtr.Zero);

            }
            catch (Exception ex)
            {
                Log.Error("{ErrorMessage}",ex.Message);
                return null;
            }

            return ldapConnection;
        }

        public async Task<bool> TestCredentialsAsync(string domainName, string username, string password)
        {
            using var connection = await CreateBindAsync(domainName, username, password);
            if (connection != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> TestCredentialsAsync(string domainName, LdapCredential ldapCredential)
        {
            using var connection = await CreateBindAsync(domainName, ldapCredential.UserName, ldapCredential.Password);
            if (connection != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ClearLapsPassword(string domainName, LdapCredential ldapCredential, string distinguishedName, LAPSVersion version, bool encrypted)
        {
            Domain? domain = _Domains.Value.SingleOrDefault(x => x.Name == domainName) ?? throw new Exception($"No configured domain found with name {domainName}");

            if (ldapCredential is null)
            {
                throw new Exception("Failed to get LDAP Credentials");
            }
            using LdapConnection? ldapConnection = await CreateBindAsync(domainName, ldapCredential.UserName, ldapCredential.Password) ?? throw new Exception("LDAP bind failed!");

            string? defaultNamingContext = domain.Ldap.SearchBase;
            string attribute = string.Empty;

            if (version == LAPSVersion.v1)
            {
                attribute = "ms-Mcs-AdmPwd";
            }

            if (version == LAPSVersion.v2)
            {
                attribute = encrypted ? "msLAPS-EncryptedPassword" : "msLAPS-Password";
            }

            var ldapSearchResult = (await ldapConnection.SearchAsync(defaultNamingContext, $"(&(objectCategory=computer)(distinguishedName={distinguishedName}))", [attribute], LdapSearchScope.LDAP_SCOPE_SUB)).SingleOrDefault();

            if (ldapSearchResult != null)
            {
                var resetRequest = new DirectoryModificationAttribute
                {
                    LdapModOperation = LdapModOperation.LDAP_MOD_DELETE,
                    Name = attribute
                };

                if (version == LAPSVersion.v1 || !encrypted)
                {
                    resetRequest.Add(ldapSearchResult.DirectoryAttributes[attribute].GetValues<string>().First().ToString());
                }
                else
                {
                    resetRequest.Add(ldapSearchResult.DirectoryAttributes[attribute].GetValues<byte[]>().First().ToArray());
                }

                var response = (ModifyResponse)await ldapConnection.SendRequestAsync(new ModifyRequest(distinguishedName, resetRequest));

                return response.ResultCode == ResultCode.Success;
            }
            else
            {
                throw new Exception($"AD Computer with DN '{distinguishedName}' could not be found");
            }

           
        }

        public async Task<ADComputer?> GetADComputerAsync(string domainName, LdapCredential ldapCredential, string distinguishedName)
        {
            ADComputer? ADComputer = null;
            Domain? domain = _Domains.Value.SingleOrDefault(x => x.Name == domainName) ?? throw new Exception($"No configured domain found with name {domainName}");

            if (ldapCredential is null)
            {
                throw new Exception("Failed to get LDAP Credentials");
            }

            using (LdapConnection? ldapConnection = await CreateBindAsync(domainName, ldapCredential.UserName, ldapCredential.Password))
            {
                if (ldapConnection is null)
                {
                    throw new Exception("LDAP bind failed!");
                }

                string? defaultNamingContext = domain.Ldap.SearchBase;

                var ldapSearchResult = (await ldapConnection.SearchAsync(defaultNamingContext, $"(&(objectCategory=computer)(distinguishedName={distinguishedName}))",null, LdapSearchScope.LDAP_SCOPE_SUB)).SingleOrDefault();

                if (ldapSearchResult != null)
                {
                    ADComputer = new ADComputer(ldapSearchResult.Dn, ldapSearchResult.DirectoryAttributes["cn"].GetValues<string>().First())
                    {
                        LAPSInformations = []
                    };

                    #region "Try LAPS v1"

                    if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == "ms-Mcs-AdmPwd") && (domain.Laps.ForceVersion == LAPSVersion.All || domain.Laps.ForceVersion == LAPSVersion.v1))
                    {
                        LapsInformation lapsInformationEntry = new()
                        {
                            ComputerName = ADComputer.Name,
                            Version = LAPSVersion.v1,
                            Account = null,
                            Password = ldapSearchResult.DirectoryAttributes["ms-Mcs-AdmPwd"].GetValues<string>().First().ToString(),
                            PasswordExpireDate = DateTime.FromFileTimeUtc(Convert.ToInt64(ldapSearchResult.DirectoryAttributes["ms-Mcs-AdmPwdExpirationTime"].GetValues<string>().First().ToString())),
                            IsCurrent = true,
                            PasswordSetDate = null
                        };

                        ADComputer.LAPSInformations.Add(lapsInformationEntry);
                    }

                    #endregion

                    #region "Try LAPS v2"

                    string fieldName = (domain.Laps.EncryptionDisabled ? "msLAPS-Password" : "msLAPS-EncryptedPassword");

                    if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == fieldName) && (domain.Laps.ForceVersion == LAPSVersion.All || domain.Laps.ForceVersion == LAPSVersion.v2))
                    {
                        MsLapsPayload? msLAPS_Payload = null;
                        string ldapValue = string.Empty;

                        if (domain.Laps.EncryptionDisabled)
                        {
                            ldapValue = ldapSearchResult.DirectoryAttributes["msLAPS-Password"].GetValues<string>().First().ToString();
                        }
                        else
                        {
                            byte[] encryptedPass = ldapSearchResult.DirectoryAttributes["msLAPS-EncryptedPassword"].GetValues<byte[]>().First().Skip(16).ToArray();
                            ldapValue = await DecryptLAPSPayload(encryptedPass, ldapCredential);
                        }

                        msLAPS_Payload = JsonSerializer.Deserialize<MsLapsPayload>(ldapValue) ?? throw new Exception("Failed to parse LAPS Password");

                        LapsInformation lapsInformationEntry = new()
                        {
                            ComputerName = ADComputer.Name,
                            Version = LAPSVersion.v2,
                            Account = msLAPS_Payload.ManagedAccountName,
                            Password = msLAPS_Payload.Password,
                            WasEncrypted = !domain.Laps.EncryptionDisabled,
                            PasswordExpireDate =  DateTime.FromFileTimeUtc(Convert.ToInt64(ldapSearchResult.DirectoryAttributes["msLAPS-PasswordExpirationTime"].GetValues<string>().First().ToString())),
                            IsCurrent = true,
                            PasswordSetDate = DateTime.FromFileTimeUtc(Int64.Parse(msLAPS_Payload.PasswordUpdateTime!, System.Globalization.NumberStyles.HexNumber))
                            
                        };

                        ADComputer.LAPSInformations.Add(lapsInformationEntry);

                        if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == "msLAPS-EncryptedPasswordHistory"))
                        {

                            foreach (var historyEntry in ldapSearchResult.DirectoryAttributes["msLAPS-EncryptedPasswordHistory"].GetValues<byte[]>())
                            {
                                byte[] historicEncryptedPass = historyEntry.Skip(16).ToArray();
                                string historicLdapValue = await DecryptLAPSPayload(historicEncryptedPass, ldapCredential);
                                var historic_msLAPS_Payload = JsonSerializer.Deserialize<MsLapsPayload>(historicLdapValue);

                                if (historic_msLAPS_Payload != null)
                                {
                                    LapsInformation historicLapsInformationEntry = new()
                                    {
                                        ComputerName = ADComputer.Name,
                                        Version = LAPSVersion.v2,
                                        Account = historic_msLAPS_Payload.ManagedAccountName,
                                        Password = historic_msLAPS_Payload.Password,
                                        PasswordExpireDate = null,
                                        PasswordSetDate = DateTime.FromFileTimeUtc(Int64.Parse(historic_msLAPS_Payload.PasswordUpdateTime!, System.Globalization.NumberStyles.HexNumber))
                                    };

                                    ADComputer.LAPSInformations.Add(historicLapsInformationEntry);
                                }
                                else
                                {
                                    Log.Warning("Failed to decrypt LAPS History entry");
                                }
                            }
                        }
                    }

                    #endregion

                    if (ADComputer.LAPSInformations is null || ADComputer.LAPSInformations.Count == 0)
                    {
                        ADComputer.FailedToRetrieveLAPSDetails = true;
                    }
                    else
                    {
                        ADComputer.LAPSInformations = [.. ADComputer.LAPSInformations.OrderBy(x => x.PasswordExpireDate)];
                    }
     
                }
                else
                {
                    throw new Exception($"AD Computer with DN '{distinguishedName}' could not be found");
                }
            }

            return ADComputer;
        }

        private static async Task<string> DecryptLAPSPayload(byte[] value, LdapCredential ldapCredential)
        {

            StringBuilder pythonScriptResult = new();
            string pythonDecryptScriptPath = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory)!, "scripts", "DecryptEncryptedLAPSPassword.py");

            string pythonBin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "python" : "python3";

            try
            {

                var pythonCmd = Cli.Wrap(pythonBin)
                                .WithArguments($"\"{pythonDecryptScriptPath}\" --user \"{ldapCredential.UserName}\" --password \"{ldapCredential.Password}\" --data \"{Convert.ToBase64String(value)}\"")
                                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(pythonScriptResult));

                await pythonCmd.ExecuteAsync();

                if (pythonDecryptScriptPath is null || pythonDecryptScriptPath.Length == 0)
                {
                    throw new Exception("Failed to decrypt laps password!");
                }

                string ldapValue = pythonScriptResult.ToString().Trim();
                ldapValue = ldapValue.Remove(ldapValue.LastIndexOf('}') + 1);

                return ldapValue;
            }
            catch (Exception ex)
            {
                Log.Error("Decrypt LAPS Password failed => {ErrorMessage}", ex.Message);
                throw new ArgumentException("Failed to decrypt LAPSv2 Password");
            }

        }

        public async Task<List<ADComputer>> SearchADComputersAsync(string domainName, LdapCredential ldapCredential, string query)
        {
            List<ADComputer> result = [];
            Domain? domain = _Domains.Value.SingleOrDefault(x => x.Name == domainName) ?? throw new Exception($"No configured domain found with name {domainName}");

            if (ldapCredential is null)
            {
                throw new Exception("Failed to get LDAP Credentials");
            }

            using (LdapConnection? ldapConnection = await CreateBindAsync(domainName, ldapCredential.UserName, ldapCredential.Password))
            {
                string filter = $"(&(objectCategory=computer)(name={query}{(query.EndsWith('*') ? string.Empty : '*')}))";
                var PropertiesToLoad = new string[] { "cn", "distinguishedName" };
                string? defaultNamingContext = domain.Ldap.SearchBase;

                try
                {
                    if (ldapConnection is null)
                    {
                        throw new Exception("LDAP Bind failed!");
                    }

                    var ldapSearchResults = await ldapConnection.SearchAsync(defaultNamingContext, filter, PropertiesToLoad, LdapSearchScope.LDAP_SCOPE_SUB);

                    result.AddRange(ldapSearchResults.Select(o =>
                    {
                        return new ADComputer(o.Dn, o.DirectoryAttributes["cn"].GetValues<string>().First());
                    }).ToList());
                }
                catch (Exception ex)
                {
                    Log.Error("{ErrorMessage}", ex.Message);
                }
            }
            return result;
        }
    }
}
