using System;
using System.Text.RegularExpressions;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.Core.Configuration;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.ContentStore.Server.Handlers
{
    public class RegisterDocumentHandler : IMessageHandler<RegisterDocumentCommand>
    {
        private static readonly Regex SuspiciousExpression = new Regex(ConfigurationItem<string>.ReadSetting("SuspiciousExpression", "(?!)").GetValue());

        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMalwareService _malwareService;

        public RegisterDocumentHandler(IMalwareService malwareService,
            IDatabaseContextFactory databaseContextFactory, IDocumentRepository documentRepository)
        {
            Guard.AgainstNull(malwareService, nameof(malwareService));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(documentRepository, nameof(documentRepository));

            _malwareService = malwareService;
            _databaseContextFactory = databaseContextFactory;
            _documentRepository = documentRepository;
        }

        public void ProcessMessage(IHandlerContext<RegisterDocumentCommand> context)
        {
            Guard.AgainstNull(context, nameof(context));

            using (_databaseContextFactory.Create())
            {
                var document = _documentRepository.Get(context.Message.Id);

                var status = !SuspiciousExpression.IsMatch(document.FileName) 
                    ? _malwareService.Register(document) 
                    : ServiceStatus.Suspicious;

                switch (status)
                {
                    case ServiceStatus.Cleared:
                    {
                        document.Cleared();

                        _documentRepository.Save(document);

                        context.Publish(new DocumentProcessedEvent
                        {
                            Id = document.Id,
                            ReferenceId = document.ReferenceId,
                            SystemName = document.SystemName,
                            Suspicious = false
                        });

                        break;
                    }
                    case ServiceStatus.Suspicious:
                    {
                        document.Suspicious();

                        _documentRepository.Save(document);

                        context.Publish(new DocumentProcessedEvent
                        {
                            Id = document.Id,
                            ReferenceId = document.ReferenceId,
                            SystemName = document.SystemName,
                            Suspicious = true
                        });

                        break;
                    }
                    case ServiceStatus.Registered:
                    case ServiceStatus.Processing:
                    {
                        _documentRepository.Save(document);

                        context.Send(new PollDocumentCommand
                        {
                            Id = document.Id
                        }, c => c.Defer(DateTime.Now.AddSeconds(5)).Local());

                        break;
                    }
                }
            }
        }
    }
}