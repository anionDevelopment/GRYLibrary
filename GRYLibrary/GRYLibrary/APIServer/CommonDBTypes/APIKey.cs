using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.CommonDBTypes
{
    [PrimaryKey(nameof(APIKeyValue))]
    public class APIKey
    {
        public string APIKeyValue { get; set; }
        public DateTime ExpireMoment { get; set; }
        public string Description { get; set; }
        public static bool IsValid(IEnumerable<APIKey> apiKeys, string apiKeyValue, DateTime dateTime)
        {
            foreach (APIKey apiKey in apiKeys)
            {
                if (apiKey.APIKeyValue == apiKeyValue)
                {
                    return dateTime < apiKey.ExpireMoment;
                }
            }
            return false;
        }
        public static bool APIKeyIsGiven(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }
            if (apiKey.StartsWith('[') && apiKey.EndsWith(']')) // option to allow non-functional descriptions as default-value
            {
                return false;
            }
            return true;
        }
    }
}

