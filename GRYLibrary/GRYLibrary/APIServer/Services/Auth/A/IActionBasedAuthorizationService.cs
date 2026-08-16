using GRYLibrary.Core.APIServer.Services.Interfaces;

namespace GRYLibrary.Core.APIServer.Services.Auth.A
{
    public interface IActionBasedAuthorizationService : IUserAuthorizationService
    {
        public bool IsAuthorized(string userId, string action);
    }
}
