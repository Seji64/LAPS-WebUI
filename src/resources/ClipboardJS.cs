using Integrative.Lara;
namespace LAPS_WebUI.resources
{
    internal static class ClipboardJS
    {
        public static void AppendTo(Element head)
        {
            head.AppendChild(new Script
            {
                Src = "/resources/clipboardjs/js/clipboard.min.js",
                Defer = true
            });
        }
    }
}
