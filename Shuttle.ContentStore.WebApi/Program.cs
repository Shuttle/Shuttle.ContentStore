using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;

namespace Shuttle.ContentStore.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            Log.Assign(
                new Log4NetLog(LogManager.GetLogger(typeof(Program)),
                    new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.xml"))));

            Log.Information("[started]");

            CreateWebHostBuilder(args).Build().Run();

            Log.Information("[stopped]");

            LogManager.Shutdown();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIIS()
                .UseStartup<Startup>();
    }
}
