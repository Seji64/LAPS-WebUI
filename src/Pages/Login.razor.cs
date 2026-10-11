using LAPS_WebUI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;

namespace LAPS_WebUI.Pages
{
    public partial class Login
    {
        private readonly UserLoginRequest _loginRequest = new();
        private bool _processing;
        private string _errorMessage = string.Empty;
        private List<string> _domains = [];

        [CascadingParameter(Name = "RemoteIpAddress")] private string? RemoteIpAddress { get; set; }

        protected override void OnInitialized()
        {
            _domains = SessionManager.GetDomains();

            if (_domains.Count > 0)
            {
                _loginRequest.DomainName = _domains[0];
            }
        }

        private async Task OnValidSubmitAsync(EditContext context)
        {
            _errorMessage = string.Empty;
            _processing = true;
            try
            {
                if (LoginThrottle.IsBlocked(RemoteIpAddress))
                {
                    Log.ForContext("Audit",true).Warning("Login for user '{Username}' from '{RemoteIpAddress}' blocked: too many failed attempts.", _loginRequest.Username, RemoteIpAddress);
                    _errorMessage = "Too many failed login attempts. Please try again later.";
                    return;
                }

                if (await SessionManager.LoginAsync(_loginRequest.DomainName ?? string.Empty, _loginRequest.Username ?? string.Empty, _loginRequest.Password ?? string.Empty))
                {
                    Log.ForContext("Audit",true).Information("User '{Username}' from '{RemoteIpAddress}' successfully logged in.", _loginRequest.Username, RemoteIpAddress);
                    NavigationManager.NavigateTo("/laps");
                }
                else
                {
                    LoginThrottle.RegisterFailure(RemoteIpAddress);
                    Log.ForContext("Audit",true).Warning("User '{Username}' from '{RemoteIpAddress}' failed logged in.", _loginRequest.Username, RemoteIpAddress);
                    _errorMessage = "Login failed!";
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            finally
            {
                _processing = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
