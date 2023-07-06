namespace LAPS_WebUI.Pages
{
    public partial class Logout
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (await sessionManager.IsUserLoggedInAsync())
            {
                await sessionManager.LogoutAsync();
            }

            await Task.Delay(500);
            NavigationManager.NavigateTo("/");
        }
    }
}
