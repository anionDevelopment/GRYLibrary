using System.Net;

namespace GRYLibrary.Core.APIServer.Services.LoginThrottling
{
    /// <summary>
    /// Throttles logins per user-name and per ip-address to prevent online-password-guessing.
    /// </summary>
    /// <remarks>
    /// Failed logins for user-names which do not exist must be registered as well, so that the throttling does not reveal whether a user-name exists.
    /// </remarks>
    public interface ILoginThrottlingService
    {
        /// <summary>
        /// Throws a <see cref="Exceptions.BadRequestException"/> with status-code 429 if logins for <paramref name="userName"/> or from <paramref name="clientIPAddress"/> are currently locked.
        /// </summary>
        public void AssertLoginIsAllowed(string userName, IPAddress? clientIPAddress);

        public void RegisterFailedLogin(string userName, IPAddress? clientIPAddress);

        /// <summary>
        /// Forgets the failed logins of <paramref name="userName"/>.
        /// </summary>
        /// <remarks>
        /// The failed logins of the ip-address are intentionally not forgotten, otherwise an attacker could reset the throttling of its ip-address by logging in with an own account.
        /// </remarks>
        public void RegisterSuccessfulLogin(string userName);
    }
}
