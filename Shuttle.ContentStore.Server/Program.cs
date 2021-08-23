using System.Data.Common;
using System.Data.SqlClient;
using System.Net;
using System.Text;
using log4net;
using Ninject;
using Shuttle.Core.Configuration;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;
using Shuttle.Core.Ninject;
using Shuttle.Core.Reflection;
using Shuttle.Core.ServiceHost;
using Shuttle.Esb;

namespace Shuttle.ContentStore.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceHost.Run<Host>();
        }
    }

    public class Host : IServiceHost
    {
        private readonly IKernel _kernel = new StandardKernel();
        private IServiceBus _bus;
        private NinjectComponentContainer _container;

        public void Start()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Log.Assign(new Log4NetLog(LogManager.GetLogger(typeof(Host))));

            if (ConfigurationItem<bool>.ReadSetting("SkipCertificateValidation", true).GetValue())
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            }

            ServicePointManager.Expect100Continue = false;
            ServicePointManager.MaxServicePointIdleTime = 0;

            _container = new NinjectComponentContainer(_kernel);

            _container.Register<IMalwareService, DefaultMalwareService>();

            ServiceBus.Register(_container);

            _container.Resolve<IDatabaseContextFactory>().ConfigureWith("ContentStore");

            _bus = ServiceBus.Create(_container).Start();
        }

        public void Stop()
        {
            _bus?.Dispose();
            _container?.AttemptDispose();
            _kernel?.Dispose();
        }
    }
}