using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Captcha;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class CaptchaManagerTests
    {
        /// <summary>Settings whose captcha and access-token stay valid for the duration of a testcase.</summary>
        private static CaptchaGenerationSettings GetSettings()
        {
            return GetSettings(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
        }

        private static CaptchaGenerationSettings GetSettings(TimeSpan expireDurationOfCaptcha, TimeSpan expireDurationOfAccessToken)
        {
            return new CaptchaGenerationSettings
            {
                Length = 5,
                Alphabet = "abcdefghijklmnopqrstuvwxyz",
                ExpireDurationOfCaptcha = expireDurationOfCaptcha,
                ExpireDurationOfAccessToken = expireDurationOfAccessToken,
            };
        }

        /// <summary>A manager which cleans up on every access, so that the cleanup does not have to be awaited.</summary>
        private static CaptchaManager GetManagerWhichAlwaysCleansUp()
        {
            return new CaptchaManager(TimeSpan.Zero);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void SolvingACaptchaWithTheCorrectInputSucceedsAndYieldsAnAcceptedAccessKey()
        {
            // arrange
            CaptchaManager captchaManager = GetManagerWhichAlwaysCleansUp();
            CaptchaInstance captcha = captchaManager.GetNewCaptcha(GetSettings());

            // act
            bool solved = captchaManager.TrySolve(captcha.Id, captcha.ExpectedUserInput, out string accessKey, out string failMessage);

            // assert
            Assert.IsTrue(solved, failMessage);
            Assert.IsTrue(captchaManager.UserHasAlreadySolvedTheCaptcha(accessKey, out string _));
        }

        /// <remarks>
        /// This testcase covers the finding S21: a solved captcha which stays in the store can be replayed until it
        /// expires, which removes the automation-barrier the captcha exists for.
        /// </remarks>
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ASolvedCaptchaCanNotBeSolvedASecondTime()
        {
            // arrange
            CaptchaManager captchaManager = GetManagerWhichAlwaysCleansUp();
            CaptchaInstance captcha = captchaManager.GetNewCaptcha(GetSettings());
            Assert.IsTrue(captchaManager.TrySolve(captcha.Id, captcha.ExpectedUserInput, out string _, out string _));

            // act
            bool solvedASecondTime = captchaManager.TrySolve(captcha.Id, captcha.ExpectedUserInput, out string accessKey, out string _);

            // assert
            Assert.IsFalse(solvedASecondTime);
            Assert.IsNull(accessKey);
            Assert.AreEqual(0, captchaManager.AmountOfStoredCaptchas);
        }

        /// <remarks>
        /// A captcha whose answer was guessed wrongly must be consumed as well, because otherwise the answer can be
        /// guessed by repeating the attempt with the same captcha-id.
        /// </remarks>
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AFailedAttemptConsumesTheCaptchaAsWell()
        {
            // arrange
            CaptchaManager captchaManager = GetManagerWhichAlwaysCleansUp();
            CaptchaInstance captcha = captchaManager.GetNewCaptcha(GetSettings());
            Assert.IsFalse(captchaManager.TrySolve(captcha.Id, "wrong-input", out string _, out string _));

            // act
            bool solvedAfterTheFailedAttempt = captchaManager.TrySolve(captcha.Id, captcha.ExpectedUserInput, out string _, out string _);

            // assert
            Assert.IsFalse(solvedAfterTheFailedAttempt, "The captcha must be consumed by the failed attempt.");
            Assert.AreEqual(0, captchaManager.AmountOfStoredCaptchas);
        }

        /// <remarks>
        /// This testcase covers the second half of the finding S21: without a cleanup both collections only grow,
        /// because a captcha which is generated but never solved is not removed anywhere else.
        /// </remarks>
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ExpiredCaptchasAreRemoved()
        {
            // arrange
            CaptchaManager captchaManager = GetManagerWhichAlwaysCleansUp();
            captchaManager.GetNewCaptcha(GetSettings(TimeSpan.Zero, TimeSpan.FromMinutes(10)));
            Assert.AreEqual(1, captchaManager.AmountOfStoredCaptchas);

            // act
            captchaManager.GetNewCaptcha(GetSettings());

            // assert: only the captcha which is not expired is left
            Assert.AreEqual(1, captchaManager.AmountOfStoredCaptchas);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ExpiredAccessKeysAreRemoved()
        {
            // arrange
            CaptchaManager captchaManager = GetManagerWhichAlwaysCleansUp();
            CaptchaInstance captcha = captchaManager.GetNewCaptcha(GetSettings(TimeSpan.FromMinutes(10), TimeSpan.Zero));
            Assert.IsTrue(captchaManager.TrySolve(captcha.Id, captcha.ExpectedUserInput, out string accessKey, out string _));
            Assert.AreEqual(1, captchaManager.AmountOfStoredAccessKeys);

            // act
            bool accepted = captchaManager.UserHasAlreadySolvedTheCaptcha(accessKey, out string failMessage);

            // assert
            Assert.IsFalse(accepted);
            Assert.AreEqual(0, captchaManager.AmountOfStoredAccessKeys);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AnUnknownCaptchaIsRejected()
        {
            // arrange
            CaptchaManager captchaManager = GetManagerWhichAlwaysCleansUp();

            // act
            bool solved = captchaManager.TrySolve(Guid.NewGuid().ToString(), "whatever", out string accessKey, out string failMessage);

            // assert
            Assert.IsFalse(solved);
            Assert.IsNull(accessKey);
            Assert.IsNotNull(failMessage);
        }
    }
}
