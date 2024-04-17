using LAPS_WebUI.Dialogs;
using LAPS_WebUI.Enums;
using LAPS_WebUI.Models;
using MudBlazor;
using Serilog;

namespace LAPS_WebUI.Pages
{
    public partial class LAPS
    {
        private readonly Dictionary<string, MudTabs?> MudTabsDict = [];
        private MudAutocomplete<ADComputer>? AutoCompleteSearchBox;
        private bool Authenticated { get; set; } = true;
        private LdapForNet.LdapCredential? LdapCredential { get; set; }
        private List<ADComputer> SelectedComputers { get; set; } = [];
        private string? DomainName { get; set; }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Authenticated = await sessionManager.IsUserLoggedInAsync();

            if (!Authenticated)
            {
                NavigationManager.NavigateTo("/login");
            }

            if (firstRender && Authenticated)
            {
                LdapCredential = await sessionManager.GetLdapCredentialsAsync();
                DomainName = await sessionManager.GetDomainAsync();
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task OnSelectedItemChangedAsync(ADComputer value)
        {
            if (value != null && !string.IsNullOrEmpty(value.Name) && !SelectedComputers.Exists(x => x.Name == value.Name))
            {
                AutoCompleteSearchBox?.Clear();
                MudTabsDict.Add(value.Name, null);
                await FetchComputerDetailsAsync(value.DistinguishedName, value.Name);
            }
        }

        private async Task ClearLapsPassword(ADComputer computer)
        {
            try
            {

                MudTabsDict.TryGetValue(computer.Name, out MudTabs? _tab);

                if (_tab != null && computer.LAPSInformations != null)
                {
                    LAPSVersion version = LAPSVersion.v1;
                    bool encrypted = false;

                    if (_tab.ActivePanel.ID.ToString() == "v1")
                    {
                        version = LAPSVersion.v1;
                    }

                    if (_tab.ActivePanel.ID.ToString() == "v2")
                    {
                        version = LAPSVersion.v2;
                        encrypted = computer.LAPSInformations.Single(x => x.Version == LAPSVersion.v2 && x.IsCurrent).WasEncrypted;
                    }
                    var parameters = new DialogParameters { ["ContentText"] = $"Clear LAPS {version} Password on Computer '{computer.Name}' ?{Environment.NewLine}You have to invoke gpupdate /force on computer '{computer.Name}' in order so set a new LAPS password", ["CancelButtonText"] = "Cancel", ["ConfirmButtonText"] = "Clear", ["ConfirmButtonColor"] = Color.Error };
                    IDialogReference dialog = Dialog.Show<Confirmation>("Clear LAPS Password", parameters,new DialogOptions() { NoHeader = true });
                    DialogResult result = await dialog.Result;

                    if(!result.Canceled)
                    {
                        computer.LAPSInformations.Clear();
                        await InvokeAsync(StateHasChanged);
                        await LDAPService.ClearLapsPassword(DomainName ?? await sessionManager.GetDomainAsync(), LdapCredential ?? await sessionManager.GetLdapCredentialsAsync(), computer.DistinguishedName, version, encrypted);
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

        private async Task RefreshComputerDetailsAsync(ADComputer computer, bool supressNotify = false)
        {

            ADComputer? placeHolder = null;
            List<LapsInformation> backup = [];

            try
            {
                placeHolder = SelectedComputers.Single(x => x.Name == computer.Name);

                if (placeHolder.LAPSInformations != null)
                {
                    backup.AddRange(placeHolder.LAPSInformations);
                }

                placeHolder.LAPSInformations = null;
                await InvokeAsync(StateHasChanged);

                var tmp = await LDAPService.GetADComputerAsync(DomainName ?? await sessionManager.GetDomainAsync(), LdapCredential ?? await sessionManager.GetLdapCredentialsAsync(), computer.DistinguishedName);

                if (tmp != null)
                {
                    placeHolder.LAPSInformations = tmp.LAPSInformations;

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
                    placeHolder.LAPSInformations = backup;
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
            ADComputer? placeHolder = null;

            try
            {
                placeHolder = new ADComputer(distinguishedName, computerName);
                SelectedComputers.Add(placeHolder);
                await InvokeAsync(StateHasChanged);

                var AdComputerObject = await LDAPService.GetADComputerAsync(DomainName ?? await sessionManager.GetDomainAsync(), LdapCredential ?? await sessionManager.GetLdapCredentialsAsync(), distinguishedName);
                var selectedComputer = SelectedComputers.SingleOrDefault(x => x.Name == computerName);

                if (AdComputerObject != null && selectedComputer != null)
                {
                    selectedComputer.LAPSInformations = AdComputerObject.LAPSInformations;
                    selectedComputer.FailedToRetrieveLAPSDetails = AdComputerObject.FailedToRetrieveLAPSDetails;

                    await InvokeAsync(StateHasChanged);
                    MudTabsDict.TryGetValue(computerName, out MudTabs? _tab);

                    if (!selectedComputer.FailedToRetrieveLAPSDetails && _tab != null)
                    {
                        await InvokeAsync(StateHasChanged);
                        _tab.ActivatePanel(_tab.Panels.First(x => !x.Disabled));
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
            MudTabsDict.Remove(computerName);
            SelectedComputers.RemoveAll(x => x.Name == computerName);
        }

        private async Task<IEnumerable<ADComputer>> SearchAsync(string value)
        {
            List<ADComputer> searchResult = [];

            if (string.IsNullOrEmpty(value))
            {
                return [];
            }

            var tmp = await LDAPService.SearchADComputersAsync(DomainName ?? await sessionManager.GetDomainAsync(), LdapCredential ?? await sessionManager.GetLdapCredentialsAsync(), value);

            if (tmp != null)
            {
                searchResult.AddRange(tmp);
            }

            return searchResult;

        }
    }
}
