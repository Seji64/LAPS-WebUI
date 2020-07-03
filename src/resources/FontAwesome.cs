using Integrative.Lara;


namespace LAPS_WebUI.resources
{
    internal static class FontAwesome
    {
        public static void AppendTo(Element head)
        {
            head.AppendChild(new Link
            {
                Rel = "stylesheet",
                HRef = "/resources/fontawesome/css/all.min.css"
            });
        }
    }
}
