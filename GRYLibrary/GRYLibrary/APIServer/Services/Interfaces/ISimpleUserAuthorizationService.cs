namespace GRYLibrary.Core.APIServer.Services.Interfaces
{
    public interface ISimpleUserAuthorizationService : IUserAuthorizationService
    {
        public bool IsAuthorized(string user, string action);
    }
}
