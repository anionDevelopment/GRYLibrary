using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System;

namespace GRYLibrary.Tests.Testcases.APIServer.Services.Database
{
    [TestClass]
    public class DatabaseStartingUpDetectionTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PostgreSQLCannotConnectNowIsDetected()
        {
            Assert.IsTrue(DBUtilities.IsCausedByDatabaseStartingUp(CreatePostgresException(PostgresErrorCodes.CannotConnectNow)));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PostgreSQLCannotConnectNowIsDetectedAsNestedInnerException()
        {
            Exception exception = new DependencyNotAvailableException("Not connected", new DependencyNotAvailableException("Connection is null.", CreatePostgresException(PostgresErrorCodes.CannotConnectNow)));

            Assert.IsTrue(DBUtilities.IsCausedByDatabaseStartingUp(exception));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void OtherPostgreSQLErrorIsNotDetected()
        {
            Exception exception = new DependencyNotAvailableException("Not connected", CreatePostgresException(PostgresErrorCodes.InvalidPassword));

            Assert.IsFalse(DBUtilities.IsCausedByDatabaseStartingUp(exception));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void ExceptionWithoutPostgreSQLErrorIsNotDetected()
        {
            Assert.IsFalse(DBUtilities.IsCausedByDatabaseStartingUp(new DependencyNotAvailableException("Connection is null.", new InvalidOperationException("Connection refused"))));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void NullIsNotDetected()
        {
            Assert.IsFalse(DBUtilities.IsCausedByDatabaseStartingUp(null));
        }

        internal static PostgresException CreatePostgresException(string sqlState)
        {
            return new PostgresException("Example-error", "FATAL", "FATAL", sqlState);
        }
    }
}
