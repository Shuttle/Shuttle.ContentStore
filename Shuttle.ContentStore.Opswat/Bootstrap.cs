using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Opswat
{
    public class Bootstrap : IComponentRegistryBootstrap
    {
        public void Register(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, nameof(registry));

            if (!registry.IsRegistered<IOpswatConfiguration>())
            {
                registry.RegisterInstance(OpswatSection.Configuration());
            }

            registry.AttemptRegister<IOpswatApi, OpswatApi>();
            registry.AttemptRegister<IMalwareService, OpswatMalwareService>();
        }
    }
}