using GRYLibrary.Core.APIServer.MidT.Aut;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;

namespace GRYLibrary.Tests.Testcases.APIServer.Mid.Aut
{
    [TestClass]
    public class AuthorizationMiddlewareTests
    {
        private sealed class TestAuthorizationMiddleware : AuthorizationMiddleware
        {
            private readonly bool _IsAuthorizedResult;
            public TestAuthorizationMiddleware(IGRYLog log, bool isAuthorizedResult) : base(_ => Task.CompletedTask, log, null!)
            {
                this._IsAuthorizedResult = isAuthorizedResult;
            }
            protected override bool IsAuthorized(HttpContext context)
            {
                return this._IsAuthorizedResult;
            }
        }

        private sealed class TestController
        {
            public void WithoutAttribute()
            {
            }

            [Authorize("some-group")]
            public void WithGroups()
            {
            }

            [Authorize]
            public void WithEmptyGroups()
            {
            }
        }

        private static HttpContext BuildContextForMethod(string methodName)
        {
            MethodInfo methodInfo = typeof(TestController).GetMethod(methodName)!;
            ControllerActionDescriptor descriptor = new ControllerActionDescriptor() { MethodInfo = methodInfo };
            Endpoint endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(descriptor), methodName);
            DefaultHttpContext context = new DefaultHttpContext();
            context.SetEndpoint(endpoint);
            context.Items["RequestId"] = "test-request-id";
            return context;
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AuthorizationIsNotRequiredWithoutAuthorizeAttribute()
        {
            GRYLog log = GRYLog.Create();
            TestAuthorizationMiddleware middleware = new TestAuthorizationMiddleware(log, false);
            Assert.IsFalse(middleware.AuthorizationIsRequired(BuildContextForMethod(nameof(TestController.WithoutAttribute))));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AuthorizationIsRequiredWithNonEmptyGroups()
        {
            GRYLog log = GRYLog.Create();
            TestAuthorizationMiddleware middleware = new TestAuthorizationMiddleware(log, false);
            Assert.IsTrue(middleware.AuthorizationIsRequired(BuildContextForMethod(nameof(TestController.WithGroups))));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void AuthorizationIsRequiredWithEmptyGroups()
        {
            // Regression-test for SEC-4: an AuthorizeAttribute without groups must still require the authorization-check instead of admitting every authenticated user.
            GRYLog log = GRYLog.Create();
            TestAuthorizationMiddleware middleware = new TestAuthorizationMiddleware(log, false);
            Assert.IsTrue(middleware.AuthorizationIsRequired(BuildContextForMethod(nameof(TestController.WithEmptyGroups))));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void RequestIsDeniedWithEmptyGroupsWhenNotAuthorized()
        {
            // Regression-test for SEC-4: with an empty-groups AuthorizeAttribute and a failing authorization-check the request must be denied (fail-closed) instead of passed through.
            GRYLog log = GRYLog.Create();
            TestAuthorizationMiddleware middleware = new TestAuthorizationMiddleware(log, false);
            HttpContext context = BuildContextForMethod(nameof(TestController.WithEmptyGroups));
            BadRequestException exception = Assert.ThrowsExactly<BadRequestException>(() => middleware.Invoke(context));
            Assert.AreEqual((ushort)StatusCodes.Status403Forbidden, exception.HTTPStatusCode);
        }
    }
}
