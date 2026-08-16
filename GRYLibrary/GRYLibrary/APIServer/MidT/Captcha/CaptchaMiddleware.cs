using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.MidT.Captcha
{
    public abstract class CaptchaMiddleware : AbstractMiddleware
    {
        /// <inheritdoc/>
        public CaptchaMiddleware(RequestDelegate next) : base(next)
        {
        }
        /// <inheritdoc/>
        public override Task Invoke(HttpContext context)
        {
            if (this.UserHasAlreadySolvedTheCaptcha(context))
            {
                return this._Next(context);
            }
            else
            {
                return this.GetCaptcha();
            }
        }

        protected abstract bool UserHasAlreadySolvedTheCaptcha(HttpContext context);

        protected abstract Task GetCaptcha();
    }
}