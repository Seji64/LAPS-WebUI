using CliWrap;
using LAPS_WebUI.Enums;
using LAPS_WebUI.Interfaces;
using LAPS_WebUI.Models;
using LdapForNet;
using Microsoft.Extensions.Options;
using Serilog;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using static LdapForNet.Native.Native;

namespace LAPS_WebUI.Services
{
    public class LdapService : ILdapService
    {
        private readonly IOptions<List<Domain>> _domains;
        public LdapService(IOptions<List<Domain>> domains)
        {
            _domains = domains;

            if (_domains.Value.Count == 0)
            {
                Log.Error("No Domains configured! Please check your configuration!");
            }
        }

        public async Task<List<Domain>> GetDomainsAsync()
        {
            return await Task.FromResult(_domains.Value);
        }

        public async Task<LdapConnection?> CreateBindAsync(string domainName, string username, string password)
        {
            LdapConnection ldapConnection = new();

            try
            {
                Domain domain = _domains.Value.Single(x => x.Name == domainName);

                ldapConnection.Connect(domain.Ldap.Server, domain.Ldap.Port, domain.Ldap.UseSsl ? LdapSchema.LDAPS : LdapSchema.LDAP);

                if (domain.Ldap.TrustAllCertificates)
                {
                    ldapConnection.TrustAllCertificates();
                }
                
                await ldapConnection.BindAsync(domain.Ldap.AuthMechanism,username, password);

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
            using LdapConnection? connection = await CreateBindAsync(domainName, username, password);
            return connection != null;
        }

        public async Task<bool> TestCredentialsAsync(string domainName, LdapCredential ldapCredential)
        {
            using LdapConnection? connection = await CreateBindAsync(domainName, ldapCredential.UserName, ldapCredential.Password);
            return connection != null;
        }

        public async Task<bool> ClearLapsPassword(string domainName, LdapCredential ldapCredential, string distinguishedName, LAPSVersion version)
        {

            if (ldapCredential is null)
            {
                throw new Exception("Failed to get LDAP Credentials");
            }
            using LdapConnection ldapConnection = await CreateBindAsync(domainName, ldapCredential.UserName, ldapCredential.Password) ?? throw new Exception("LDAP bind failed!");

            string attribute = version switch
            {
                LAPSVersion.v1 => "ms-Mcs-AdmPwdExpirationTime",
                LAPSVersion.v2 => "msLAPS-PasswordExpirationTime",
                _ => string.Empty
            };

            DirectoryModificationAttribute resetRequest = new DirectoryModificationAttribute
            {
                LdapModOperation = LdapModOperation.LDAP_MOD_REPLACE,
                Name = attribute
            };

            resetRequest.Add(DateTime.Now.ToFileTimeUtc().ToString());

            ModifyResponse? response = (ModifyResponse)await ldapConnection.SendRequestAsync(new ModifyRequest(distinguishedName, resetRequest));

            return response.ResultCode == ResultCode.Success;
        }
        
        private static string EscapeLdapSearchFilter(string searchFilter)
        {
            StringBuilder escape = new StringBuilder();
            foreach (char current in searchFilter)
            {
                switch (current)
                {
                    case '\\':
                        escape.Append(@"\5c");
                        break;
                    case '*':
                        escape.Append(@"\2a");
                        break;
                    case '(':
                        escape.Append(@"\28");
                        break;
                    case ')':
                        escape.Append(@"\29");
                        break;
                    case '\u0000':
                        escape.Append(@"\00");
                        break;
                    case '/':
                        escape.Append(@"\2f");
                        break;
                    default:
                        escape.Append(current);
                        break;
                }
            }

            return escape.ToString();
        }
        
        public async Task<AdComputer?> GetAdComputerAsync(string domainName, LdapCredential ldapCredential, string distinguishedName)
        {
            AdComputer? adComputer;
            Domain domain = _domains.Value.SingleOrDefault(x => x.Name == domainName) ?? throw new Exception($"No configured domain found with name {domainName}");

            if (ldapCredential is null)
            {
                throw new Exception("Failed to get LDAP Credentials");
            }

            using LdapConnection? ldapConnection = await CreateBindAsync(domainName, ldapCredential.UserName, ldapCredential.Password);
            if (ldapConnection is null)
            {
                throw new Exception("LDAP bind failed!");
            }

            string? defaultNamingContext = domain.Ldap.SearchBase;
            
            string ldapFilter = $"(&(objectCategory=computer)(distinguishedName={distinguishedName}))";
            
            LdapEntry? ldapSearchResult = (await ldapConnection.SearchAsync(defaultNamingContext, ldapFilter)).SingleOrDefault();

            if (ldapSearchResult != null)
            {
                adComputer = new AdComputer(ldapSearchResult.Dn, ldapSearchResult.DirectoryAttributes["cn"].GetValues<string>().First())
                {
                    LapsInformations = []
                };

                #region "Try LAPS v1"

                if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == "ms-Mcs-AdmPwd") && (domain.Laps.ForceVersion == LAPSVersion.All || domain.Laps.ForceVersion == LAPSVersion.v1))
                {
                    LapsInformation lapsInformationEntry = new()
                    {
                        ComputerName = adComputer.Name,
                        Version = LAPSVersion.v1,
                        Account = null,
                        Password = ldapSearchResult.DirectoryAttributes["ms-Mcs-AdmPwd"].GetValues<string>().First(),
                        PasswordExpireDate = DateTime.FromFileTimeUtc(Convert.ToInt64(ldapSearchResult.DirectoryAttributes["ms-Mcs-AdmPwdExpirationTime"].GetValues<string>().First())).ToLocalTime(),
                        IsCurrent = true,
                        PasswordSetDate = null
                    };

                    adComputer.LapsInformations.Add(lapsInformationEntry);
                }

                #endregion

                #region "Try LAPS v2"

                string fieldName = (domain.Laps.EncryptionDisabled ? "msLAPS-Password" : "msLAPS-EncryptedPassword");

                if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == fieldName) && (domain.Laps.ForceVersion == LAPSVersion.All || domain.Laps.ForceVersion == LAPSVersion.v2))
                {
                    MsLapsPayload? msLapsPayload;
                    string ldapValue;

                    if (domain.Laps.EncryptionDisabled)
                    {
                        ldapValue = ldapSearchResult.DirectoryAttributes["msLAPS-Password"].GetValues<string>().First();
                    }
                    else
                    {
                        byte[] encryptedPass = ldapSearchResult.DirectoryAttributes["msLAPS-EncryptedPassword"].GetValues<byte[]>().First().Skip(16).ToArray();
                        ldapValue = await DecryptLapsPayload(encryptedPass, ldapCredential);
                    }

                    msLapsPayload = JsonSerializer.Deserialize<MsLapsPayload>(ldapValue) ?? throw new Exception("Failed to parse LAPS Password");

                    LapsInformation lapsInformationEntry = new()
                    {
                        ComputerName = adComputer.Name,
                        Version = LAPSVersion.v2,
                        Account = msLapsPayload.ManagedAccountName,
                        Password = msLapsPayload.Password,
                        WasEncrypted = !domain.Laps.EncryptionDisabled,
                        PasswordExpireDate =  DateTime.FromFileTimeUtc(Convert.ToInt64(ldapSearchResult.DirectoryAttributes["msLAPS-PasswordExpirationTime"].GetValues<string>().First())).ToLocalTime(),
                        IsCurrent = true,
                        PasswordSetDate = DateTime.FromFileTimeUtc(Int64.Parse(msLapsPayload.PasswordUpdateTime!, System.Globalization.NumberStyles.HexNumber)).ToLocalTime()

                    };

                    adComputer.LapsInformations.Add(lapsInformationEntry);

                    if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == "msLAPS-EncryptedPasswordHistory"))
                    {

                        foreach (byte[]? historyEntry in ldapSearchResult.DirectoryAttributes["msLAPS-EncryptedPasswordHistory"].GetValues<byte[]>())
                        {
                            byte[] historicEncryptedPass = historyEntry.Skip(16).ToArray();
                            string historicLdapValue = await DecryptLapsPayload(historicEncryptedPass, ldapCredential);
                            MsLapsPayload? historicMsLapsPayload = JsonSerializer.Deserialize<MsLapsPayload>(historicLdapValue);

                            if (historicMsLapsPayload != null)
                            {
                                LapsInformation historicLapsInformationEntry = new()
                                {
                                    ComputerName = adComputer.Name,
                                    Version = LAPSVersion.v2,
                                    Account = historicMsLapsPayload.ManagedAccountName,
                                    Password = historicMsLapsPayload.Password,
                                    PasswordExpireDate = null,
                                    PasswordSetDate = DateTime.FromFileTimeUtc(Int64.Parse(historicMsLapsPayload.PasswordUpdateTime!, System.Globalization.NumberStyles.HexNumber)).ToLocalTime()
                                };

                                adComputer.LapsInformations.Add(historicLapsInformationEntry);
                            }
                            else
                            {
                                Log.Warning("Failed to decrypt LAPS History entry");
                            }
                        }
                    }
                }

                #endregion

                if (adComputer.LapsInformations is null || adComputer.LapsInformations.Count == 0)
                {
                    adComputer.FailedToRetrieveLapsDetails = true;
                }
                else
                {
                    adComputer.LapsInformations = [.. adComputer.LapsInformations.OrderBy(x => x.PasswordExpireDate)];
                }
     
            }
            else
            {
                throw new Exception($"AD Computer with DN '{distinguishedName}' could not be found");
            }

            return adComputer;
        }

        private static async Task<string> DecryptLapsPayload(byte[] value, LdapCredential ldapCredential)
        {

            StringBuilder pythonScriptResult = new();
            string pythonDecryptScriptPath = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory)!, "scripts", "DecryptEncryptedLAPSPassword.py");

            string pythonBin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "python" : "python3";

            try
            {

                Command pythonCmd = Cli.Wrap(pythonBin)
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

        public async Task<List<AdComputer>> SearchAdComputersAsync(string domainName, LdapCredential ldapCredential, string query)
        {
            List<AdComputer> result = [];
            Domain domain = _domains.Value.SingleOrDefault(x => x.Name == domainName) ?? throw new Exception($"No configured domain found with name {domainName}");

            if (ldapCredential is null)
            {
                throw new Exception("Failed to get LDAP Credentials");
            }
            
            using LdapConnection? ldapConnection = await CreateBindAsync(domainName, ldapCredential.UserName, ldapCredential.Password);
            string filter = $"(&(objectCategory=computer)(name={query}{(query.EndsWith('*') ? string.Empty : '*')}))";
            string[] propertiesToLoad = new string[] { "cn", "distinguishedName" };
            string? defaultNamingContext = domain.Ldap.SearchBase;

            try
            {
                if (ldapConnection is null)
                {
                    throw new Exception("LDAP Bind failed!");
                }

                IList<LdapEntry>? ldapSearchResults = await ldapConnection.SearchAsync(defaultNamingContext, filter, propertiesToLoad);

                result.AddRange(ldapSearchResults.Select(o => new AdComputer(EscapeLdapSearchFilter(o.Dn), o.DirectoryAttributes["cn"].GetValues<string>().First())).ToList());
            }
            catch (Exception ex)
            {
                Log.Error("{ErrorMessage}", ex.Message);
            }

            return result;
        }
    }
}
