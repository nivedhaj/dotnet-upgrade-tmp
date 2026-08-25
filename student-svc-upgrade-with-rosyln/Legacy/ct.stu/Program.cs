using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ct.stu.tests")]

namespace ct
{
    /// <summary>
    /// Runtime call into Program to statrt the service
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Method that gets called first
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Console.WriteLine(Process.GetCurrentProcess().Id);
            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
            var host_builder = WebHost.CreateDefaultBuilder();
            var env_name = host_builder.GetSetting("environment");//Development | Staging | Production
            var config = buildConfig(args, env_name);


            // var host = host_builder.UseConfiguration(config).UseStartup<Startup>().Build();
            var host = host_builder
                       .UseConfiguration(config)
                       .ConfigureServices(o => o.AddAutofac()) //allows for ConfigureContainer to be supported in Startup with a strongly-typed ContainerBuilder.
                       .UseStartup<Startup>()
                       .ConfigureLogging(logging => {
                           logging.ClearProviders();
                           logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                       })
                           .UseNLog()
                       .Build();
            host.Run();
        }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }
        static IConfigurationRoot buildConfig(string[] args, string env_name)
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env_name}.json", optional: false, reloadOnChange: true)
                    .AddCommandLine(args);

            if (env_name == "Development") builder.AddUserSecrets<Startup>();
            return builder.Build();
        }
    }
}