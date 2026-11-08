using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Tests.Testcases.APIServer.Services.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Tests.Testcases.APIServer.Utilities
{
    [TestClass]
    public class WaitUntilDatabaseIsAvailableTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DatabaseWhichIsStartingUpDoesNotConsumeAttempts()
        {
            Exception startingUp = new DependencyNotAvailableException("Not connected", DatabaseStartingUpDetectionTests.CreatePostgresException(PostgresErrorCodes.CannotConnectNow));
            ReconnectableDatabaseStub database = new ReconnectableDatabaseStub([(false, startingUp), (false, startingUp), (true, null)]);

            Tools.WaitUntilDatabaseIsAvailable(database, GeneralLogger.NoLog(), 1, 0);

            Assert.AreEqual(0, database.RemainingResults);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void UnavailableDatabaseConsumesAttempts()
        {
            Exception refused = new DependencyNotAvailableException("Not connected", new InvalidOperationException("Connection refused"));
            ReconnectableDatabaseStub database = new ReconnectableDatabaseStub([(false, refused), (false, refused), (true, null)]);

            Assert.ThrowsExactly<DependencyNotAvailableException>(() => Tools.WaitUntilDatabaseIsAvailable(database, GeneralLogger.NoLog(), 1, 0));
            Assert.AreEqual(1, database.RemainingResults);
        }

        private sealed class ReconnectableDatabaseStub : IReconnectableDatabase
        {
            private readonly Queue<(bool, Exception?)> _Results;

            public ReconnectableDatabaseStub(IEnumerable<(bool, Exception?)> results)
            {
                this._Results = new Queue<(bool, Exception?)>(results);
            }

            public int RemainingResults => this._Results.Count;

            public (bool, Exception?) IsAvailable()
            {
                return this._Results.Dequeue();
            }

            public void SetLogConnectionAttemptErrors(bool enabled)
            {
                GRYLibrary.Core.Misc.Utilities.NoOperation();
            }

            public void Dispose()
            {
                GRYLibrary.Core.Misc.Utilities.NoOperation();
            }
        }
    }
}
