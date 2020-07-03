using Integrative.Lara;

namespace LAPS_WebUI.resources
{
    internal static class Bootstrap
    {
        public static void AppendTo(Element head)
        {
            head.AppendChild(new Link
            {
                Rel = "stylesheet",
                HRef = "/resources/bootstrap/css/bootstrap.min.css"
            });
            head.AppendChild(new Script
            {
                Src = "/resources/bootstrap/js/popper.min.js",
                Defer = true
            });
            head.AppendChild(new Script
            {
                Src = "/resources/bootstrap/js/bootstrap.min.js",
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
