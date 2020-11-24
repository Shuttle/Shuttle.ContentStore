using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ninject;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Data.Http;
using Shuttle.Core.Ninject;
using Shuttle.Esb;

namespace Shuttle.ContentStore.WebApi
{
    public class Startup
    {
        private IServiceBus _bus;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private void OnShutdown()
        {
            _bus?.Dispose();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IKernel>(new StandardKernel());
            services.AddSingleton<IControllerActivator, ControllerActivator>();

            services.AddSingleton<IConnectionConfigurationProvider, ConnectionConfigurationProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IDatabaseContextCache, ContextDatabaseContextCache>();
            services.AddSingleton<IDatabaseContextFactory, DatabaseContextFactory>();
            services.AddSingleton<IDatabaseGateway, DatabaseGateway>();
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddSingleton<IDbCommandFactory, DbCommandFactory>();
            services.AddSingleton<IQueryMapper, QueryMapper>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddCors();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IApplicationLifetime applicationLifetime)
        {
            var container = app.ApplicationServices.GetService<IKernel>();

            var componentContainer = new NinjectComponentContainer(container);

            componentContainer.Register<IHttpContextAccessor, HttpContextAccessor>();
            componentContainer.Register<IDatabaseContextCache, ContextDatabaseContextCache>();

            var applicationPartManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            var controllerFeature = new ControllerFeature();

            applicationPartManager.PopulateFeature(controllerFeature);

            foreach (var type in controllerFeature.Controllers.Select(t => t.AsType()))
            {
                componentContainer.Register(type, type);
            }

            ServiceBus.Register(componentContainer);

            var databaseContextFactory = componentContainer.Resolve<IDatabaseContextFactory>();

            databaseContextFactory.ConfigureWith("DocumentStore");

            _bus = ServiceBus.Create(componentContainer).Start();

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
