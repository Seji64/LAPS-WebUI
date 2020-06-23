using Integrative.Lara;
namespace LAPS_WebUI.Ressources
{
    internal static class ClipboardJS
    {
        public static void AppendTo(Element head)
        {
            head.AppendChild(new Script
            {
                Src = "/ressources/clipboardjs/js/clipboard.min.js",
                Defer = true
            });
        }
    }
}
