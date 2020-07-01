using Integrative.Lara;

namespace LAPS_WebUI.Ressources
{
    internal static class Select2
    {
        public static void AppendSelect2To(Element head)
        {
            head.AppendChild(new Script
            {
                Src = "/ressources/select2/js/select2.min.js",
                Defer = true
            });
            head.AppendChild(new Link
            {
                Rel = "stylesheet",
                HRef = "/ressources/select2/css/select2.min.css"
            });

            head.AppendChild(new Link
            {
                Rel = "stylesheet",
                HRef = "/ressources/select2/css/select2-bootstrap4.css"
            });
        }
    }
}
