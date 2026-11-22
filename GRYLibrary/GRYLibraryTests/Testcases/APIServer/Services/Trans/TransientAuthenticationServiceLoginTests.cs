using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace GRYLibrary.Tests.Testcases.APIServer.Services.Trans
{
    [TestClass]
    public class TransientAuthenticationServiceLoginTests
    {
        private const string ExistingUserName = "existing-user";
        private const string CorrectPassword = "correct-password";

        private static TransientAuthenticationService<User> GetServiceWithUser()
        {
            TimeService timeService = new TimeService();
            TransientAuthenticationService<User> service = new TransientAuthenticationService<User>(timeService, new TransientAuthenticationServicePersistence<User>(timeService), new AuthenticationServiceSettings());
            service.AddUserTyped(User.CreateNewUser(ExistingUserName, service.Hash(CorrectPassword), timeService));
            service.AddUserTyped(User.CreateNewUser("user-without-password", string.Empty, timeService));
            return service;
        }

        private static string GetMessageOfFailedLogin(TransientAuthenticationService<User> service, string userName, string password)
        {
            BadRequestException exception = Assert.ThrowsExactly<BadRequestException>(() => service.Login(userName, password));
            return exception.Message;
        }

        /// <summary>Returns the median-duration of several failed logins, so that a single outlier does not influence the result.</summary>
        private static TimeSpan GetMedianDurationOfFailedLogin(TransientAuthenticationService<User> service, string userName)
        {
            TimeSpan[] durations = Enumerable.Range(0, 5).Select(_ =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Assert.ThrowsExactly<BadRequestException>(() => service.Login(userName, "wrong-password"));
                stopwatch.Stop();
                return stopwatch.Elapsed;
            }).OrderBy(duration => duration).ToArray();
            return durations[durations.Length / 2];
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LoginWithCorrectPasswordReturnsAccessToken()
        {
            // arrange
            TransientAuthenticationService<User> service = GetServiceWithUser();

            // act
            AccessToken accessToken = service.Login(ExistingUserName, CorrectPassword);

            // assert
            Assert.IsFalse(string.IsNullOrEmpty(accessToken.Value));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void FailedLoginsCanNotBeDistinguishedByTheirMessage()
        {
            // arrange
            TransientAuthenticationService<User> service = GetServiceWithUser();

            // act
            string messageForUnknownUser = GetMessageOfFailedLogin(service, "unknown-user", "wrong-password");
            string messageForWrongPassword = GetMessageOfFailedLogin(service, ExistingUserName, "wrong-password");
            string messageForUserWithoutPassword = GetMessageOfFailedLogin(service, "user-without-password", string.Empty);

            // assert
            Assert.AreEqual(messageForWrongPassword, messageForUnknownUser);
            Assert.AreEqual(messageForWrongPassword, messageForUserWithoutPassword);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LoginOfUnknownUserTakesAsLongAsLoginWithWrongPassword()
        {
            // arrange
            TransientAuthenticationService<User> service = GetServiceWithUser();

            // act
            TimeSpan durationForWrongPassword = GetMedianDurationOfFailedLogin(service, ExistingUserName);
            TimeSpan durationForUnknownUser = GetMedianDurationOfFailedLogin(service, "unknown-user");

            // assert
            // Without the dummy-verification the login of an unknown user would take almost no time compared to a password-hash-verification (600000 PBKDF2-iterations), so a generous tolerance keeps this testcase stable on slow or busy machines.
            Assert.IsTrue(durationForWrongPassword * 0.5 < durationForUnknownUser, $"Unknown user: {durationForUnknownUser.TotalMilliseconds} ms, wrong password: {durationForWrongPassword.TotalMilliseconds} ms.");
        }
    }
}
