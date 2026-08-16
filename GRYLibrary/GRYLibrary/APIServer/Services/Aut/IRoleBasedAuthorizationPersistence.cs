using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Services.Aut
{
    public interface IRoleBasedAuthorizationPersistence
    {
        IEnumerable<string> GetAuhorizedGroupsForAction(string action);
    }
}
