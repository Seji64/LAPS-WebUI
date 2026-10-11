namespace LAPS_WebUI.Pages
{
    public partial class Logout
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Models.UserSession? session = await SessionManager.GetSessionAsync();
            if (session != null)
            {
                Serilog.Log.Information("User logged out: {Username}", session.Credential.UserName);
            }

            await SessionManager.LogoutAsync();

            await Task.Delay(500);
            NavigationManager.NavigateTo("/");
        }
    }
}
