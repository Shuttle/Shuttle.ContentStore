using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.ContentStore.DataAccess;
using Shuttle.ContentStore.DataAccess.Query;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.ContentStore.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;
using Shuttle.Esb;

namespace Shuttle.ContentStore.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentQuery _documentQuery;
        private readonly IServiceBus _serviceBus;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public DocumentsController(IServiceBus serviceBus, ITransactionScopeFactory transactionScopeFactory,
            IDatabaseContextFactory databaseContextFactory, IDocumentRepository documentRepository, IDocumentQuery documentQuery)
        {
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(transactionScopeFactory, nameof(transactionScopeFactory));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(documentRepository, nameof(documentRepository));
            Guard.AgainstNull(documentQuery, nameof(documentQuery));

            _serviceBus = serviceBus;
            _transactionScopeFactory = transactionScopeFactory;
            _databaseContextFactory = databaseContextFactory;
            _documentRepository = documentRepository;
            _documentQuery = documentQuery;
        }

        [HttpGet]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = _documentQuery.Search(new DataAccess.Query.Document.Specification()
                        .IncludeProperties()
                        .IncludeStatusEvents()
                        .GetMaximumRows(30))
                });
            }
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = _documentQuery.Search(new DataAccess.Query.Document.Specification()
                        .AddId(id)
                        .GetActiveOnly()
                        .IncludeProperties()
                        .IncludeStatusEvents())
                });
            }
        }

        [HttpPost]
        public IActionResult Post([FromForm] RegisterDocumentModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            try
            {
                model.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var id = Guid.NewGuid();
            var effectiveFromDate = DateTime.Now;

            using (var scope = _transactionScopeFactory.Create())
            using (_databaseContextFactory.Create())
            using (var stream = new MemoryStream())
            {
                var result = _documentQuery.Search(new DataAccess.Query.Document.Specification().AddId(model.Id).GetActiveOnly()).ToList();

                if (result.Any())
                {
                    var document = result.First();

                    if (model.EffectiveFromDate <= document.EffectiveFromDate)
                    {
                        return BadRequest(
                            $"Existing active document (id = '{document.Id}' / reference id = '{document.ReferenceId}') is effective from date '{document.EffectiveFromDate:O}' and the document being registered for the same reference id is effective from date '{model.EffectiveFromDate:O}' which on or after the new one.  The new document should be effective from a date after the existing document.");
                    }
                }

                model.Document.CopyTo(stream);

                _documentRepository.Save(new Document(id, model.Id, model.Document.FileName, model.ContentType,
                    stream.ToArray(), model.SystemName, model.Username, effectiveFromDate));

                _serviceBus.Send(new RegisterDocumentCommand
                {
                    Id = id
                });

                scope.Complete();
            }

            return Ok(new
            {
                DocumentId = id,
                EffectiveFromDate = $"{effectiveFromDate:O}"
            });
        }

        [HttpGet("{id}/content")]
        public IActionResult Content(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var documentContent = _documentQuery.FindContent(id);

                if (documentContent == null)
                {
                    return BadRequest($"Could not find a document with id '{id}'.");
                }

                if (documentContent.Content == null)
                {
                    return BadRequest($"The content for document with id '{id}' is not yet available.");
                }

                Response.Headers.Add("sanitized-content", HasBeenSanitized(documentContent));

                return Ok(new
                {
                    Data = documentContent.Content
                });
            }
        }

        private string HasBeenSanitized(DocumentContent documentContent)
        {
            return (documentContent.Status.Equals("Suspicious") && documentContent.Content != null).ToString().ToLower();
        }

        [HttpGet("{id}/file")]
        public IActionResult Download(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var documentContent = _documentQuery.FindContent(id);

                if (documentContent == null)
                {
                    return BadRequest($"Could not find a document with id '{id}'.");
                }

                if (documentContent.Content == null)
                {
                    return BadRequest($"The content for document with id '{id}' is not yet available.");
                }

                Response.Headers.Add("sanitized-content", HasBeenSanitized(documentContent));

                return File(documentContent.Content, documentContent.ContentType, documentContent.FileName);
            }
        }
    }
}