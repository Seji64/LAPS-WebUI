using Integrative.Lara;
using LAPS_WebUI.WebServices;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace LAPS_WebUI
{
    static class Program
    {
        static async Task Main()
        {
            Logger m_log = LogManager.GetLogger("Main");

            using var app = new Application();
            string currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            try
            {

                if (!File.Exists(System.IO.Path.Combine(currentDir, "settings.json"))){
                    throw new FileNotFoundException("settings.json not found!");
                }

                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(currentDir)
                    .AddJsonFile("settings.json", true)
                    .AddEnvironmentVariables();

                IConfigurationRoot configuration = configBuilder.Build();
                var settings = new Settings();
                configuration.Bind(settings);

                await app.Start(new StartServerOptions
                {
                    Mode = ApplicationMode.Default,
                    PublishAssembliesOnStart = true,
                    Port = settings.ListenPort,
                    IPAddress = IPAddress.Parse(settings.ListenAddress)

                });

                m_log.Info("Publishing ressources....");

                List<string> allowedFileTypes = new List<string>() { ".js", ".css", ".svg", ".woff", ".woff2", ".ttf" };

                foreach(var file in Directory.GetFiles(Path.Combine(currentDir, "ressources"), "*.*", SearchOption.AllDirectories).Where(x => allowedFileTypes.Contains(new FileInfo(x).Extension.ToLower()) == true )){

                    var fileInfo = new System.IO.FileInfo(file);
                    var bytes = File.ReadAllBytes(file);
                    var contentType = ContentTypes.TextHtml;

                    switch (fileInfo.Extension.ToLower())
                    {
                        case ".js":
                            contentType = ContentTypes.ApplicationJavascript;
                            break;

                        case ".css":
                            contentType = ContentTypes.TextCss;
                            break;

                        case ".svg":
                            contentType = ContentTypes.ImageSvgXml;
                            break;

                        case ".woff2":

                        case ".woff":
                            contentType = ContentTypes.ApplicationFontWoff;
                            break;

                        case ".ttf":
                            contentType = ContentTypes.TextXml;
                            break;

                        default:
                            m_log.Warn("Unknown file ==> {0}", fileInfo.Name);
                            break;
                    }

                    var content = new StaticContent(bytes, contentType);
                    var publishDir = fileInfo.DirectoryName.Remove(0, fileInfo.DirectoryName.IndexOf("ressources"));
                    publishDir = publishDir.Replace(Path.DirectorySeparatorChar, '/');

                    var publishPath = string.Format("/{0}/{1}", publishDir, fileInfo.Name);

                    m_log.Debug("Publishing file {0}", publishPath);

                    app.PublishFile(publishPath, content);
                }

                m_log.Info("Done!");
;
                m_log.Info("Publishing services...");

                app.PublishService(new WebServiceContent
                {
                    Address = "/search",
                    Factory = () => new SearchService()

                });

                m_log.Info("Done!");

                m_log.Info("LAPS WEB UI is up and running!");
                m_log.Info("Web Server is listening on {0}:{1}", settings.ListenAddress, settings.ListenPort);

                await app.WaitForShutdown();

            }
            catch(Exception ex)
            {
                m_log.Error(ex.Message);
            }

        }
    }
}
