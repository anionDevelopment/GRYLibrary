using GRYLibrary.Core.APIServer.Services.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GRYLibrary.Tests.Testcases.APIServer.Services.Database
{
    [TestClass]
    public class ConnectionStringPasswordEscapingTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [DataRow("Host=h;Password=secret;Database=db", "Host=h;Password=********;Database=db")]
        [DataRow("Host=h;password=secret;Database=db", "Host=h;password=********;Database=db")]
        [DataRow("Host=h;PASSWORD=secret;Database=db", "Host=h;PASSWORD=********;Database=db")]
        [DataRow("Host=h;PWD=secret;Database=db", "Host=h;PWD=********;Database=db")]
        [DataRow("Host=h;pwd=secret;Database=db", "Host=h;pwd=********;Database=db")]
        [DataRow("Host=h;Pwd=secret;Database=db", "Host=h;Pwd=********;Database=db")]
        [DataRow("Host=h;PSW=secret;Database=db", "Host=h;PSW=********;Database=db")]
        [DataRow("Password=secret;Database=db", "Password=********;Database=db")]
        [DataRow("Database=db;Password=secret", "Database=db;Password=********")]
        [DataRow("Host=h; Password = secret ;Database=db", "Host=h; Password = ********;Database=db")]
        [DataRow("Host=h;Password=\"sec;ret\";Database=db", "Host=h;Password=********;Database=db")]
        [DataRow("Host=h;Password='sec;ret';Database=db", "Host=h;Password=********;Database=db")]
        [DataRow("Host=h;Password=\"sec\"\"ret\";Database=db", "Host=h;Password=********;Database=db")]
        public void PasswordIsEscaped(string connectionString, string expected)
        {
            // act
            string result = GenericDatabaseInteractor.GetConnectionStringWithEscapedPassword(connectionString);

            // assert
            Assert.AreEqual(expected, result);
            Assert.IsFalse(result.Contains("secret") || result.Contains("sec;ret") || result.Contains("ret"));
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void EveryPasswordIsEscaped()
        {
            // act
            string result = GenericDatabaseInteractor.GetConnectionStringWithEscapedPassword("Password=first;Database=db;pwd=second");

            // assert
            Assert.AreEqual("Password=********;Database=db;pwd=********", result);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        [DataRow("Host=h;Username=user;Database=db")]
        [DataRow("Host=h;OldPassword=x;Database=db")]
        [DataRow("Host=h;Database=passwords")]
        public void ConnectionStringWithoutPasswordIsNotChanged(string connectionString)
        {
            // act
            string result = GenericDatabaseInteractor.GetConnectionStringWithEscapedPassword(connectionString);

            // assert
            Assert.AreEqual(connectionString, result);
        }
    }
}
