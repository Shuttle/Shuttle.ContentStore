using System;
using System.Text.RegularExpressions;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.Core.Configuration;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.ContentStore.Server.Handlers
{
    public class RegisterContentHandler : IMessageHandler<RegisterContentCommand>
    {
        private static readonly Regex SuspiciousExpression = new Regex(ConfigurationItem<string>.ReadSetting("SuspiciousExpression", "(?!)").GetValue());

        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IContentRepository _contentRepository;
        private readonly IMalwareService _malwareService;

        public RegisterContentHandler(IMalwareService malwareService,
            IDatabaseContextFactory databaseContextFactory, IContentRepository contentRepository)
        {
            Guard.AgainstNull(malwareService, nameof(malwareService));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(contentRepository, nameof(contentRepository));

            _malwareService = malwareService;
            _databaseContextFactory = databaseContextFactory;
            _contentRepository = contentRepository;
        }

        public void ProcessMessage(IHandlerContext<RegisterContentCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            using (_databaseContextFactory.Create())
            {
                var content = _contentRepository.Get(context.Message.Id);

                var status = !SuspiciousExpression.IsMatch(content.FileName) 
                    ? _malwareService.Register(content) 
                    : ServiceStatus.Suspicious;

                switch (status)
                {
                    case ServiceStatus.Passed:
                    {
                        content.Passed();

                        _contentRepository.Save(content);

                        context.Publish(new ContentProcessedEvent
                        {
                            Id = content.Id,
                            ReferenceId = content.ReferenceId,
                            SystemName = content.SystemName,
                            Suspicious = false
                        });

                        break;
                    }
                    case ServiceStatus.Suspicious:
                    {
                        content.Suspicious();

                        _contentRepository.Save(content);

                        context.Publish(new ContentProcessedEvent
                        {
                            Id = content.Id,
                            ReferenceId = content.ReferenceId,
                            SystemName = content.SystemName,
                            Suspicious = true
                        });

                        break;
                    }
                    case ServiceStatus.Registered:
                    case ServiceStatus.Processing:
                    {
                        _contentRepository.Save(content);

                        context.Send(new PollContentCommand
                        {
                            Id = content.Id
                        }, c => c.Defer(DateTime.Now.AddSeconds(5)).Local());

                        break;
                    }
                }
            }
        }
    }
}