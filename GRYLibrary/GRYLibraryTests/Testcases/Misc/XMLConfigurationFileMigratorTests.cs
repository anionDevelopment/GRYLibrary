using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc.MetaConfiguration;
using GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats;
using GRYLibrary.Core.Misc.MetaConfiguration.Migrations;
using GRYLibrary.Core.XMLSerializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace GRYLibrary.Tests.Testcases.Misc
{
    [TestClass]
    public class XMLConfigurationFileMigratorTests
    {
        public class ConfigurationWithRequestLogging
        {
            public DRequestLoggingConfiguration ConfigurationForDLoggingMiddleware { get; set; } = new DRequestLoggingConfiguration();
        }

        private const string TestMigrationsResourceNamePrefix = "GRYLibrary.Tests.ConfigurationFileMigrations.";
        private static readonly Encoding _Encoding = new UTF8Encoding(false);

        private static string CreateTestFolder()
        {
            string folder = Path.Combine(Path.GetTempPath(), $"GRYLibraryTests_{nameof(XMLConfigurationFileMigratorTests)}_{Guid.NewGuid():N}");
            Directory.CreateDirectory(folder);
            return folder;
        }

        private static XMLConfigurationFileMigrator CreateMigrator()
        {
            return new XMLConfigurationFileMigrator(new TimeService(), GeneralLogger.NoLog());
        }

        /// <summary>
        /// Returns a xslt which renames every element with the name <paramref name="oldName"/> to <paramref name="newName"/>.
        /// </summary>
        private static string GetRenameElementXSLT(string oldName, string newName)
        {
            return $@"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
  <xsl:template match=""@*|node()""><xsl:copy><xsl:apply-templates select=""@*|node()""/></xsl:copy></xsl:template>
  <xsl:template match=""*[local-name()='{oldName}']""><xsl:element name=""{newName}"" namespace=""{{namespace-uri()}}""><xsl:apply-templates select=""@*|node()""/></xsl:element></xsl:template>
</xsl:stylesheet>";
        }

        /// <summary>
        /// Returns the content of <paramref name="document"/> with the attributes (including the namespace-declarations) of every element ordered by their name, because a xslt-transformation does not keep the order of them.
        /// </summary>
        private static string Normalize(XDocument document)
        {
            foreach (XElement element in document.Descendants())
            {
                List<XAttribute> attributes = [.. element.Attributes().OrderBy(attribute => attribute.Name.ToString(), StringComparer.Ordinal)];
                element.RemoveAttributes();
                element.Add(attributes);
            }
            return document.Root!.ToString(SaveOptions.DisableFormatting);
        }

        private static string CreateOldRequestLoggingConfigurationFile(string folder)
        {
            ConfigurationWithRequestLogging configuration = new ConfigurationWithRequestLogging();
            configuration.ConfigurationForDLoggingMiddleware.MaximalLengthOfRequestBodies = 1234;
            configuration.ConfigurationForDLoggingMiddleware.LoggedHTTPRequestHeader = new HashSet<string>() { "X-Test-Header" };
            string newContent = new SimpleGenericXMLSerializer<ConfigurationWithRequestLogging>().Serialize(configuration);
            string oldContent = newContent.Replace("MaximalLengthOfRequestBodies", "MaximalLengthofRequestBodies").Replace("LoggedHTTPRequestHeader", "LoggedHTTPRequeustHeader");
            Assert.AreNotEqual(newContent, oldContent);
            string file = Path.Combine(folder, "Configuration.xml");
            File.WriteAllText(file, oldContent, _Encoding);
            return file;
        }

        private static MetaConfigurationSettings<ConfigurationWithRequestLogging, object> CreateSettings(string file, IList<ConfigurationFileMigration> migrations)
        {
            return new MetaConfigurationSettings<ConfigurationWithRequestLogging, object>()
            {
                ConfigurationFormat = XML.Instance,
                File = file,
                InitialValue = new ConfigurationWithRequestLogging(),
                Migrations = migrations,
                MigrationLog = GeneralLogger.NoLog(),
            };
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void InternalMigrationsAreLoadedFromEmbeddedResources()
        {
            // act
            IList<ConfigurationFileMigration> migrations = APIServerConfigurationFileMigrations.GetInternalMigrations();

            // assert
            Assert.IsTrue(migrations.Any(migration => migration.Name == APIServerConfigurationFileMigrations.InternalMigrationsResourceNamePrefix + "Migration000001"));
            Assert.IsTrue(migrations.All(migration => migration.Name.StartsWith(APIServerConfigurationFileMigrations.InternalMigrationsResourceNamePrefix, StringComparison.Ordinal)));
            CollectionAssert.AreEqual(migrations.Select(migration => migration.Name).Order(StringComparer.Ordinal).ToList(), migrations.Select(migration => migration.Name).ToList());
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void CustomMigrationsAreLoadedFromEmbeddedResourcesOfTheGivenAssembly()
        {
            // act
            IList<ConfigurationFileMigration> migrations = ConfigurationFileMigration.LoadFromEmbeddedResources(typeof(XMLConfigurationFileMigratorTests).Assembly, TestMigrationsResourceNamePrefix);

            // assert
            Assert.HasCount(1, migrations);
            Assert.AreEqual(TestMigrationsResourceNamePrefix + "Migration000001", migrations[0].Name);
            Assert.Contains("xsl:stylesheet", migrations[0].XSLT);
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void OldRequestLoggingConfigurationIsMigratedByInternalMigration()
        {
            string folder = CreateTestFolder();
            try
            {
                // arrange
                string file = CreateOldRequestLoggingConfigurationFile(folder);

                // act
                ConfigurationWithRequestLogging result = MetaConfigurationManager.GetConfiguration(CreateSettings(file, APIServerConfigurationFileMigrations.GetAllMigrations([])), new HashSet<Type>(), out bool fileWasCreatedNew);

                // assert
                Assert.IsFalse(fileWasCreatedNew);
                Assert.AreEqual(1234u, result.ConfigurationForDLoggingMiddleware.MaximalLengthOfRequestBodies);
                CollectionAssert.AreEquivalent(new List<string>() { "X-Test-Header" }, result.ConfigurationForDLoggingMiddleware.LoggedHTTPRequestHeader.ToList());
                string migratedContent = File.ReadAllText(file, _Encoding);
                Assert.DoesNotContain("MaximalLengthofRequestBodies", migratedContent);
                Assert.DoesNotContain("LoggedHTTPRequeustHeader", migratedContent);
                Assert.HasCount(1, Directory.GetFiles(folder, "Configuration.xml.bak-*"));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void InternalMigrationIsIdempotent()
        {
            string folder = CreateTestFolder();
            try
            {
                // arrange
                string file = Path.Combine(folder, "Configuration.xml");
                string currentContent = new SimpleGenericXMLSerializer<ConfigurationWithRequestLogging>().Serialize(new ConfigurationWithRequestLogging());
                File.WriteAllText(file, currentContent, _Encoding);

                // act
                CreateMigrator().MigrateIfRequired(file, APIServerConfigurationFileMigrations.GetInternalMigrations());

                // assert
                Assert.AreEqual(Normalize(XDocument.Parse(currentContent)), Normalize(XDocument.Load(file)));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void MigrationsAreExecutedOnlyOnce()
        {
            string folder = CreateTestFolder();
            try
            {
                // arrange
                string file = Path.Combine(folder, "Configuration.xml");
                File.WriteAllText(file, "<Root><A/></Root>", _Encoding);
                IList<ConfigurationFileMigration> migrations = [new ConfigurationFileMigration("RenameAToB", GetRenameElementXSLT("A", "B"))];
                XMLConfigurationFileMigrator migrator = CreateMigrator();
                migrator.MigrateIfRequired(file, migrations);
                File.WriteAllText(file, "<Root><A/></Root>", _Encoding);

                // act
                IList<string> executedMigrations = migrator.MigrateIfRequired(file, migrations);

                // assert
                Assert.HasCount(0, executedMigrations);
                Assert.Contains("<A", File.ReadAllText(file, _Encoding));
                CollectionAssert.AreEqual(new List<string>() { "RenameAToB" }, XMLConfigurationFileMigrator.LoadNamesOfExecutedMigrations(XMLConfigurationFileMigrator.GetMigrationInformationFile(file)).ToList());
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void CustomMigrationsAreExecutedAfterInternalMigrations()
        {
            string folder = CreateTestFolder();
            try
            {
                // arrange
                string file = CreateOldRequestLoggingConfigurationFile(folder);
                // The custom migration renames the property which got its current name by the internal migration, so it only has an effect if it runs after the internal migration.
                IList<ConfigurationFileMigration> customMigrations = ConfigurationFileMigration.LoadFromEmbeddedResources(typeof(XMLConfigurationFileMigratorTests).Assembly, TestMigrationsResourceNamePrefix);

                // act
                IList<string> executedMigrations = CreateMigrator().MigrateIfRequired(file, APIServerConfigurationFileMigrations.GetAllMigrations(customMigrations));

                // assert
                Assert.AreEqual(customMigrations.Last().Name, executedMigrations.Last());
                string migratedContent = File.ReadAllText(file, _Encoding);
                Assert.Contains("RenamedMaximalLengthOfRequestBodies", migratedContent);
                Assert.DoesNotContain("MaximalLengthofRequestBodies", migratedContent);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void NewConfigurationFileIsMarkedAsMigrated()
        {
            string folder = CreateTestFolder();
            try
            {
                // arrange
                string file = Path.Combine(folder, "Configuration.xml");
                IList<ConfigurationFileMigration> migrations = APIServerConfigurationFileMigrations.GetAllMigrations([]);

                // act
                MetaConfigurationManager.GetConfiguration(CreateSettings(file, migrations), new HashSet<Type>(), out bool fileWasCreatedNew);

                // assert
                Assert.IsTrue(fileWasCreatedNew);
                CollectionAssert.AreEqual(migrations.Select(migration => migration.Name).ToList(), XMLConfigurationFileMigrator.LoadNamesOfExecutedMigrations(XMLConfigurationFileMigrator.GetMigrationInformationFile(file)).ToList());
                Assert.HasCount(0, Directory.GetFiles(folder, "Configuration.xml.bak-*"));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DuplicatedMigrationNamesAreRejected()
        {
            string folder = CreateTestFolder();
            try
            {
                // arrange
                string file = Path.Combine(folder, "Configuration.xml");
                File.WriteAllText(file, "<Root><A/></Root>", _Encoding);
                IList<ConfigurationFileMigration> migrations = [new ConfigurationFileMigration("SameName", GetRenameElementXSLT("A", "B")), new ConfigurationFileMigration("SameName", GetRenameElementXSLT("B", "C"))];

                // act & assert
                Assert.Throws<Exception>(() => CreateMigrator().MigrateIfRequired(file, migrations));
                Assert.Contains("<A", File.ReadAllText(file, _Encoding));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
