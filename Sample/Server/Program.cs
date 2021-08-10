using System.Data.Common;
using System.Data.SqlClient;
using Ninject;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.Core.Ninject;
using Shuttle.Core.ServiceHost;
using Shuttle.Esb;
using Shuttle.Esb.Sql.Subscription;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            ServiceHost.Run<Host>();
        }
    }

    public class Host : IServiceHost
    {
        private IServiceBus _bus;
        private StandardKernel _kernel;

        public void Stop()
        {
            _bus?.Dispose();
            _kernel?.Dispose();
        }

        public void Start()
        {
            _kernel = new StandardKernel();

            var container = new NinjectComponentContainer(_kernel);

            ServiceBus.Register(container);

            container.Resolve<ISubscriptionManager>().Subscribe<ContentProcessedEvent>();

            _bus = ServiceBus.Create(container).Start();
        }
    }
}