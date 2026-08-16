using GRYLibrary.Core.APIServer.Services.GDPR;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Settings.Regulations.GDPR
{
    [ApiController]
    [Route($"{ServerConfiguration.APIRoutePrefix}/Other/Regulations/GDPR")]
    public class GDPRController : Controller
    {
        private readonly IGDPRService _GDPRService;
        public GDPRController(IGDPRService gdprService)
        {
            this._GDPRService = gdprService;
            //TODO add authentication-middleware for this controller
        }

        [HttpGet]
        [Route(nameof(GetPersonalData))]
        public ISet<PersonalData> GetPersonalData([FromQuery] string personIdentifier)
        {
            return this._GDPRService.GetPersonalData(personIdentifier);
        }

        [HttpGet]
        [Route(nameof(DeleteDeletablePersonalData))]
        public void DeleteDeletablePersonalData([FromQuery] string personIdentifier)
        {
            this._GDPRService.DeleteDeletablePersonalData(personIdentifier);
        }
    }
}
