using LAPS_WebUI.Dialogs;
using LAPS_WebUI.Enums;
using LAPS_WebUI.Models;
using MudBlazor;
using Serilog;
using System.Net;

namespace LAPS_WebUI.Pages
{
    public partial class LAPS : IDisposable
    {
        private readonly Dictionary<string, MudTabs?> _mudTabsDict = [];
        private MudAutocomplete<AdComputer>? _autoCompleteSearchBox;
        private bool Authenticated { get; set; } = true;
        private List<AdComputer> SelectedComputers { get; set; } = [];
        private string? DomainName { get; set; }
        private string DateDisplayFormat { get; set; } = "dd.MM.yyyy HH:mm:ss";
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                UserSession? session = await SessionManager.GetSessionAsync();
                Authenticated = session != null;

                if (session is null)
                {
                    NavigationManager.NavigateTo("/login");
                }
                else
                {
                    DomainName = session.DomainName;
                    DateDisplayFormat = LdapService.GetDomains().Single(d => d.Name == DomainName).Laps.DateDisplayFormat;
                }

                StateHasChanged();
            }
        }

        // Looked up per action, so an expired or logged-out session stops working right away
        private async Task<LdapForNet.LdapCredential?> GetCredentialAsync()
        {
            UserSession? session = await SessionManager.GetSessionAsync();

            if (session is null)
            {
                Snackbar.Add("Your session has expired. Please log in again.", Severity.Warning);
                NavigationManager.NavigateTo("/login");
            }

            return session?.Credential;
        }

        private async Task OnSelectedItemChangedAsync(AdComputer? value)
        {
            if (value != null && _autoCompleteSearchBox != null && !string.IsNullOrEmpty(value.Name) && !SelectedComputers.Exists(x => x.Name == value.Name))
            {
                await _autoCompleteSearchBox.ClearAsync();
                await FetchComputerDetailsAsync(value.DistinguishedName, value.Name);
            }
        }

        private async Task ClearLapsPassword(AdComputer computer)
        {
            LdapForNet.LdapCredential? credential = await GetCredentialAsync();
            if (credential is null)
            {
                return;
            }

            try
            {

                _mudTabsDict.TryGetValue(computer.Name, out MudTabs? tab);

                if (tab != null && computer.LapsInformations != null)
                {
                    LAPSVersion version = tab.ActivePanel?.ID?.ToString() switch
                    {
                        "v1" => LAPSVersion.v1,
                        "v2" => LAPSVersion.v2,
                        _ => LAPSVersion.v1
                    };

                    string encodedName = WebUtility.HtmlEncode(computer.Name);
                    DialogParameters parameters = new() { ["ContentText"] = $"""
                        Clear LAPS {version} Password on Computer <code>{encodedName}</code>?
                        <p>You have to invoke '<code>gpupdate /force</code>' on <code>{encodedName}</code> in order to set a new LAPS password.</p>
                        """, ["CancelButtonText"] = "Cancel", ["ConfirmButtonText"] = "Clear", ["ConfirmButtonColor"] = Color.Error };
                    IDialogReference dialog = await Dialog.ShowAsync<Confirmation>("Clear LAPS Password", parameters,new DialogOptions());
                    DialogResult? result = await dialog.Result;

                    if(result is { Canceled: false })
                    {
                        computer.LapsInformations.Clear();
                        await InvokeAsync(StateHasChanged);
                        if (await LdapService.ClearLapsPassword(DomainName!,
                                credential,
                                computer.DistinguishedName, version))
                        {
                            Snackbar.Add($"LAPS {version} Password for computer '{computer.Name}' successfully cleared! - Please invoke 'gpupdate' on {computer.Name} to set a new LAPS Password", Severity.Success);
                            Log.ForContext("Audit", true)
                                .Information("LAPS password cleared for computer '{ComputerName}' (LAPS version: {LAPSVersion}) by user '{UserName}'", computer.Name, version, credential.UserName);
                        }
                        else
                        {
                            throw new Exception($"Failed to reset LAPS password for computer {computer.Name}");
                        }
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
            LdapForNet.LdapCredential? credential = await GetCredentialAsync();
            if (credential is null)
            {
                return;
            }

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

                AdComputer? tmp = await LdapService.GetAdComputerAsync(DomainName!, credential, computer.DistinguishedName);

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

                placeHolder?.LapsInformations = backup;

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
            LdapForNet.LdapCredential? credential = await GetCredentialAsync();
            if (credential is null)
            {
                return;
            }

            try
            {
                AdComputer placeHolder = new AdComputer(distinguishedName, computerName);
                SelectedComputers.Add(placeHolder);
                await InvokeAsync(StateHasChanged);

                AdComputer? adComputerObject = await LdapService.GetAdComputerAsync(DomainName!, credential, distinguishedName);
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
                        await tab.ActivatePanelAsync(tab.Panels.First(x => !x.Disabled));
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

        private async Task<IEnumerable<AdComputer>> SearchAsync(string? value, CancellationToken token)
        {
            if (string.IsNullOrEmpty(value))
            {
                return [];
            }

            LdapForNet.LdapCredential? credential = await GetCredentialAsync();
            if (credential is null)
            {
                return [];
            }

            return await LdapService.SearchAdComputersAsync(DomainName!, credential, value);

        }

        public void Dispose() => SelectedComputers.Clear();
    }
}
