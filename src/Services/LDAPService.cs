using LAPS_WebUI.Interfaces;
using LAPS_WebUI.Models;
using LdapForNet;
using LdapForNet.Native;
using Microsoft.Extensions.Options;
using Serilog;

namespace LAPS_WebUI.Services
{
    public class LDAPService : ILDAPService
    {
        private readonly IOptions<LDAPOptions> _ldapOptions;
        public LDAPService(IOptions<LDAPOptions> ldapoptions)
        {
            _ldapOptions = ldapoptions;
        }

        public async Task<LdapConnection?> CreateBindAsync(string username, string password)
        {
            LdapConnection ldapConnection;
            ldapConnection = new LdapConnection();

            try
            {
                ldapConnection.Connect(_ldapOptions.Value.Server, _ldapOptions.Value.Port, _ldapOptions.Value.UseSSL ? LdapForNet.Native.Native.LdapSchema.LDAPS : LdapForNet.Native.Native.LdapSchema.LDAP);

                if (_ldapOptions.Value.TrustAllCertificates)
                {
                    ldapConnection.TrustAllCertificates();
                }

                await ldapConnection.BindAsync(LdapForNet.Native.Native.LdapAuthMechanism.SIMPLE, username, password);

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
                    ADComputer = new ADComputer(ldapSearchResult.DirectoryAttributes["cn"].GetValues<string>().First());

                    if (ldapSearchResult.DirectoryAttributes.Any(x => x.Name == "ms-Mcs-AdmPwd"))
                    {
                        ADComputer.LAPSPassword = ldapSearchResult.DirectoryAttributes["ms-Mcs-AdmPwd"].GetValues<string>().First().ToString();
                        ADComputer.LAPSPasswordExpireDate = DateTime.FromFileTime(Convert.ToInt64(ldapSearchResult.DirectoryAttributes["ms-Mcs-AdmPwdExpirationTime"].GetValues<string>().First().ToString()));
                    }
                    else
                    {
                        throw new Exception("No permission to retrieve LAPS Password");
                    }
                }
                else
                {
                    throw new Exception($"AD Computer {name} could not be found");
                }
            }

            return ADComputer;
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
