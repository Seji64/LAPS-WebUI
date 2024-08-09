using LAPS_WebUI.Dialogs;
using LAPS_WebUI.Enums;
using LAPS_WebUI.Models;
using MudBlazor;
using Serilog;

namespace LAPS_WebUI.Pages
{
    public partial class LAPS : IDisposable
    {
        private readonly Dictionary<string, MudTabs?> _mudTabsDict = [];
        private MudAutocomplete<AdComputer>? _autoCompleteSearchBox;
        private bool _disposedValue;
        private bool Authenticated { get; set; } = true;
        private LdapForNet.LdapCredential? LdapCredential { get; set; }
        private List<AdComputer> SelectedComputers { get; set; } = [];
        private string? DomainName { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Authenticated = await SessionManager.IsUserLoggedInAsync();

            if (!Authenticated)
            {
                NavigationManager.NavigateTo("/login");
            }

            if (firstRender && Authenticated)
            {
                LdapCredential = await SessionManager.GetLdapCredentialsAsync();
                DomainName = await SessionManager.GetDomainAsync();
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task OnSelectedItemChangedAsync(AdComputer value)
        {
            if (value != null && _autoCompleteSearchBox != null && !string.IsNullOrEmpty(value.Name) && !SelectedComputers.Exists(x => x.Name == value.Name))
            {
                await _autoCompleteSearchBox.ClearAsync();
                _mudTabsDict.Add(value.Name, null);
                await FetchComputerDetailsAsync(value.DistinguishedName, value.Name);
            }
        }

        private async Task ClearLapsPassword(AdComputer computer)
        {
            try
            {

                _mudTabsDict.TryGetValue(computer.Name, out MudTabs? tab);

                if (tab != null && computer.LapsInformations != null)
                {
                    LAPSVersion version = tab.ActivePanel.ID.ToString() switch
                    {
                        "v1" => LAPSVersion.v1,
                        "v2" => LAPSVersion.v2,
                        _ => LAPSVersion.v1
                    };

                    DialogParameters parameters = new DialogParameters { ["ContentText"] = $"Clear LAPS {version} Password on Computer '{computer.Name}' ?{Environment.NewLine}You have to invoke gpupdate /force on computer '{computer.Name}' in order so set a new LAPS password", ["CancelButtonText"] = "Cancel", ["ConfirmButtonText"] = "Clear", ["ConfirmButtonColor"] = Color.Error };
                    IDialogReference dialog = await Dialog.ShowAsync<Confirmation>("Clear LAPS Password", parameters,new DialogOptions() { NoHeader = true });
                    DialogResult? result = await dialog.Result;

                    if(result is { Canceled: false })
                    {
                        computer.LapsInformations.Clear();
                        await InvokeAsync(StateHasChanged);
                        await LdapService.ClearLapsPassword(DomainName ?? await SessionManager.GetDomainAsync(), LdapCredential ?? await SessionManager.GetLdapCredentialsAsync(), computer.DistinguishedName, version);
                        Snackbar.Add($"LAPS {version} Password for computer '{computer.Name}' successfully cleared! - Please invoke gpupdate on {computer.Name} to set a new LAPS Password", Severity.Success);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("{ErrorMessage}", ex.Message);
                Snackbar.Add($"Failed to reset LAPS password for computer {computer.Name}", Severity.Error);
            }
            finally
            {
                await RefreshComputerDetailsAsync(computer,true);
            }
        }

        private async Task RefreshComputerDetailsAsync(AdComputer computer, bool supressNotify = false)
        {

            AdComputer? placeHolder = null;
            List<LapsInformation> backup = [];

            try
            {
                placeHolder = SelectedComputers.Single(x => x.Name == computer.Name);

                if (placeHolder.LapsInformations != null)
                {
                    backup.AddRange(placeHolder.LapsInformations);
                }

                placeHolder.LapsInformations = null;
                await InvokeAsync(StateHasChanged);

                AdComputer? tmp = await LdapService.GetAdComputerAsync(DomainName ?? await SessionManager.GetDomainAsync(), LdapCredential ?? await SessionManager.GetLdapCredentialsAsync(), computer.DistinguishedName);

                if (tmp != null)
                {
                    placeHolder.LapsInformations = tmp.LapsInformations;

                    if (!supressNotify)
                    {
                        Snackbar.Add($"LAPS data for computer {computer.Name} successfully refreshed!", Severity.Success);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Log.Error("{ErrorMessage}", ex.Message);

                if (placeHolder != null)
                {
                    placeHolder.LapsInformations = backup;
                }

                if (!supressNotify)
                {
                    Snackbar.Add($"Failed to refresh LAPS data for computer {computer.Name}", Severity.Error);
                }
                    
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task FetchComputerDetailsAsync(string distinguishedName, string computerName)
        {
            try
            {
                AdComputer placeHolder = new AdComputer(distinguishedName, computerName);
                SelectedComputers.Add(placeHolder);
                await InvokeAsync(StateHasChanged);

                AdComputer? adComputerObject = await LdapService.GetAdComputerAsync(DomainName ?? await SessionManager.GetDomainAsync(), LdapCredential ?? await SessionManager.GetLdapCredentialsAsync(), distinguishedName);
                AdComputer? selectedComputer = SelectedComputers.SingleOrDefault(x => x.Name == computerName);

                if (adComputerObject != null && selectedComputer != null)
                {
                    selectedComputer.LapsInformations = adComputerObject.LapsInformations;
                    selectedComputer.FailedToRetrieveLapsDetails = adComputerObject.FailedToRetrieveLapsDetails;

                    await InvokeAsync(StateHasChanged);
                    _mudTabsDict.TryGetValue(computerName, out MudTabs? tab);

                    if (!selectedComputer.FailedToRetrieveLapsDetails && tab != null)
                    {
                        await InvokeAsync(StateHasChanged);
                        tab.ActivatePanel(tab.Panels.First(x => !x.Disabled));
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error("{ErrorMessage}", ex.Message);
                SelectedComputers.RemoveAll(x => x.Name == computerName);
                Snackbar.Add($"Failed to fetch LAPS data for computer {computerName}\nError: {ex.Message}", Severity.Error);
            }
        }

        private void RemoveComputerCard(string computerName)
        {
            _mudTabsDict.Remove(computerName);
            SelectedComputers.RemoveAll(x => x.Name == computerName);
        }

        private async Task<IEnumerable<AdComputer>> SearchAsync(string value,CancellationToken token)
        {
            List<AdComputer> searchResult = [];
            if (string.IsNullOrEmpty(value))
            {
                return [];
            }
            List<AdComputer> tmp = await LdapService.SearchAdComputersAsync(DomainName ?? await SessionManager.GetDomainAsync(), LdapCredential ?? await SessionManager.GetLdapCredentialsAsync(), value);
            searchResult.AddRange(tmp);
            return searchResult;

        }

        protected virtual void Dispose(bool disposing)
        {
            // check if already disposed
            if (_disposedValue) return;
            if (disposing)
            {
                // free managed objects here
                SelectedComputers.Clear();
            }
            
            // set the bool value to true
            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
