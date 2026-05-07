using LAPS_WebUI.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LAPS_WebUI.Components
{
    public partial class LapsInformationDetail : MudComponentBase
    {
        [Parameter] public LapsInformation? LapsInfo { get; set; }
        [Parameter] public MudTabs? MudTab { get; set; }
        private bool IsCopyToClipboardSupported { get; set; }
        private List<Domain> domains = [];
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                IsCopyToClipboardSupported = await Clipboard.IsSupportedAsync();
            }
        }

        protected override async Task OnInitializedAsync()
        {
            domains = await LdapService.GetDomainsAsync();
        }

        private bool IsCopyButtonDisabled()
        {
            return !IsCopyToClipboardSupported || LapsInfo is null || string.IsNullOrEmpty(LapsInfo.Password);
        }
        private async Task CopyLapsPasswordToClipboardAsync()
        {
            if (LapsInfo != null && !string.IsNullOrEmpty(LapsInfo.Password))
            {
                await Clipboard.WriteTextAsync(LapsInfo.Password);
                Snackbar.Add("Copied password to clipboard!", Severity.Success);
            }
            else
            {
                Snackbar.Add("Failed to copy password to clipboard!", Severity.Error);
            }
        }

        private string GetLapsDateDisplayFormat(DateTime? date)
        { 
            string format = domains.Single(x => x.Name == LapsInfo!.DomainName).Laps.DateDisplayFormat;
            return date.HasValue ? date.Value.ToString(format) : string.Empty;
        }
    }
}
