using Shuttle.ContentStore.DataAccess;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore
{
    public class Bootstrap : IComponentRegistryBootstrap
    {
        public void Register(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, nameof(registry));

            registry.AttemptRegister<IContentQueryFactory, ContentQueryFactory>();
            registry.AttemptRegister<IContentRepository, ContentRepository>();
            registry.AttemptRegister<IContentQuery, ContentQuery>();
        }
    }
}