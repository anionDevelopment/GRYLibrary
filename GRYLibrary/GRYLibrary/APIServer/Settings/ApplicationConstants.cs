using GRYLibrary.Core.APIServer.CommonRoutes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.MaintenanceRoutes;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.FilePath;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Settings
{
    /// <summary>
    /// Represents Application-constants which are not editable by a configuration-file.
    /// </summary>
    public interface IApplicationConstants
    {
        public string BaseFolder { get; }
        public string ApplicationName { get; set; }
        public bool UseWebSockets { get; set; }
        public string ApplicationDescription { get; set; }
        public IList<Regulation> Regulations { get; set; }
        public Version3 ApplicationVersion { get; set; }
        public ExecutionMode ExecutionMode { get; set; }
        public GRYEnvironment Environment { get; set; }
        public AbstractFilePath DataFolder { get; set; }
        public AbstractFilePath ConfigurationFolder { get; set; }
        public AbstractFilePath CertificateFolder { get; set; }
        public AbstractFilePath ConfigurationFile { get; set; }
        public AbstractFilePath LogFolder { get; set; }
        public bool AdminHasToEnterInformationAfterInitialConfigurationFileGeneration { get; set; }
        public bool ThrowErrorIfConfigurationDoesNotExistInProduction { get; set; }
        public string GetDataFolder();
        public string GetConfigurationFolder();
        public string GetCertificateFolder();
        public string GetConfigurationFile();
        public string GetLogFolder();
        /// <summary>
        /// Thir property only applies for if the envirnonment is not <see cref="Development"/>.
        /// </summary>
        public CommonRoutesHostInformation CommonRoutesHostInformation { get; set; }
        public AbstractHostMaintenanceInformation HostMaintenanceInformation { get; set; }
        public void Initialize(string baseFolder);
        public Type AuthenticationMiddleware { get; set; }
        public Type AuthorizationMiddleware { get; set; }
        public Type BlackListMiddleware { get; set; }
        public Type CaptchaMiddleware { get; set; }
        public Type DDOSProtectionMiddleware { get; set; }
        public Type ExceptionManagerMiddleware { get; set; }
        public Type ObfuscationMiddleware { get; set; }
        public Type RequestCounterMiddleware { get; set; }
        public Type LoggingMiddleware { get; set; }
        public Type WebApplicationFirewallMiddleware { get; set; }
        public Type MaintenanceSiteMiddleware { get; set; }
        public IList<Type> CustomMiddlewares1 { get; set; }
        public IList<Type> CustomMiddlewares2 { get; set; }
        public ISet<Type> KnownTypes { get; set; }
        public bool ListenOnEveryIP { get; set; }
    }
    public interface IApplicationConstants<AppSpecificConstants> : IApplicationConstants
    {
        public AppSpecificConstants ApplicationSpecificConstants { get; set; }
    }
    public class ApplicationConstants<AppSpecificConstants> : IApplicationConstants<AppSpecificConstants>
    {
        public ApplicationConstants(string applicationName, string appDescription, Version3 applicationVersion, ExecutionMode executionMode, GRYEnvironment environment, AppSpecificConstants applicationSpecificConstants)
        {
            this.ApplicationName = applicationName;
            this.ApplicationDescription = appDescription;
            this.ApplicationVersion = applicationVersion;
            this.ExecutionMode = executionMode;
            this.Environment = environment;
            this.ApplicationSpecificConstants = applicationSpecificConstants;
            this.ConfigurationFolder = AbstractFilePath.FromString("./Configuration");
            this.CertificateFolder = AbstractFilePath.FromString("./Certificates");
            this.DataFolder = AbstractFilePath.FromString("./Data");
            this.ConfigurationFile = AbstractFilePath.FromString("./Configuration.xml");
            this.LogFolder = AbstractFilePath.FromString("./Logs");
            this.UseWebSockets = false;
            this.ListenOnEveryIP = true;
        }
        public bool AdminHasToEnterInformationAfterInitialConfigurationFileGeneration { get; set; } = false;
        public bool ThrowErrorIfConfigurationDoesNotExistInProduction { get; set; } = false;
        public string BaseFolder { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationDescription { get; set; }
        public IList<Regulation> Regulations { get; set; } = [];
        public Version3 ApplicationVersion { get; set; }
        public ExecutionMode ExecutionMode { get; set; }
        public GRYEnvironment Environment { get; set; }
        public AppSpecificConstants ApplicationSpecificConstants { get; set; }
        public AbstractFilePath DataFolder { get; set; }
        public AbstractFilePath ConfigurationFolder { get; set; }
        public AbstractFilePath CertificateFolder { get; set; }
        public AbstractFilePath ConfigurationFile { get; set; }
        public AbstractFilePath LogFolder { get; set; }
        public string GetDataFolder()
        {
            return this.DataFolder.GetPath(this.BaseFolder);
        }

        public string GetConfigurationFolder()
        {
            string result = this.ConfigurationFolder.GetPath(this.BaseFolder);
            return result;
        }

        public string GetCertificateFolder()
        {
            return this.CertificateFolder.GetPath(this.GetConfigurationFolder());
        }

        public string GetConfigurationFile()
        {
            return this.ConfigurationFile.GetPath(this.GetConfigurationFolder());
        }

        public string GetLogFolder()
        {
            string result = this.LogFolder.GetPath(this.BaseFolder);
            return result;
        }

        public CommonRoutesHostInformation CommonRoutesHostInformation { get; set; } = new HostCommonRoutes();
        public AbstractHostMaintenanceInformation HostMaintenanceInformation { get; set; } = new HostMaintenanceRoutes();
        public Type AuthenticationMiddleware { get; set; } = null;
        public Type AuthorizationMiddleware { get; set; } = null;
        public Type BlackListMiddleware { get; set; } = null;
        public Type CaptchaMiddleware { get; set; } = null;
        public Type DDOSProtectionMiddleware { get; set; } = null;
        public Type ExceptionManagerMiddleware { get; set; } = null;
        public Type ObfuscationMiddleware { get; set; } = null;
        public Type RequestCounterMiddleware { get; set; } = null;
        public Type LoggingMiddleware { get; set; } = null;
        public Type WebApplicationFirewallMiddleware { get; set; } = null;
        public Type MaintenanceSiteMiddleware { get; set; } = null;
        public IList<Type> CustomMiddlewares1 { get; set; } = [];
        public IList<Type> CustomMiddlewares2 { get; set; } = [];
        public ISet<Type> KnownTypes { get; set; } = new HashSet<Type>();
        public bool UseWebSockets { get; set; }
        public bool ListenOnEveryIP { get; set; }

        public void Initialize(string baseFolder)
        {
            this.BaseFolder = baseFolder;
        }
    }
}
