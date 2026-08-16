using Microsoft.AspNetCore.Http;

namespace GRYLibrary.Core.APIServer.MidT.General
{
    public abstract class GeneralMiddlewareT : AbstractMiddleware
    {
        protected GeneralMiddlewareT(RequestDelegate next) : base(next)
        {
        }
    }
}
