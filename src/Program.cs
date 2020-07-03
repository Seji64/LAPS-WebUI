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

                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(currentDir)
                    .AddJsonFile("settings.json", true, false)
                    .AddEnvironmentVariables(prefix: "LAPS_");

                IConfigurationRoot configuration = configBuilder.Build();
                var settings = new Settings();
                configuration.Bind(settings);

                if (settings.LDAP is null || string.IsNullOrWhiteSpace(settings.LDAP.Server))
                {
                    throw new Exception("LDAP Server cannot be empty or null!");
                }

                await app.Start(new StartServerOptions
                {
                    Mode = ApplicationMode.Default,
                    PublishAssembliesOnStart = true,
                    Port = settings.ListenPort,
                    IPAddress = IPAddress.Parse(settings.ListenAddress),
                });

                m_log.Info("Publishing resources....");

                List<string> allowedFileTypes = new List<string>() { ".js", ".css", ".svg", ".woff", ".woff2", ".ttf", ".png" , ".json" };

                foreach(var file in Directory.GetFiles(Path.Combine(currentDir, "resources"), "*.*", SearchOption.AllDirectories).Where(x => allowedFileTypes.Contains(new FileInfo(x).Extension.ToLower()) == true )){

                    var fileInfo = new System.IO.FileInfo(file);
                    var bytes = File.ReadAllBytes(file);
                    var contentType = ContentTypes.TextHtml;

                    switch (fileInfo.Extension.ToLower())
                    {
                        case ".js":
                            contentType = ContentTypes.ApplicationJavascript;
                            break;

                        case ".json":
                            contentType = ContentTypes.ApplicationJson;
                            break;

                        case ".png":
                            contentType = ContentTypes.ImagePng;
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
                    var publishDir = fileInfo.DirectoryName.Remove(0, fileInfo.DirectoryName.IndexOf("resources"));
                    publishDir = publishDir.Replace(Path.DirectorySeparatorChar, '/');

                    var publishPath = string.Format("/{0}/{1}", publishDir, fileInfo.Name);

                    //ServiceWorker must be published at root - this is needed for PWA
                    if (fileInfo.Name == "sw.js")
                    {
                        publishPath = "/sw.js";
                    }
                    
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

                m_log.Info("Configured LDAP Server: {0} | LDAP Port: {1} | SSL: {2} | SearchBase: {3}", settings.LDAP.Server, settings.LDAP.Port, settings.LDAP.UseSSL, string.IsNullOrWhiteSpace(settings.LDAP.SearchBase) ? "default" : settings.LDAP.SearchBase);

                await app.WaitForShutdown();

            }
            catch(Exception ex)
            {
                m_log.Error(ex.Message);
            }

        }
    }
}
