using Integrative.Lara;
using NLog;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;

namespace LAPS_WebUI.WebServices
{
    [LaraWebService(Address = "/search", Method = "GET")]
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
                var rootDSE = new DirectoryEntry(string.Format("LDAP://{0}:{1}/rootDSE", Settings.ThisInstance.LDAP.Server,Settings.ThisInstance.LDAP.Port), UserSession.loginData.Username, UserSession.loginData.Password, Settings.ThisInstance.LDAP.UseSSL ? AuthenticationTypes.SecureSocketsLayer : AuthenticationTypes.None);
                var defaultNamingContext = rootDSE.Properties["defaultNamingContext"].Value.ToString();

                using DirectoryEntry domainEntry = new DirectoryEntry(string.Format("LDAP://{0}:{1}/{2}", Settings.ThisInstance.LDAP.Server, Settings.ThisInstance.LDAP.Port, defaultNamingContext), UserSession.loginData.Username, UserSession.loginData.Password, Settings.ThisInstance.LDAP.UseSSL ? AuthenticationTypes.SecureSocketsLayer : AuthenticationTypes.None);
                var filter = string.Format("(&(objectCategory=computer)(name={0}*))", searchTerm);
                using var dirSearch = new DirectorySearcher(domainEntry, filter);
                var dirSearchResult = dirSearch.FindAll();

                if (dirSearchResult != null)
                {

                    Parallel.ForEach(dirSearchResult.Cast<System.DirectoryServices.SearchResult>(), (entry) =>
                    {
                        searchResult.Add(new ADComputer(entry.Properties["cn"][0].ToString()));
                    });
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
