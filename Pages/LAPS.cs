using Integrative.Lara;
using NLog;
using System;
using System.DirectoryServices;
using System.Threading.Tasks;
using LAPS_WebUI.Ressources;

namespace LAPS_WebUI
{
    [LaraPage(Address = "/laps")]

    class LAPS : IPage
    {

        private readonly LAPSData m_lapsdata = new LAPSData();

        public Task OnGet()
        {
    
            var thisDocument = LaraUI.Page.Document;

            if (!UserSession.LoggedIn)
            {
                LaraUI.Page.Navigation.Replace("/login");
            }
            else
            {

                LaraUI.Page.Session.SaveValue("loggedin", "true");

                Bootstrap.AppendTo(thisDocument.Head);
                Select2.AppendSelect2To(thisDocument.Head);
                ClipboardJS.AppendTo(thisDocument.Head);
                FontAwesome.AppendTo(thisDocument.Head);

                thisDocument.Head.AppendChild(new Script { Src = "/js/helper.js", Defer = true });

                var builder = new LaraBuilder(thisDocument.Body);

                builder

                    .Push("nav", "navbar navbar-dark bg-dark")
                        .Push("a", "navbar-brand")
                            .Attribute("href","#")
                            .InnerText("LAPS WebUI")
                        .Pop()
                        .Push("button", "btn btn-outline-success my-2 my-sm-0")
                            .InnerText("Logout")
                            .On("click",() =>
                            {
                                UserSession.LoggedIn = false;
                                LaraUI.Page.Navigation.Replace("/");
                            })
                        .Pop()
                    .Pop() //Navbar

                    .Push("div", "container mt-5")
                        .Push("div", "input-group")
                            .Push("div", "input-group-prepend")
                                .Push("span", "input-group-text")
                                    .Push("i", "fa fa-search")
                                        .FlagAttribute("aria-hidden", true)
                                    .Pop()
                                .Pop()
                            .Pop() //div prepend-input group
                            .Push("select", "mysearchbox form-control")
                                .Attribute("id", "mysearchbox")
                            .Pop()
                        .Pop() //End div
                        .Push("div", "card mt-5")
                            .BindToggleAttribute("hidden", m_lapsdata, () => m_lapsdata.selectedADComputer is null)
                            .Push("h5", "card-header")
                                .Push("p")
                                    .Attribute("id", "cardHeader")
                                    .BindInnerText(m_lapsdata, x => x.ComputerName)
                                .Pop()
                            .Pop() // card header
                            .Push("div", "card-body")
                                .Push("div", "input-group mb-3")
                                    .Push("div", "input-group-prepend")
                                        .Push("span", "input-group-text")
                                            .Push("i", "fa fa-key")
                                                .Attribute("data-toggle","tooltip")
                                                .Attribute("title","LAPS Password")
                                            .Pop()
                                        .Pop()
                                    .Pop()
                                    .Push("pre", "form-control")
                                        .Attribute("id","lapspw")
                                        .Push("code")
                                            .BindInnerText(m_lapsdata, x => x.LAPSPassword)
                                        .Pop()          
                                    .Pop()
                                    .Push("button", "btn btn-light clipboard-btn")
                                        .Attribute("id","clipboard-btn")
                                        .Attribute("data-clipboard-target", "#lapspw")
                                        .Push("i","fas fa-copy")
                                        .Pop()
                                    .Pop()
                                .Pop() //input group
                                .Push("div", "input-group mb-3")
                                    .Push("div", "input-group-prepend")
                                        .Push("span", "input-group-text")
                                            .Push("i", "fa fa-hourglass-end")
                                                .Attribute("data-toggle", "tooltip")
                                                .Attribute("title", "LAPS Password Expire Date")
                                            .Pop()
                                        .Pop()
                                    .Pop()
                                    .Push("pre", "form-control")
                                        .Attribute("id", "lapspwexpiredate")
                                        .Push("code")
                                            .BindInnerText(m_lapsdata, x => x.LAPSPasswordExpireDate.ToString())
                                        .Pop()
                                    .Pop()
                                .Pop() //input group
                                .Push("div", "alert alert-danger")
                                    .Attribute("role", "alert")
                                    .BindInnerText(m_lapsdata, x => x.ErrorMessage)
                                    .BindToggleAttribute("hidden", m_lapsdata, () => m_lapsdata.ErrorMessage == "_")
                                .Pop() //alert if failed to read LAPS PW
                            .Pop() //card body
                        .Pop() // div result
                    .Pop();
            }

            LaraUI.Page.JSBridge.Submit("$('[data-toggle=\"tooltip\"]').tooltip();");
            LaraUI.Page.JSBridge.Submit(@"var clipboard = new ClipboardJS('.clipboard-btn'); clipboard.on('success', function(e) { setTooltip(e.trigger, 'Copied!'); hideTooltip(e.trigger); }); clipboard.on('error', function(e) { setTooltip(e.trigger, 'Copied!'); hideTooltip(e.trigger); });");
            LaraUI.Page.JSBridge.Submit(@"$('.clipboard-btn').tooltip({ trigger:'click', placement:'bottom' });");
            LaraUI.Page.JSBridge.Submit(@"$('.mysearchbox').select2({ minimumInputLength: 4,placeholder: 'Computernamen eingeben', allowClear: true, theme: 'bootstrap4', ajax: { url: '/search' , dataType: 'json'} });");
            LaraUI.Page.JSBridge.Submit(@"var $eventSelect = $('.mysearchbox'); $eventSelect.on('select2:select', function (e) { LaraSelectedEventProxy('select2Selected', e); });");
            LaraUI.Page.JSBridge.AddMessageListener("select2Selected", select2SelectedHandler);


            return Task.CompletedTask;
        }

        private Task select2SelectedHandler(MessageEventArgs arg)
        {

            var rawdata = Newtonsoft.Json.Linq.JObject.Parse(arg.Body)["data"].ToString();
            var selectedComputername = Newtonsoft.Json.Linq.JObject.Parse(rawdata)["text"].ToString();

            m_lapsdata.ErrorMessage = "_";

            m_lapsdata.selectedADComputer = GetADComputerByName(selectedComputername);

            return Task.CompletedTask;
        }

        private ADComputer GetADComputerByName(string name)
        {

            Logger m_log = LogManager.GetLogger("GetADComputerByName");

            var rootDSE = new DirectoryEntry(string.Format("LDAP://{0}:{1}/rootDSE", Settings.ThisInstance.LDAP.Server, Settings.ThisInstance.LDAP.Port), UserSession.loginData.Username, UserSession.loginData.Password, Settings.ThisInstance.LDAP.UseSSL ? AuthenticationTypes.SecureSocketsLayer : AuthenticationTypes.None);
            var defaultNamingContext = rootDSE.Properties["defaultNamingContext"].Value.ToString();

            using DirectoryEntry domainEntry = new DirectoryEntry(string.Format("LDAP://{0}:{1}/{2}", Settings.ThisInstance.LDAP.Server, Settings.ThisInstance.LDAP.Port, defaultNamingContext), UserSession.loginData.Username, UserSession.loginData.Password, Settings.ThisInstance.LDAP.UseSSL ? AuthenticationTypes.SecureSocketsLayer : AuthenticationTypes.None);
            var filter = "(&(objectCategory=computer)(name=" + name + "))";

            using var dirSearch = new DirectorySearcher(domainEntry, filter);
            var dirSearchResult = dirSearch.FindOne();

            if (null != dirSearchResult)
            {
                ADComputer rt = null;

                try
                {
                    rt = new ADComputer(dirSearchResult.Properties["cn"][0].ToString());

                    if (dirSearchResult.Properties.Contains("ms-Mcs-AdmPwd") && dirSearchResult.Properties["ms-Mcs-AdmPwd"].Count != 0)
                    {
                        rt.LAPSPassword = dirSearchResult.Properties["ms-Mcs-AdmPwd"][0].ToString();
                        rt.LAPSPasswordExpireDate = DateTime.FromFileTime(Convert.ToInt64(dirSearchResult.Properties["ms-MCS-AdmPwdExpirationTime"][0].ToString()));
                    }
                    else
                    {
                        throw new Exception("No permission to retrieve LAPS Password");
                    }

                }
                catch (Exception ex)
                {
                    rt.LAPSPassword = "N/A";
                    m_lapsdata.ErrorMessage = ex.Message;
                    m_log.Error(ex.Message);
                }

                return rt;

            }
            else
            {
                return null;
            }
        }

    }
}
