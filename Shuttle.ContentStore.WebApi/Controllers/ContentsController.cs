using System;
using System.IO;
using System.Linq;
using Shuttle.ContentStore.DataAccess;
using Shuttle.ContentStore.DataAccess.Query;
using Shuttle.ContentStore.Messages.v1;
using Shuttle.ContentStore.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Mvc;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;
using Shuttle.Esb;

namespace Shuttle.ContentStore.WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ContentsController : ControllerBase
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IContentRepository _contentRepository;
        private readonly IContentQuery _contentQuery;
        private readonly IServiceBus _serviceBus;
        private readonly ITransactionScopeFactory _transactionScopeFactory;

        public ContentsController(IServiceBus serviceBus, ITransactionScopeFactory transactionScopeFactory,
            IDatabaseContextFactory databaseContextFactory, IContentRepository contentRepository, IContentQuery contentQuery)
        {
            Guard.AgainstNull(serviceBus, nameof(serviceBus));
            Guard.AgainstNull(transactionScopeFactory, nameof(transactionScopeFactory));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(contentRepository, nameof(contentRepository));
            Guard.AgainstNull(contentQuery, nameof(contentQuery));

            _serviceBus = serviceBus;
            _transactionScopeFactory = transactionScopeFactory;
            _databaseContextFactory = databaseContextFactory;
            _contentRepository = contentRepository;
            _contentQuery = contentQuery;
        }

        [HttpGet]
        [RequiresPermission(Permissions.View.Content)]
        public IActionResult Get()
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = _contentQuery.Search(new DataAccess.Query.Content.Specification()
                        .IncludeProperties()
                        .IncludeStatusEvents()
                        .GetMaximumRows(30))
                });
            }
        }

        [HttpGet("{id}")]
        [RequiresPermission(Permissions.View.Content)]
        public IActionResult Get(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                return Ok(new
                {
                    Data = _contentQuery.Search(new DataAccess.Query.Content.Specification()
                        .AddId(id)
                        .GetActiveOnly()
                        .IncludeProperties()
                        .IncludeStatusEvents())
                });
            }
        }

        [HttpPost]
        [RequiresPermission(Permissions.Register.Content)]
        public IActionResult Post([FromForm] RegisterContentModel model)
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
                var result = _contentQuery.Search(new DataAccess.Query.Content.Specification().AddId(model.Id).GetActiveOnly()).ToList();

                if (result.Any())
                {
                    var content = result.First();

                    if (model.EffectiveFromDate <= content.EffectiveFromDate)
                    {
                        return BadRequest(
                            $"Existing active content (id = '{content.Id}' / reference id = '{content.ReferenceId}') is effective from date '{content.EffectiveFromDate:O}' and the content being registered for the same reference id is effective from date '{model.EffectiveFromDate:O}' which on or after the new one.  The new content should be effective from a date after the existing content.");
                    }
                }

                model.FormFile.CopyTo(stream);

                _contentRepository.Save(new Content(id, model.Id, model.FormFile.FileName, model.ContentType,
                    stream.ToArray(), model.SystemName, model.Username, effectiveFromDate));

                _serviceBus.Send(new RegisterContentCommand
                {
                    Id = id
                });

                scope.Complete();
            }

            return Ok(new
            {
                ContentId = id,
                EffectiveFromDate = $"{effectiveFromDate:O}"
            });
        }

        [HttpGet("{id}/bytes")]
        [RequiresPermission(Permissions.Download.Content)]
        public IActionResult Content(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var rawContent = _contentQuery.FindRawContent(id);

                if (rawContent == null)
                {
                    return BadRequest($"Could not find a content with id '{id}'.");
                }

                if (rawContent.Bytes == null)
                {
                    return BadRequest($"The bytes for content with id '{id}' is not yet available.");
                }

                Response.Headers.Add("sanitized-content", HasBeenSanitized(rawContent));

                return Ok(new
                {
                    Data = rawContent.Bytes
                });
            }
        }

        private string HasBeenSanitized(RawContent rawContent)
        {
            return (rawContent.Status.Equals("Suspicious") && rawContent.Bytes != null).ToString().ToLower();
        }

        [HttpGet("{id}/file")]
        [RequiresPermission(Permissions.Download.Content)]
        public IActionResult Download(Guid id)
        {
            using (_databaseContextFactory.Create())
            {
                var rawContent = _contentQuery.FindRawContent(id);

                if (rawContent == null)
                {
                    return BadRequest($"Could not find a content with id '{id}'.");
                }

                if (rawContent.Bytes == null)
                {
                    return BadRequest($"The bytes for content with id '{id}' is not yet available.");
                }

                Response.Headers.Add("sanitized-content", HasBeenSanitized(rawContent));

                return File(rawContent.Bytes, rawContent.ContentType, rawContent.FileName);
            }
        }
    }
}