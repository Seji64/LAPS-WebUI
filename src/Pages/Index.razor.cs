namespace LAPS_WebUI.Pages
{
    public partial class Index
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // redirect to home if already logged in
            if (await SessionManager.IsUserLoggedInAsync())
            {
                NavigationManager.NavigateTo("/laps");
            }
            else
            {
                NavigationManager.NavigateTo("/login");
            }
        }
    }
}
