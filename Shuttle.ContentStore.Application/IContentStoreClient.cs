﻿using System;

namespace Shuttle.ContentStore.Application
{
    public interface IContentStoreClient
    {
        Guid Register(Guid accessToken, Guid referenceId, string fileName, string contentType, byte[] bytes,
            string systemName, string username, DateTime effectiveFromDate);
    }
}