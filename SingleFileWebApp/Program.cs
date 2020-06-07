using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace SingleFileWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var pathToExe = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var pathToContentRoot = AppDomain.CurrentDomain.BaseDirectory;

            NLog.LayoutRenderers.LayoutRenderer.Register("startupdir", (logEvent) => pathToContentRoot);

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("allfile") { FileName = @"${startupdir}\logs\nlog-all-${shortdate}.log" };

            config.AddRuleForAllLevels(logfile);
            NLog.LogManager.Configuration = config;

            var logger = NLog.LogManager.GetCurrentClassLogger();

            logger.Trace($"Executable path: {pathToExe}");
            logger.Trace($"AppContext.BaseDirectory: {AppContext.BaseDirectory}");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            })
            .UseNLog();

            if ((!Debugger.IsAttached && args.Contains("--service")))
            {
                builder.UseWindowsService();
            }

            return builder;
        }
    }
}
