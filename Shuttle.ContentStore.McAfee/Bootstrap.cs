using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.McAfee
{
    public class Bootstrap : IComponentRegistryBootstrap
    {
        public void Register(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, nameof(registry));

            if (!registry.IsRegistered<IMcAfeeConfiguration>())
            {
                registry.RegisterInstance(McAfeeSection.Configuration());
            }

            registry.AttemptRegister<IMcAfeeApi, McAfeeApi>();
            registry.AttemptRegister<IMalwareService, McAfeeMalwareService>();
        }
    }
}