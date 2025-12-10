namespace LAPS_WebUI.Pages
{
    public partial class Logout
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (await SessionManager.IsUserLoggedInAsync())
            {
                string username = await SessionManager.GetUsernameAsync();
                Serilog.Log.Information("User logged out: {Username}", username);
                await SessionManager.LogoutAsync();
            }

            await Task.Delay(500);
            NavigationManager.NavigateTo("/");
        }
    }
}
