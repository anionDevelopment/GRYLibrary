using GRYLibrary.Core.APIServer.MidT.Auth;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Mid.AuthS
{
    public class AuthSConfiguration : AuthenticationConfiguration, IAuthSConfiguration
    {
        public override ISet<FilterDescriptor> GetFilter()
        {
            return new HashSet<FilterDescriptor>() { new FilterDescriptor { Type = typeof(AuthSFilter), Arguments = Array.Empty<object>() } };
        }
    }
}
