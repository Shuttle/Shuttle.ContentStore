﻿using System;
using Microsoft.AspNetCore.Http;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.WebApi.Models
{
    public class RegisterDocumentModel
    {
        public Guid Id { get; set; }
        public IFormFile Document { get; set; }
        public string ContentType { get; set; }
        public string SystemName { get; set; }
        public string Username { get; set; }
        public DateTime EffectiveFromDate { get; set; }

        public void ApplyInvariants()
        {
            Guard.Against<ArgumentException>(Guid.Empty.Equals(Id), "The 'Id' may not be an empty guid.");
            Guard.AgainstNull(Document, nameof(Document));
            Guard.AgainstNullOrEmptyString(ContentType, nameof(ContentType));
            Guard.AgainstNullOrEmptyString(SystemName, nameof(SystemName));
            Guard.AgainstNullOrEmptyString(Username, nameof(Username));
        }
    }
}