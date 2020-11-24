using System;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.ContentStore.Server.Handlers
{
    public class PollDocumentHandler : IMessageHandler<PollDocumentCommand>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMalwareService _malwareService;

        public PollDocumentHandler(IMalwareService malwareService,
            IDatabaseContextFactory databaseContextFactory, IDocumentRepository documentRepository)
        {
            Guard.AgainstNull(malwareService, nameof(malwareService));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(documentRepository, nameof(documentRepository));

            _malwareService = malwareService;
            _databaseContextFactory = databaseContextFactory;
            _documentRepository = documentRepository;
        }

        public void ProcessMessage(IHandlerContext<PollDocumentCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            object commandMessage = null;
            object eventMessage = null;
            var pollIntervalTimeSpan = TimeSpan.Zero;

            using (_databaseContextFactory.Create())
            {
                var document = _documentRepository.Get(context.Message.Id);

                var status = _malwareService.Poll(document);

                switch (status)
                {
                    case ServiceStatus.Cleared:
                    {
                        document.Cleared();

                        _documentRepository.Save(document);

                        eventMessage = new DocumentProcessedEvent
                        {
                            Id = document.Id,
                            ReferenceId = document.ReferenceId,
                            SystemName = document.SystemName,
                            Suspicious = false
                        };

                        break;
                    }
                    case ServiceStatus.Suspicious:
                    {
                        document.Suspicious();

                        _documentRepository.Save(document);

                        eventMessage = new DocumentProcessedEvent
                        {
                            Id = document.Id,
                            ReferenceId = document.ReferenceId,
                            SystemName = document.SystemName,
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
                        _documentRepository.Save(document);

                        commandMessage = new PollDocumentCommand
                        {
                            Id = document.Id
                        };

                        pollIntervalTimeSpan = TimeSpan.FromSeconds(5);

                        if (document.ContainsProperty("PollIntervalTimeSpan"))
                        {
                            TimeSpan.TryParse(document.GetPropertyValue("PollIntervalTimeSpan"),
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