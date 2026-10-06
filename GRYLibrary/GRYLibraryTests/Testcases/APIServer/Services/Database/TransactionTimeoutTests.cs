using GRYLibrary.Core.APIServer.Services.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using Npgsql;

namespace GRYLibrary.Tests.Testcases.APIServer.Services.Database
{
    /// <summary>
    /// The database-providers complete a transaction (commit/rollback) with the default command-timeout of the connection and not with the timeout of the
    /// commands of the transaction, so the connection-string must carry a timeout which is at least as high as <see cref="DBUtilities.TransactionTimeoutInSeconds"/>.
    /// </summary>
    [TestClass]
    public class TransactionTimeoutTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DefaultCommandTimeoutIsRaisedToTransactionTimeout()
        {
            Assert.AreEqual(DBUtilities.TransactionTimeoutInSeconds, DBUtilities.GetDefaultCommandTimeoutForConnection(30));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void HigherDefaultCommandTimeoutIsKept()
        {
            Assert.AreEqual(DBUtilities.TransactionTimeoutInSeconds + 1, DBUtilities.GetDefaultCommandTimeoutForConnection(DBUtilities.TransactionTimeoutInSeconds + 1));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void InfiniteDefaultCommandTimeoutIsKept()
        {
            Assert.AreEqual(0, DBUtilities.GetDefaultCommandTimeoutForConnection(0));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void PostgreSQLConnectionStringGetsTransactionTimeout()
        {
            string adjusted = PostgreSQLDatabaseInteractor.AdjustConnectionString("Host=myhost;Port=5432;Username=user;Password=pw;Database=db");

            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(adjusted);
            Assert.AreEqual(DBUtilities.TransactionTimeoutInSeconds, builder.CommandTimeout);
            Assert.AreEqual("myhost", builder.Host);
            Assert.AreEqual("pw", builder.Password);
            Assert.AreEqual("db", builder.Database);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void MariaDBConnectionStringGetsTransactionTimeout()
        {
            string adjusted = MariaDBDatabaseInteractor.AdjustConnectionString("Server=myhost;Port=3306;User ID=user;Password=pw;Database=db");

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(adjusted);
            Assert.AreEqual((uint)DBUtilities.TransactionTimeoutInSeconds, builder.DefaultCommandTimeout);
            Assert.AreEqual("myhost", builder.Server);
            Assert.AreEqual("pw", builder.Password);
            Assert.AreEqual("db", builder.Database);
        }
    }
}
