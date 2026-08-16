namespace GRYLibrary.Core.Misc.Captcha
{
    public interface ICaptchaManager
    {
        public CaptchaInstance GetNewCaptcha(CaptchaGenerationSettings settings);
        public bool TrySolve(string captchaId, string userInput, out string accessKey, out string failMessage);
        /// <summary>
        /// Can optionally be used to skip captcha-validation for actions which only should be captcha-protected as long as the user not have a valid accessKey.
        /// </summary>
        internal bool UserHasAlreadySolvedTheCaptcha(string accessToken, out string failMessage);
    }
}
