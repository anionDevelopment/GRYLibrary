using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.GDPR
{
    public interface IGDPRService
    {
        public ISet<PersonalData> GetPersonalData(string personIdentifier);
        public void DeleteDeletablePersonalData(string personIdentifier);
    }
}
