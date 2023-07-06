using LAPS_WebUI.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LAPS_WebUI.Components
{
    public partial class LapsInformationDetail : MudComponentBase
    {
        [Parameter] public LapsInformation? LapsInfo { get; set; }
    }
}
