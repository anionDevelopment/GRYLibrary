using System;

namespace GRYLibrary.Core.APIServer.Services.LoginThrottling
{
    public class LoginThrottlingConfiguration : ILoginThrottlingConfiguration
    {
        public uint AmountOfFailedLoginsPerUserNameUntilLock { get; set; } = 5;
        public uint AmountOfFailedLoginsPerIPAddressUntilLock { get; set; } = 20;
        public TimeSpan InitialLockDuration { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan MaximalLockDuration { get; set; } = TimeSpan.FromMinutes(15);
        public TimeSpan DurationUntilFailedLoginsAreForgotten { get; set; } = TimeSpan.FromHours(1);
    }
}
