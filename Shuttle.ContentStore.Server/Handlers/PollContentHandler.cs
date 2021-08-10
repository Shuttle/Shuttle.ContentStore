using System;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.ContentStore.Server.Handlers
{
    public class PollContentHandler : IMessageHandler<PollContentCommand>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IContentRepository _contentRepository;
        private readonly IMalwareService _malwareService;

        public PollContentHandler(IMalwareService malwareService,
            IDatabaseContextFactory databaseContextFactory, IContentRepository contentRepository)
        {
            Guard.AgainstNull(malwareService, nameof(malwareService));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(contentRepository, nameof(contentRepository));

            _malwareService = malwareService;
            _databaseContextFactory = databaseContextFactory;
            _contentRepository = contentRepository;
        }

        public void ProcessMessage(IHandlerContext<PollContentCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            object commandMessage = null;
            object eventMessage = null;
            var pollIntervalTimeSpan = TimeSpan.Zero;

            using (_databaseContextFactory.Create())
            {
                var content = _contentRepository.Get(context.Message.Id);

                var status = _malwareService.Poll(content);

                switch (status)
                {
                    case ServiceStatus.Passed:
                    {
                        content.Passed();

                        _contentRepository.Save(content);

                        eventMessage = new ContentProcessedEvent
                        {
                            Id = content.Id,
                            ReferenceId = content.ReferenceId,
                            SystemName = content.SystemName,
                            Suspicious = false
                        };

                        break;
                    }
                    case ServiceStatus.Suspicious:
                    {
                        content.Suspicious();

                        _contentRepository.Save(content);

                        eventMessage = new ContentProcessedEvent
                        {
                            Id = content.Id,
                            ReferenceId = content.ReferenceId,
                            SystemName = content.SystemName,
                            Suspicious = true
                        };

                        break;
                    }
                }

                switch (status)
                {
                    case ServiceStatus.Processing:
                    case ServiceStatus.Registered:
                    {
                        _contentRepository.Save(content);

                        commandMessage = new PollContentCommand
                        {
                            Id = content.Id
                        };

                        pollIntervalTimeSpan = TimeSpan.FromSeconds(5);

                        if (content.ContainsProperty("PollIntervalTimeSpan"))
                        {
                            TimeSpan.TryParse(content.GetPropertyValue("PollIntervalTimeSpan"),
                                out pollIntervalTimeSpan);
                        }

                        break;
                    }
                }
            }

            if (commandMessage != null)
            {
                context.Send(commandMessage, c =>
                {
                    if (pollIntervalTimeSpan == TimeSpan.Zero)
                    {
                        return;
                    }

                    c.Defer(DateTime.Now.Add(pollIntervalTimeSpan)).Local();
                });
            }

            if (eventMessage != null)
            {
                context.Publish(eventMessage);
            }
        }
    }
}