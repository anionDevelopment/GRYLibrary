namespace GRYLibrary.Core.APIServer.Services.Interfaces
{
    public interface IAPIKeyAuthorizationService : IAuthorizationService
    {
        public bool IsAuthorized(string apiKey, string action);
    }
}
