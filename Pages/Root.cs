using Integrative.Lara;
using System.Threading.Tasks;

namespace LAPS_WebUI
{
    [LaraPage(Address = "/")]
    class Root : IPage
    {
        public Task OnGet()
        {

            if (!UserSession.LoggedIn)
            {
                LaraUI.Page.Navigation.Replace("/login");
            }
            else
            {
                LaraUI.Page.Navigation.Replace("/laps");
            }

            return Task.CompletedTask;

        }
    }
}
