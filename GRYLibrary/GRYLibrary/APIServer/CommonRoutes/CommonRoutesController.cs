using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.CommonRoutes
{
    [ApiController]
    [Route($"{ServerConfiguration.APIRoutePrefix}/Other/Resources/Information")]
    public class CommonRoutesController : Controller
    {
        private readonly IApplicationConstants _Configuration;
        private readonly ICommonRoutesInformation _CommonRoutesInformation;
        private readonly IEnumerable<EndpointDataSource> _EndpointSources;
        public CommonRoutesController(IApplicationConstants configuration, ICommonRoutesInformation commonRoutesInformation, IEnumerable<EndpointDataSource> endpointSources)
        {
            this._Configuration = configuration;
            this._CommonRoutesInformation = commonRoutesInformation;
            this._EndpointSources = endpointSources;
        }

        [HttpGet]
        [Route(nameof(TermsOfService))]
        public virtual IActionResult TermsOfService()
        {
            return this.Redirect(this._CommonRoutesInformation.TermsOfServiceLink);
        }

        [HttpGet]
        [Route(nameof(Contact))]
        public virtual IActionResult Contact()
        {
            return this.Redirect(this._CommonRoutesInformation.ContactLink);
        }

        [HttpGet]
        [Route(nameof(License))]
        public virtual IActionResult License()
        {
            return this.Redirect(this._CommonRoutesInformation.LicenseLink);
        }
    }
}
