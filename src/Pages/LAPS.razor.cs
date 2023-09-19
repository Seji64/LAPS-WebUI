using LAPS_WebUI.Models;
using MudBlazor;

namespace LAPS_WebUI.Pages
{
    public partial class LAPS
    {
        private readonly Dictionary<string, MudTabs?> MudTabsDict = new();
        private MudAutocomplete<ADComputer>? AutoCompleteSearchBox;
        private bool Authenticated { get; set; } = true;
        private LdapForNet.LdapCredential? LdapCredential { get; set; }
        private List<ADComputer> SelectedComputers { get; set; } = new List<ADComputer>();
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
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task OnSelectedItemChangedAsync(ADComputer value)
        {
            if (value != null && !string.IsNullOrEmpty(value.Name) && !SelectedComputers.Exists(x => x.Name == value.Name))
            {
                AutoCompleteSearchBox?.Clear();
                MudTabsDict.Add(value.Name, null);
                await FetchComputerDetailsAsync(value.Name);
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

                var AdComputerObject = await LDAPService.GetADComputerAsync(await sessionManager.GetLdapCredentialsAsync(), computerName);
                var selectedComputer = SelectedComputers.SingleOrDefault(x => x.Name == computerName);

                if (AdComputerObject != null && selectedComputer != null)
                {
                    selectedComputer.LAPSInformations = AdComputerObject.LAPSInformations;
                    selectedComputer.FailedToRetrieveLAPSDetails = AdComputerObject.FailedToRetrieveLAPSDetails;

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

            var tmp = await LDAPService.SearchADComputersAsync(LdapCredential ?? await sessionManager.GetLdapCredentialsAsync(), value);

            if (tmp != null)
            {
                searchResult.AddRange(tmp);
            }

            return searchResult;

        }
    }
}
