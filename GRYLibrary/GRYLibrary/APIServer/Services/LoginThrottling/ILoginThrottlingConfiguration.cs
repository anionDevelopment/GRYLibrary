using System;

namespace GRYLibrary.Core.APIServer.Services.LoginThrottling
{
    /// <summary>
    /// Configuration of the <see cref="ILoginThrottlingService"/>.
    /// </summary>
    public interface ILoginThrottlingConfiguration
    {
        /// <summary>
        /// Amount of failed logins for one user-name from which on further logins for this user-name are locked temporarily.
        /// </summary>
        public uint AmountOfFailedLoginsPerUserNameUntilLock { get; set; }

        /// <summary>
        /// Amount of failed logins from one ip-address (or ipv6-/64-network) from which on further logins from this address are locked temporarily.
        /// </summary>
        /// <remarks>
        /// This should be higher than <see cref="AmountOfFailedLoginsPerUserNameUntilLock"/>, because several users can share one ip-address (for example behind a nat-router).
        /// </remarks>
        public uint AmountOfFailedLoginsPerIPAddressUntilLock { get; set; }

        /// <summary>
        /// Duration of the first lock. Every further failed login doubles the duration up to <see cref="MaximalLockDuration"/>.
        /// </summary>
        public TimeSpan InitialLockDuration { get; set; }

        public TimeSpan MaximalLockDuration { get; set; }

        /// <summary>
        /// Duration without failed login after which the failed logins of a user-name or ip-address are forgotten.
        /// </summary>
        public TimeSpan DurationUntilFailedLoginsAreForgotten { get; set; }
    }
}
