using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace GRYLibrary.Core.APIServer.Services.LoginThrottling
{
    /// <summary>
    /// Default implementation of <see cref="ILoginThrottlingService"/> which keeps the throttling-information in memory.
    /// </summary>
    /// <remarks>
    /// The throttling-information is lost when the application restarts and is not shared between several instances of the application. This is accepted by design.
    /// Register this service as singleton, otherwise the throttling-information is not kept between requests.
    /// </remarks>
    public class LoginThrottlingService : ILoginThrottlingService
    {
        /// <summary>
        /// Key which is used for all clients whose ip-address is unknown, so that they can not bypass the throttling per ip-address.
        /// </summary>
        private const string UnknownIPAddressKey = "unknown";
        private static readonly TimeSpan MinimalDurationBetweenTwoCleanups = TimeSpan.FromMinutes(1);

        private readonly ITimeService _TimeService;
        private readonly ILoginThrottlingConfiguration _Configuration;
        private readonly Dictionary<string, FailedLoginsInformation> _FailedLoginsPerUserName = new Dictionary<string, FailedLoginsInformation>();
        private readonly Dictionary<string, FailedLoginsInformation> _FailedLoginsPerIPAddress = new Dictionary<string, FailedLoginsInformation>();
        private readonly object _Lock = new object();
        private DateTimeOffset _MomentOfLastCleanup = DateTimeOffset.MinValue;

        private sealed class FailedLoginsInformation
        {
            public uint AmountOfFailedLogins { get; set; }
            public DateTimeOffset MomentOfLastFailedLogin { get; set; }
            public DateTimeOffset LockedUntil { get; set; }
        }

        public LoginThrottlingService(ITimeService timeService, ILoginThrottlingConfiguration configuration)
        {
            this._TimeService = timeService;
            this._Configuration = configuration;
        }

        /// <inheritdoc/>
        public void AssertLoginIsAllowed(string userName, IPAddress? clientIPAddress)
        {
            lock (this._Lock)
            {
                DateTimeOffset now = this._TimeService.GetCurrentTimeInUTCAsDateTimeOffset();
                this.CleanupIfRequired(now);
                DateTimeOffset lockedUntil = new[]
                {
                    this.GetLockedUntil(this._FailedLoginsPerUserName, userName, now),
                    this.GetLockedUntil(this._FailedLoginsPerIPAddress, GetIPAddressKey(clientIPAddress), now),
                }.Max();
                if (now < lockedUntil)
                {
                    int secondsUntilUnlock = (int)Math.Ceiling((lockedUntil - now).TotalSeconds);
                    throw new BadRequestException(StatusCodes.Status429TooManyRequests, $"Too many failed login-attempts. Try again in {secondsUntilUnlock} seconds.");
                }
            }
        }

        /// <inheritdoc/>
        public void RegisterFailedLogin(string userName, IPAddress? clientIPAddress)
        {
            lock (this._Lock)
            {
                DateTimeOffset now = this._TimeService.GetCurrentTimeInUTCAsDateTimeOffset();
                this.CleanupIfRequired(now);
                this.RegisterFailedLogin(this._FailedLoginsPerUserName, userName, this._Configuration.AmountOfFailedLoginsPerUserNameUntilLock, now);
                this.RegisterFailedLogin(this._FailedLoginsPerIPAddress, GetIPAddressKey(clientIPAddress), this._Configuration.AmountOfFailedLoginsPerIPAddressUntilLock, now);
            }
        }

        /// <inheritdoc/>
        public void RegisterSuccessfulLogin(string userName)
        {
            lock (this._Lock)
            {
                this._FailedLoginsPerUserName.Remove(userName);
            }
        }

        private DateTimeOffset GetLockedUntil(Dictionary<string, FailedLoginsInformation> failedLogins, string key, DateTimeOffset now)
        {
            if (failedLogins.TryGetValue(key, out FailedLoginsInformation? information) && !this.IsForgotten(information, now))
            {
                return information.LockedUntil;
            }
            return DateTimeOffset.MinValue;
        }

        private void RegisterFailedLogin(Dictionary<string, FailedLoginsInformation> failedLogins, string key, uint amountOfFailedLoginsUntilLock, DateTimeOffset now)
        {
            if (!failedLogins.TryGetValue(key, out FailedLoginsInformation? information) || this.IsForgotten(information, now))
            {
                information = new FailedLoginsInformation() { AmountOfFailedLogins = 0, LockedUntil = DateTimeOffset.MinValue };
                failedLogins[key] = information;
            }
            information.AmountOfFailedLogins = information.AmountOfFailedLogins + 1;
            information.MomentOfLastFailedLogin = now;
            if (amountOfFailedLoginsUntilLock <= information.AmountOfFailedLogins)
            {
                information.LockedUntil = now + this.GetLockDuration(information.AmountOfFailedLogins - amountOfFailedLoginsUntilLock);
            }
        }

        /// <summary>
        /// Returns the lock-duration which doubles with every failed login after the threshold, limited to <see cref="ILoginThrottlingConfiguration.MaximalLockDuration"/>.
        /// </summary>
        private TimeSpan GetLockDuration(uint amountOfFailedLoginsAfterThreshold)
        {
            TimeSpan result = this._Configuration.InitialLockDuration;
            for (uint i = 0; i < amountOfFailedLoginsAfterThreshold && result < this._Configuration.MaximalLockDuration; i++)
            {
                result = result * 2;
            }
            return result < this._Configuration.MaximalLockDuration ? result : this._Configuration.MaximalLockDuration;
        }

        private bool IsForgotten(FailedLoginsInformation information, DateTimeOffset now)
        {
            return information.LockedUntil <= now && this._Configuration.DurationUntilFailedLoginsAreForgotten <= now - information.MomentOfLastFailedLogin;
        }

        /// <summary>
        /// Removes forgotten entries, so that the dictionaries can not grow unlimited (for example by failed logins with random user-names).
        /// </summary>
        private void CleanupIfRequired(DateTimeOffset now)
        {
            if (now - this._MomentOfLastCleanup < MinimalDurationBetweenTwoCleanups)
            {
                return;
            }
            this._MomentOfLastCleanup = now;
            foreach (Dictionary<string, FailedLoginsInformation> failedLogins in new[] { this._FailedLoginsPerUserName, this._FailedLoginsPerIPAddress })
            {
                foreach (string key in failedLogins.Where(kvp => this.IsForgotten(kvp.Value, now)).Select(kvp => kvp.Key).ToList())
                {
                    failedLogins.Remove(key);
                }
            }
        }

        /// <summary>
        /// Returns the key for an ip-address. Ipv6-addresses are reduced to their /64-network, because usually a single client gets a whole /64-network and could otherwise bypass the throttling by changing its address.
        /// </summary>
        internal static string GetIPAddressKey(IPAddress? ipAddress)
        {
            if (ipAddress == null)
            {
                return UnknownIPAddressKey;
            }
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                byte[] bytes = ipAddress.GetAddressBytes();
                Array.Clear(bytes, 8, 8);
                return new IPAddress(bytes).ToString() + "/64";
            }
            return ipAddress.ToString();
        }
    }
}
