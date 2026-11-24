using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.LoginThrottling;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace GRYLibrary.Tests.Testcases.APIServer.Services.LoginThrottling
{
    [TestClass]
    public class LoginThrottlingServiceTests
    {
        private static readonly IPAddress ClientIPAddress = IPAddress.Parse("192.0.2.1");
        private static readonly IPAddress OtherClientIPAddress = IPAddress.Parse("192.0.2.2");

        /// <summary>A time-service whose current time is set by the testcase.</summary>
        private sealed class ControllableTimeService : ITimeService
        {
            public DateTimeOffset Now { get; set; } = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            public DateTimeOffset GetCurrentTimeInUTCAsDateTimeOffset()
            {
                return this.Now;
            }

            public DateTimeOffset GetCurrentLocalTimeAsDateTimeOffset()
            {
                return this.Now;
            }

            public GRYDateTime GetCurrentLocalTimeAsGRYDateTime()
            {
                throw new NotSupportedException();
            }

            public GRYDateTime GetCurrentTimeInUTCAsGRYDateTime()
            {
                throw new NotSupportedException();
            }

            public DateTimeOffset GetCurrentTimeInTimezone(TimeZoneInfo timeZone)
            {
                throw new NotSupportedException();
            }

            public GRYDateTime GetCurrentTimeInTimezoneAsGRYDateTime(TimeZoneInfo timeZone)
            {
                throw new NotSupportedException();
            }
        }

        private static LoginThrottlingConfiguration GetConfiguration()
        {
            return new LoginThrottlingConfiguration()
            {
                AmountOfFailedLoginsPerUserNameUntilLock = 3,
                AmountOfFailedLoginsPerIPAddressUntilLock = 5,
                InitialLockDuration = TimeSpan.FromMinutes(1),
                MaximalLockDuration = TimeSpan.FromMinutes(4),
                DurationUntilFailedLoginsAreForgotten = TimeSpan.FromHours(1),
            };
        }

        private static void RegisterFailedLogins(LoginThrottlingService service, string userName, IPAddress? ipAddress, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                service.RegisterFailedLogin(userName, ipAddress);
            }
        }

        private static void AssertLoginIsLocked(LoginThrottlingService service, string userName, IPAddress? ipAddress)
        {
            BadRequestException exception = Assert.ThrowsExactly<BadRequestException>(() => service.AssertLoginIsAllowed(userName, ipAddress));
            Assert.AreEqual(StatusCodes.Status429TooManyRequests, exception.HTTPStatusCode);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LoginIsAllowedBelowThresholdOfUserName()
        {
            // arrange
            LoginThrottlingService service = new LoginThrottlingService(new ControllableTimeService(), GetConfiguration());

            // act
            RegisterFailedLogins(service, "user", ClientIPAddress, 2);

            // assert
            service.AssertLoginIsAllowed("user", ClientIPAddress);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UserNameIsLockedFromThresholdOnAlsoFromOtherIPAddresses()
        {
            // arrange
            LoginThrottlingService service = new LoginThrottlingService(new ControllableTimeService(), GetConfiguration());

            // act
            RegisterFailedLogins(service, "user", ClientIPAddress, 3);

            // assert
            AssertLoginIsLocked(service, "user", OtherClientIPAddress);
            service.AssertLoginIsAllowed("other-user", OtherClientIPAddress);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IPAddressIsLockedFromThresholdOnAlsoForOtherUserNames()
        {
            // arrange
            LoginThrottlingService service = new LoginThrottlingService(new ControllableTimeService(), GetConfiguration());

            // act
            for (int i = 0; i < 5; i++)
            {
                service.RegisterFailedLogin($"user-{i}", ClientIPAddress);
            }

            // assert
            AssertLoginIsLocked(service, "never-used-user", ClientIPAddress);
            service.AssertLoginIsAllowed("never-used-user", OtherClientIPAddress);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LockDurationDoublesUpToMaximum()
        {
            // arrange
            ControllableTimeService timeService = new ControllableTimeService();
            LoginThrottlingService service = new LoginThrottlingService(timeService, GetConfiguration());
            DateTimeOffset start = timeService.Now;

            // act & assert
            RegisterFailedLogins(service, "user", ClientIPAddress, 3);//lock: 1 minute
            timeService.Now = start + TimeSpan.FromSeconds(59);
            AssertLoginIsLocked(service, "user", OtherClientIPAddress);
            timeService.Now = start + TimeSpan.FromMinutes(1);
            service.AssertLoginIsAllowed("user", OtherClientIPAddress);

            service.RegisterFailedLogin("user", OtherClientIPAddress);//lock: 2 minutes
            timeService.Now = start + TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(119);
            AssertLoginIsLocked(service, "user", OtherClientIPAddress);

            timeService.Now = start + TimeSpan.FromMinutes(10);
            RegisterFailedLogins(service, "user", OtherClientIPAddress, 3);//lock would be 16 minutes, limited to 4 minutes
            timeService.Now = start + TimeSpan.FromMinutes(14);
            service.AssertLoginIsAllowed("user", OtherClientIPAddress);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void FailedLoginsAreForgottenAfterConfiguredDuration()
        {
            // arrange
            ControllableTimeService timeService = new ControllableTimeService();
            LoginThrottlingService service = new LoginThrottlingService(timeService, GetConfiguration());
            RegisterFailedLogins(service, "user", ClientIPAddress, 2);

            // act
            timeService.Now = timeService.Now + TimeSpan.FromHours(1);
            service.RegisterFailedLogin("user", ClientIPAddress);

            // assert
            service.AssertLoginIsAllowed("user", ClientIPAddress);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SuccessfulLoginResetsOnlyUserNameButNotIPAddress()
        {
            // arrange
            LoginThrottlingService service = new LoginThrottlingService(new ControllableTimeService(), GetConfiguration());
            RegisterFailedLogins(service, "user", ClientIPAddress, 2);
            RegisterFailedLogins(service, "other-user", ClientIPAddress, 2);

            // act
            service.RegisterSuccessfulLogin("user");
            RegisterFailedLogins(service, "user", ClientIPAddress, 1);

            // assert
            AssertLoginIsLocked(service, "third-user", ClientIPAddress);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void IPv6AddressesOfSameNetworkAreThrottledTogether()
        {
            // arrange
            LoginThrottlingService service = new LoginThrottlingService(new ControllableTimeService(), GetConfiguration());

            // act
            for (int i = 1; i <= 5; i++)
            {
                service.RegisterFailedLogin($"user-{i}", IPAddress.Parse($"2001:db8:1:2::{i}"));
            }

            // assert
            AssertLoginIsLocked(service, "never-used-user", IPAddress.Parse("2001:db8:1:2:ffff::1"));
            service.AssertLoginIsAllowed("never-used-user", IPAddress.Parse("2001:db8:1:3::1"));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ClientsWithUnknownIPAddressAreThrottledTogether()
        {
            // arrange
            LoginThrottlingService service = new LoginThrottlingService(new ControllableTimeService(), GetConfiguration());

            // act
            for (int i = 0; i < 5; i++)
            {
                service.RegisterFailedLogin($"user-{i}", null);
            }

            // assert
            AssertLoginIsLocked(service, "never-used-user", null);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LockedLoginIsRejectedEvenWithCorrectPasswordAndUnknownUserIsThrottledLikeExistingUser()
        {
            // arrange
            ControllableTimeService timeService = new ControllableTimeService();
            TransientAuthenticationService<User> authenticationService = new TransientAuthenticationService<User>(timeService, new TransientAuthenticationServicePersistence<User>(timeService), new AuthenticationServiceSettings(), new LoginThrottlingService(timeService, GetConfiguration()));
            authenticationService.AddUserTyped(User.CreateNewUser("user", authenticationService.Hash("correct-password"), timeService));
            for (int i = 0; i < 3; i++)
            {
                Assert.ThrowsExactly<BadRequestException>(() => authenticationService.Login("user", "wrong-password", ClientIPAddress));
                Assert.ThrowsExactly<BadRequestException>(() => authenticationService.Login("unknown-user", "wrong-password", OtherClientIPAddress));
            }

            // act
            BadRequestException exceptionForExistingUser = Assert.ThrowsExactly<BadRequestException>(() => authenticationService.Login("user", "correct-password", ClientIPAddress));
            BadRequestException exceptionForUnknownUser = Assert.ThrowsExactly<BadRequestException>(() => authenticationService.Login("unknown-user", "wrong-password", OtherClientIPAddress));

            // assert
            Assert.AreEqual(StatusCodes.Status429TooManyRequests, exceptionForExistingUser.HTTPStatusCode);
            Assert.AreEqual(StatusCodes.Status429TooManyRequests, exceptionForUnknownUser.HTTPStatusCode);
            Assert.AreEqual(exceptionForExistingUser.Message, exceptionForUnknownUser.Message);
        }
    }
}
