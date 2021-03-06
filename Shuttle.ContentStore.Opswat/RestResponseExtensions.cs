﻿using Newtonsoft.Json;
using RestSharp;
using Shuttle.Core.Contract;

namespace Shuttle.ContentStore.Opswat
{
    public static class RestResponseExtensions
    {
        public static dynamic AsDynamic(this IRestResponse response)
        {
            Guard.AgainstNull(response, nameof(response));

            return JsonConvert.DeserializeObject<dynamic>(response.Content);
        }
    }
}