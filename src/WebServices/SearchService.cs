using Integrative.Lara;
using LdapForNet;
using LdapForNet.Native;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Logger = NLog.Logger;

namespace LAPS_WebUI.WebServices
{
    [LaraWebService(Address = "/search", Method = "GET", ContentType = "application/json")]
    class SearchService : IWebService
    {
        public Task<string> Execute()
        {

            if (!UserSession.LoggedIn)
            {
                LaraUI.Service.StatusCode = System.Net.HttpStatusCode.Forbidden;
                return Task.FromResult("{ \"Message\": \"Not Logged In\" }");
            }
            else
            {
                
                if (LaraUI.Service.Http.Request.Query.Count != 0)
                {

                    var request = new SearchRequest();

                    if (LaraUI.Service.Http.Request.Query.ContainsKey("term"))
                    {
                        request.term = LaraUI.Service.Http.Request.Query["term"].ToArray().First();
                    }

                    if (LaraUI.Service.Http.Request.Query.ContainsKey("_type"))
                    {
                        request._type = LaraUI.Service.Http.Request.Query["_type"].ToArray().First();
                    }

                    if (LaraUI.Service.Http.Request.Query.ContainsKey("page"))
                    {
                        request.page = int.Parse(LaraUI.Service.Http.Request.Query["page"].ToArray().First());
                    }


                    // create response
                    var response = new SearchResults();

                    int i = 1;
                    object _lock = new Object();

                    Parallel.ForEach(SearchADComputers(request.term), (result) =>
                    {
                        response.results.Add(new SearchResult() { id = i, disabled = false, selected = false, text = result.Name });
                        lock (_lock){
                            i++;
                        }
                    });

                    var json = LaraUI.JSON.Stringify(response);
                    return Task.FromResult(json);
                }
                else
                {
                    LaraUI.Service.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return Task.FromResult("{ \"Error\": \"Failed to process search\" }");
                }
            }

        }

        private List<ADComputer> SearchADComputers(string searchTerm)
        {

            Logger m_log = LogManager.GetLogger("SearchADComputers");

            List<ADComputer> searchResult = new List<ADComputer>();

            try
            {

                var filter = string.Format("(&(objectCategory=computer)(name={0}*))", searchTerm);
                var PropertiesToLoad = new string[] { "cn" };

                using (var ldapConnection = new LdapConnection())
                {
                    ldapConnection.Connect(Settings.ThisInstance.LDAP.Server, Settings.ThisInstance.LDAP.Port, Settings.ThisInstance.LDAP.UseSSL ? Native.LdapSchema.LDAPS : Native.LdapSchema.LDAP);

                    if (Settings.ThisInstance.LDAP.TrustAllCertificates) { ldapConnection.TrustAllCertificates(); }

                    ldapConnection.Bind(Native.LdapAuthMechanism.SIMPLE, UserSession.loginData.Username, UserSession.loginData.Password);

                    string defaultNamingContext = string.Empty;

                    if (string.IsNullOrWhiteSpace(Settings.ThisInstance.LDAP.SearchBase))
                    {
                        defaultNamingContext = ldapConnection.GetRootDse().Attributes["defaultNamingContext"].First().ToString();
                    }
                    else
                    {
                        defaultNamingContext = Settings.ThisInstance.LDAP.SearchBase;
                    }

                    var ldapSearchResults = ldapConnection.Search(defaultNamingContext, filter, PropertiesToLoad, Native.LdapSearchScope.LDAP_SCOPE_SUB);

                    foreach (var ldapSearchResult in ldapSearchResults)
                    {
                        try
                        {
                            searchResult.Add(new ADComputer(ldapSearchResult.Attributes["cn"].First().ToString()));
                        }
                        catch (Exception ex)
                        {
                            m_log.Error(ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_log.Error(ex.Message);
            }

            return searchResult;
        }
    }

}
