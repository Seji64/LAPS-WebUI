using MudBlazor.Utilities;
using MudBlazor;

namespace LAPS_WebUI.Shared
{
    public partial class MainLayout
    {

        private bool _isDarkMode;
        private MudThemeProvider _mudThemeProvider = new();
        private bool IsUserLoggedIn { get; set; } = false;
        private readonly MudTheme _myCustomTheme = new()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = new MudColor("#455FAC"),
                Secondary = new MudColor("#CE3C3C"),
                AppbarBackground = new MudColor("#455FAC"),
                DrawerText = new MudColor("#D7D7D9"),
                DrawerIcon = new MudColor("#D7D7D9"),
                DrawerBackground = new MudColor("#3C3D3F")
            }
        };

        private void ToggleDarkMode()
        {
            _isDarkMode = !_isDarkMode;
        }

        private void Logout()
        {
            NavigationManager.NavigateTo("/logout");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            IsUserLoggedIn = await sessionManager.IsUserLoggedInAsync();

            if (firstRender)
            {
                _isDarkMode = await _mudThemeProvider.GetSystemPreference();
            }

            await InvokeAsync(StateHasChanged);
            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
