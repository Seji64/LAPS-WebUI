namespace LAPS_WebUI.Pages
{
    public partial class Logout
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (await SessionManager.IsUserLoggedInAsync())
            {
                await SessionManager.LogoutAsync();
            }

            await Task.Delay(500);
            NavigationManager.NavigateTo("/");
        }
    }
}
