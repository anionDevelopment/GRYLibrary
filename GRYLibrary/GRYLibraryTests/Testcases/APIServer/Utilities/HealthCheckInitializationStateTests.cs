using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GRYLibrary.Tests.Testcases.APIServer.Utilities
{
    [TestClass]
    public class HealthCheckInitializationStateTests
    {
        private sealed class FixedInitializationService : IInitializationService
        {
            private readonly InitializationState _State;

            public FixedInitializationService(InitializationState state)
            {
                this._State = state;
            }

            public InitializationState GetInitializationState()
            {
                return this._State;
            }
        }

        private static HealthCheckResult GetHealthCheckResult(InitializationState state)
        {
            Func<(HealthStatus, IEnumerable<string>)> healthyCheck = () => (HealthStatus.Healthy, new List<string>());
            return Tools.CheckHealthAsync(GeneralLogger.NoLog(), healthyCheck, new HealthCheckContext(), CancellationToken.None, new FixedInitializationService(state)).Result;
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void InitializingServiceIsReportedAsDegraded()
        {
            // act
            HealthCheckResult result = GetHealthCheckResult(new Initializing());

            // assert
            Assert.AreEqual(HealthStatus.Degraded, result.Status);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UninitializedServiceIsReportedAsDegraded()
        {
            // act
            HealthCheckResult result = GetHealthCheckResult(new Uninitialized());

            // assert
            Assert.AreEqual(HealthStatus.Degraded, result.Status);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void FailedInitializationIsReportedAsUnhealthy()
        {
            // act
            HealthCheckResult result = GetHealthCheckResult(new InitializationFailed());

            // assert
            Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void InitializedServiceReportsResultOfCheck()
        {
            // act
            HealthCheckResult result = GetHealthCheckResult(new Initialized());

            // assert
            Assert.AreEqual(HealthStatus.Healthy, result.Status);
        }
    }
}
