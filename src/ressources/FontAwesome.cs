using Integrative.Lara;


namespace LAPS_WebUI.Ressources
{
    internal static class FontAwesome
    {
        public static void AppendTo(Element head)
        {
            head.AppendChild(new Link
            {
                Rel = "stylesheet",
                HRef = "/ressources/fontawesome/css/all.min.css"
            });
        }
    }
}
