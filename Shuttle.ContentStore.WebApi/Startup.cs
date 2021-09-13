using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ninject;
using Shuttle.Access;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Mvc.DataStore;
using Shuttle.Access.Sql;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Data.Http;
using Shuttle.Core.Logging;
using Shuttle.Core.Ninject;
using Shuttle.Core.Reflection;
using Shuttle.Esb;

namespace Shuttle.ContentStore.WebApi
{
    public class Startup
    {
        private readonly ILog _log;
        private IServiceBus _bus;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _log = Log.For(this);
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
            services.AddSingleton<IDataRowMapper, DataRowMapper>();
            services.AddSingleton(typeof(IDataRepository<>), typeof(DataRepository<>));
            services.AddSingleton<ISessionQueryFactory, SessionQueryFactory>();
            services.AddSingleton<IDataRowMapper<Session>, SessionMapper>();
            services.AddSingleton(AccessSection.Configuration());
            services.AddSingleton<ISessionRepository, SessionRepository>();
            services.AddSingleton<IAccessService, DataStoreAccessService>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime applicationLifetime)
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

            databaseContextFactory.ConfigureWith("ContentStore");

            _bus = ServiceBus.Create(componentContainer).Start();

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();

                    if (feature != null)
                    {
                        _log.Error(feature.Error.AllMessages());
                    }

                    await Task.CompletedTask;
                });
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}