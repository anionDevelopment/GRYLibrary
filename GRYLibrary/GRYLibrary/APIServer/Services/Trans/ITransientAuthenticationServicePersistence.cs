using GRYLibrary.Core.APIServer.CommonDBTypes;

namespace GRYLibrary.Core.APIServer.Services.Trans
{
    /// Represetns a authenticationservice-persistence where userdata (user, roles, accesstoken, etc.) will be stored transient so that they will be removed after every restart.
    public interface ITransientAuthenticationServicePersistence<UserType> : IAuthenticationServicePersistence<UserType>
        where UserType : User
    {
    }
}
