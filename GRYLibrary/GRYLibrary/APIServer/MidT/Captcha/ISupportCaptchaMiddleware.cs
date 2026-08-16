namespace GRYLibrary.Core.APIServer.MidT.Captcha
{
    public interface ISupportCaptchaMiddleware : ISupportedMiddleware
    {
        ICaptchaConfiguration ConfigurationForCaptchaMiddleware { get; }
    }
}
