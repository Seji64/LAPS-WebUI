using Integrative.Lara;

namespace LAPS_WebUI.Ressources
{
    internal static class Bootstrap
    {
        public static void AppendTo(Element head)
        {
            head.AppendChild(new Link
            {
                Rel = "stylesheet",
                HRef = "/ressources/bootstrap/css/bootstrap.min.css"
            });
            head.AppendChild(new Script
            {
                Src = "/ressources/bootstrap/js/popper.min.js",
                Defer = true
            });
            head.AppendChild(new Script
            {
                Src = "/ressources/bootstrap/js/bootstrap.min.js",
                Defer = true
            });

            head.AppendChild(new Meta
            {
                Name = "viewport",
                Content = "width=device-width, initial-scale=1, shrink-to-fit=no"
            });
        }
    }
}
