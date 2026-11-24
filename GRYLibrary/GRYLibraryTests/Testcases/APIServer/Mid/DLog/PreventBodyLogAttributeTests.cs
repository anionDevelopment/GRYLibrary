using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Verbs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GRYLibrary.Tests.Testcases.APIServer.Mid.DLog
{
    [TestClass]
    public class PreventBodyLogAttributeTests
    {
        private const string RequestBodyContent = "{\"password\":\"secret-request-content\"}";
        private const string ResponseBodyContent = "{\"accessToken\":\"secret-response-content\"}";

        private sealed class TestRunServer : RunServer
        {
        }

        /// <summary>Exposes <c>Log</c> and records every full log-entry, so that the formatted output can be checked.</summary>
        private sealed class TestDRequestLoggingMiddleware : DRequestLoggingMiddleware
        {
            public IList<string> FullLogEntries { get; } = new List<string>();

            public TestDRequestLoggingMiddleware(IDRequestLoggingConfiguration configuration) : base(_ => Task.CompletedTask, configuration, null!, ServerLog.GetTransientLog(), new TimeService(), new ServerConfiguration() { Domain = "test.example.com" }, new TestRunServer() { RealRun = false })
            {
            }

            public void LogForTest(HttpContext context, byte[] requestBody, byte[] responseBody)
            {
                this.Log(context, requestBody, responseBody);
            }

            public override string FormatLogEntryFull(Request request, TimeSpan? duration, ClaimsPrincipal user, uint maximalLengthOfRequestBodies, uint maximalLengthOfResponseBodies, IDictionary<string, IList<string?>> header)
            {
                string result = base.FormatLogEntryFull(request, duration, user, maximalLengthOfRequestBodies, maximalLengthOfResponseBodies, header);
                this.FullLogEntries.Add(result);
                return result;
            }
        }

        private sealed class TestController
        {
            public void WithoutAttribute()
            {
            }

            [PreventRequestBodyLog]
            public void WithPreventRequestBodyLog()
            {
            }

            [PreventResponseBodyLog]
            public void WithPreventResponseBodyLog()
            {
            }

            [PreventRequestBodyLog]
            [PreventResponseBodyLog]
            public void WithBothAttributes()
            {
            }
        }

        /// <summary>Logs a request against the given controller-method with a configuration which does not exclude any body, and returns the full log-entry.</summary>
        private static string LogRequestForMethod(string methodName)
        {
            MethodInfo methodInfo = typeof(TestController).GetMethod(methodName)!;
            ControllerActionDescriptor descriptor = new ControllerActionDescriptor() { MethodInfo = methodInfo };
            Endpoint endpoint = new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(descriptor), methodName);
            DefaultHttpContext context = new DefaultHttpContext();
            context.SetEndpoint(endpoint);
            context.Items["RequestId"] = "test-request-id";
            context.Request.Method = "POST";
            context.Request.Path = "/API/Test/" + methodName;
            context.Response.StatusCode = 200;

            DRequestLoggingConfiguration configuration = new DRequestLoggingConfiguration();
            TestDRequestLoggingMiddleware middleware = new TestDRequestLoggingMiddleware(configuration);
            middleware.LogForTest(context, Encoding.UTF8.GetBytes(RequestBodyContent), Encoding.UTF8.GetBytes(ResponseBodyContent));

            Assert.AreEqual(1, middleware.FullLogEntries.Count);
            return middleware.FullLogEntries[0];
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void BodiesAreLoggedWithoutAttribute()
        {
            // act
            string logEntry = LogRequestForMethod(nameof(TestController.WithoutAttribute));

            // assert
            StringAssert.Contains(logEntry, "secret-request-content");
            StringAssert.Contains(logEntry, "secret-response-content");
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PreventRequestBodyLogSuppressesOnlyRequestBody()
        {
            // act
            string logEntry = LogRequestForMethod(nameof(TestController.WithPreventRequestBodyLog));

            // assert
            Assert.IsFalse(logEntry.Contains("secret-request-content"));
            StringAssert.Contains(logEntry, "secret-response-content");
            StringAssert.Contains(logEntry, "(not logged)");
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PreventResponseBodyLogSuppressesOnlyResponseBody()
        {
            // act
            string logEntry = LogRequestForMethod(nameof(TestController.WithPreventResponseBodyLog));

            // assert
            StringAssert.Contains(logEntry, "secret-request-content");
            Assert.IsFalse(logEntry.Contains("secret-response-content"));
            StringAssert.Contains(logEntry, "(not logged)");
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void BothAttributesSuppressBothBodies()
        {
            // act
            string logEntry = LogRequestForMethod(nameof(TestController.WithBothAttributes));

            // assert
            Assert.IsFalse(logEntry.Contains("secret-request-content"));
            Assert.IsFalse(logEntry.Contains("secret-response-content"));
        }
    }
}
