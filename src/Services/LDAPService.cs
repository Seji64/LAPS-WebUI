using CliWrap;
using LAPS_WebUI.Interfaces;
using LAPS_WebUI.Models;
using LdapForNet;
using LdapForNet.Native;
using Microsoft.Extensions.Options;
using Serilog;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace LAPS_WebUI.Services
{
    public class LdapService : ILdapService
    {
        private readonly IOptions<LdapOptions> _ldapOptions;
        private readonly IOptions<LapsOptions> _lapsOptions;
        public LdapService(IOptions<LdapOptions> ldapoptions, IOptions<LapsOptions> lapsOptions)
        {
            _ldapOptions = ldapoptions;
            _lapsOptions = lapsOptions;
        }

        public async Task<LdapConnection?> CreateBindAsync(string username, string password)
        {
            LdapConnection ldapConnection;
            ldapConnection = new LdapConnection();

            try
            {
                ldapConnection.Connect(_ldapOptions.Value.Server, _ldapOptions.Value.Port, _ldapOptions.Value.UseSSL ? Native.LdapSchema.LDAPS : Native.LdapSchema.LDAP);

                if (_ldapOptions.Value.TrustAllCertificates)
                {
                    ldapConnection.TrustAllCertificates();
                }

                await ldapConnection.BindAsync(Native.LdapAuthMechanism.SIMPLE, username, password);

                ldapConnection.SetOption(Native.LdapOption.LDAP_OPT_REFERRALS, IntPtr.Zero);

            }
            catch (Exception ex)
            {
                Log.Error("{ErrorMessage}",ex.Message);
                return null;
            }

            return ldapConnection;
        }

        public async Task<bool> TestCredentialsAsync(string username, string password)
        {
            using var connection = await CreateBindAsync(username, password);
            if (connection != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> TestCredentialsAsync(LdapCredential ldapCredential)
        {
            using var connection = await CreateBindAsync(ldapCredential.UserName, ldapCredential.Password);
            if (connection != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<ADComputer?> GetADComputerAsync(LdapCredential ldapCredential, string name)
        {
            ADComputer? ADComputer = null;

            if (ldapCredential is null)
            {
                throw new Exception("Failed to get LDAP Credentials");
            }

            using (LdapConnection? ldapConnection = await CreateBindAsync(ldapCredential.UserName, ldapCredential.Password))
            {
                if (ldapConnection is null)
                {
                    throw new Exception("LDAP bin failed!");
                }

                string? defaultNamingContext = _ldapOptions.Value.SearchBase;

                var ldapSearchResult = (await ldapConnection.SearchByCnAsync(defaultNamingContext, name, Native.LdapSearchScope.LDAP_SCOPE_SUB)).FirstOrDefault();

                if (ldapSearchResult != null)
                {
                    ADComputer = new ADComputer(ldapSearchResult.DirectoryAttributes["cn"].GetValues<string>().First())
                    {
                        LAPSInformations = new()
                    };

                    #region "Try LAPS v1"

                    if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == "ms-Mcs-AdmPwd") && (_lapsOptions.Value.ForceVersion is null || _lapsOptions.Value.ForceVersion == Enums.LAPSVersion.v1))
                    {
                        LapsInformation lapsInformationEntry = new()
                        {
                            Version = Enums.LAPSVersion.v1,
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

                    string fieldName = (_lapsOptions.Value.EncryptionDisabled ? "msLAPS-Password" : "msLAPS-EncryptedPassword");

                    if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == fieldName) && (_lapsOptions.Value.ForceVersion is null || _lapsOptions.Value.ForceVersion == Enums.LAPSVersion.v2))
                    {
                        MsLAPSPayload? msLAPS_Payload = null;
                        string ldapValue = string.Empty;

                        if (_lapsOptions.Value.EncryptionDisabled)
                        {
                            ldapValue = ldapSearchResult.DirectoryAttributes["msLAPS-Password"].GetValues<string>().First().ToString();
                        }
                        else
                        {
                            byte[] encryptedPass = ldapSearchResult.DirectoryAttributes["msLAPS-EncryptedPassword"].GetValues<byte[]>().First().Skip(16).ToArray();
                            ldapValue = await DecryptLAPSPayload(encryptedPass, ldapCredential);
                        }

                        msLAPS_Payload = JsonSerializer.Deserialize<MsLAPSPayload>(ldapValue) ?? throw new Exception("Failed to parse LAPS Password");

                        LapsInformation lapsInformationEntry = new()
                        {
                            Version = Enums.LAPSVersion.v2,
                            Account = msLAPS_Payload.ManagedAccountName,
                            Password = msLAPS_Payload.Password,
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
                                var historic_msLAPS_Payload = JsonSerializer.Deserialize<MsLAPSPayload>(historicLdapValue);

                                if (historic_msLAPS_Payload != null)
                                {
                                    LapsInformation historicLapsInformationEntry = new()
                                    {
                                        Version = Enums.LAPSVersion.v2,
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

                        if (ADComputer.LAPSInformations is null || !ADComputer.LAPSInformations.Any())
                        {
                            throw new Exception("No permission to retrieve LAPS Password or no LAPS Password set!");
                        }

                        ADComputer.LAPSInformations = ADComputer.LAPSInformations.OrderBy(x => x.PasswordExpireDate).ToList();
                    }

                    #endregion
                }
                else
                {
                    throw new Exception($"AD Computer {name} could not be found");
                }
            }

            return ADComputer;
        }

        private static async Task<string> DecryptLAPSPayload(byte[] value, LdapCredential ldapCredential)
        {
            StringBuilder pythonScriptResult = new();
            string pythonDecryptScriptPath = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory)!, "scripts", "DecryptEncryptedLAPSPassword.py");

            var pythonCmd = Cli.Wrap("python3")
                            .WithArguments($"\"{pythonDecryptScriptPath}\" --user {ldapCredential.UserName} --password {ldapCredential.Password} --data {Convert.ToBase64String(value)})")
                            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(pythonScriptResult));

            await pythonCmd.ExecuteAsync();

            if (pythonDecryptScriptPath is null || pythonDecryptScriptPath.Length == 0)
            {
                throw new Exception("Failed to decrypt laps password!");
            }

            string ldapValue = pythonScriptResult.ToString().Trim();
            ldapValue = ldapValue.Remove(ldapValue.LastIndexOf("}") + 1);

            return ldapValue;
        }

        public async Task<List<ADComputer>> SearchADComputersAsync(LdapCredential ldapCredential, string query)
        {
            List<ADComputer> result = new();

            if (ldapCredential is null)
            {
                throw new Exception("Failed to get LDAP Credentials");
            }

            using (LdapConnection? ldapConnection = await CreateBindAsync(ldapCredential.UserName, ldapCredential.Password))
            {
                var filter = $"(&(objectCategory=computer)(name={query}{(query.EndsWith("*") ? "" : "*")}))";
                var PropertiesToLoad = new string[] { "cn" };
                string? defaultNamingContext = _ldapOptions.Value.SearchBase;

                try
                {
                    if (ldapConnection is null)
                    {
                        throw new Exception("LDAP Bind failed!");
                    }

                    var ldapSearchResults = await ldapConnection.SearchAsync(defaultNamingContext, filter, PropertiesToLoad, Native.LdapSearchScope.LDAP_SCOPE_SUB);

                    foreach (var ldapSearchResult in ldapSearchResults)
                    {
                        result.Add(new ADComputer(ldapSearchResult.DirectoryAttributes["cn"].GetValues<string>().First()));
                    }
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
