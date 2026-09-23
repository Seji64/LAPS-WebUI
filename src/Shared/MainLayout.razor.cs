using MudBlazor.Utilities;
using MudBlazor;

namespace LAPS_WebUI.Shared
{
    public partial class MainLayout : IDisposable
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

        protected override void OnInitialized()
        {
            NavigationManager.LocationChanged += OnLocationChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                IsUserLoggedIn = await SessionManager.IsUserLoggedInAsync();
                _isDarkMode = await _mudThemeProvider.GetSystemDarkModeAsync();
                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            IsUserLoggedIn = await SessionManager.IsUserLoggedInAsync();
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}
