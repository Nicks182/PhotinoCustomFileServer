
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Net.NetworkInformation;

namespace PhotinoCustomFileServer
{
    public class CustomFileServer
    {
        public static WebApplication CreateStaticFileServer(CustomFileServerOptions options, out string baseUrl)
        {
            var builder = WebApplication
            .CreateBuilder(new WebApplicationOptions()
            {
                Args = options.Args,
            });

            int port = options.PortStart;

            // Try ports until available port is found
            while (IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(x => x.Port == port))
            {
                if (port > options.PortStart + options.PortRange)
                {
                    throw new SystemException($"Couldn't find open port within range {options.PortStart} - {options.PortStart + options.PortRange}.");
                }

                port++;
            }

            baseUrl = $"http://localhost:{port}";

            if (options.AllowLocalAccess)
            {
                builder.WebHost.UseUrls($"http://*:{port}");
            }
            else
            {
                builder.WebHost.UseUrls(baseUrl);
            }

            WebApplication app = builder.Build();


            EmbeddedFileProvider fp = _Get_FileProvider();

            app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fp });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fp,
            });


            return app;
        }

        private static EmbeddedFileProvider _Get_FileProvider()
        {
            return new EmbeddedFileProvider(
                assembly: typeof(CustomFileServer).Assembly,
                baseNamespace: "PhotinoCustomFileServer.wwwroot");
        }

    }

    public class CustomFileServerOptions
    {
        public string[] Args { get; set; }
        public int PortStart { get; set; }
        public int PortRange { get; set; }
        public string WebRootFolder { get; set; }

        public bool AllowLocalAccess { get; set; }
    }
}
