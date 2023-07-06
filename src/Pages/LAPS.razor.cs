using LAPS_WebUI.Models;
using MudBlazor;

namespace LAPS_WebUI.Pages
{
    public partial class LAPS
    {
        private bool IsCopyToClipboardSupported { get; set; }

        private bool Authenticated { get; set; } = true;

        private LdapForNet.LdapCredential? LdapCredential { get; set; }

        private List<ADComputer> SelectedComputers { get; set; } = new List<ADComputer>();

        private MudTabs? _tabs;

        readonly Func<ADComputer, string> _ADComputerToStringConverter = p => (p is null ? string.Empty : p.Name);

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Authenticated = await sessionManager.IsUserLoggedInAsync();

            if (!Authenticated)
            {
                NavigationManager.NavigateTo("/login");
            }

            if (firstRender)
            {
                if (Authenticated)
                {
                    LdapCredential = await sessionManager.GetLdapCredentialsAsync();
                }

                IsCopyToClipboardSupported = await clipboard.IsSupportedAsync();
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task OnSelectedItemChangedAsync(ADComputer value)
        {
            if (value != null && !string.IsNullOrEmpty(value.Name) && !SelectedComputers.Any(x => x.Name == value.Name))
            {
                await FetchComputerDetailsAsync(value.Name);
            }
        }

        private async Task CopyLAPSPasswordToClipboardAsync(ADComputer computer)
        {
            string password = string.Empty;

            if (_tabs != null && _tabs.ActivePanel != null)
            {
                if (_tabs.ActivePanel.ID.ToString() == "v1") // aka LAPS v1
                {
                    password = computer.LAPSInformations!.Single(x => x.IsCurrent && x.Version == Enums.LAPSVersion.v1).Password!;
                }
                if (_tabs.ActivePanel.ID.ToString() == "v2") // aka LAPS v2
                {
                    password = computer.LAPSInformations!.Single(x => x.IsCurrent && x.Version == Enums.LAPSVersion.v2).Password!;
                }
            }

            if (!string.IsNullOrEmpty(password))
            {
                await clipboard.WriteTextAsync(password);
                Snackbar.Add("Copied password to clipboard!", Severity.Success);
            }
            else
            {
                Snackbar.Add("Failed to copy password to clipboard!", Severity.Error);
            }



        }

        private async Task RefreshComputerDetailsAsync(string computerName)
        {

            ADComputer? placeHolder = null;
            List<LapsInformation> backup = new();

            try
            {
                placeHolder = SelectedComputers.Single(x => x.Name == computerName);

                if (placeHolder.LAPSInformations != null)
                {
                    backup.AddRange(placeHolder.LAPSInformations);
                }

                placeHolder.LAPSInformations = null;
                await InvokeAsync(StateHasChanged);

                var tmp = await LDAPService.GetADComputerAsync(await sessionManager.GetLdapCredentialsAsync(), computerName);

                if (tmp != null)
                {
                    placeHolder.LAPSInformations = tmp.LAPSInformations;
                    Snackbar.Add($"LAPS data for computer {computerName} successfully refreshed!", Severity.Success);
                }
            }
            catch (Exception)
            {
                if (placeHolder != null)
                {
                    placeHolder.LAPSInformations = backup;
                }
                Snackbar.Add($"Failed to refresh LAPS data for computer {computerName}", Severity.Error);
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task FetchComputerDetailsAsync(string computerName)
        {
            ADComputer? placeHolder = null;

            try
            {
                placeHolder = new ADComputer(computerName);
                SelectedComputers.Add(placeHolder);
                await InvokeAsync(StateHasChanged);

                var tmp = await LDAPService.GetADComputerAsync(await sessionManager.GetLdapCredentialsAsync(), computerName);

                if (tmp != null)
                {
                    SelectedComputers.Single(x => x.Name == computerName).LAPSInformations = tmp.LAPSInformations;
                }
            }
            catch (Exception ex)
            {
                SelectedComputers.RemoveAll(x => x.Name == computerName);
                Snackbar.Add($"Failed to fetch LAPS data for computer {computerName}\nError: {ex.Message}", Severity.Error);
            }
        }

        private void RemoveComputerCard(string computerName)
        {
            SelectedComputers.RemoveAll(x => x.Name == computerName);
        }

        private async Task<IEnumerable<ADComputer>> SearchAsync(string value)
        {
            List<ADComputer> searchResult = new();

            if (string.IsNullOrEmpty(value))
            {
                return new List<ADComputer>();
            }

            var tmp = (await LDAPService.SearchADComputersAsync(LdapCredential ?? await sessionManager.GetLdapCredentialsAsync(), value));

            if (tmp != null)
            {
                searchResult.AddRange(tmp);
            }

            return searchResult;

        }
    }
}
