using GRYLibrary.Core.APIServer.MidT.Aut;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Mid.M01APIK
{
    public abstract class APIKeyValidatorConfiguration : AuthorizationConfiguration, IAPIKeyValidatorConfiguration
    {
        public override ISet<FilterDescriptor> GetFilter()
        {
            return new HashSet<FilterDescriptor>() { new FilterDescriptor { Type = typeof(APIKeyValidatorFilter), Arguments = Array.Empty<object>() } };
        }
    }
}