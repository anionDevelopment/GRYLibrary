using GRYLibrary.Core.APIServer.CommonRoutes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.ExecutionModes.Visitors;
using GRYLibrary.Core.APIServer.Formatter;
using GRYLibrary.Core.APIServer.Mid.General;
using GRYLibrary.Core.APIServer.MidT;
using GRYLibrary.Core.APIServer.MidT.Aut;
using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.MidT.Captcha;
using GRYLibrary.Core.APIServer.MidT.Exception;
using GRYLibrary.Core.APIServer.MidT.Maint;
using GRYLibrary.Core.APIServer.MidT.Obfuscation;
using GRYLibrary.Core.APIServer.MidT.RLog;
using GRYLibrary.Core.APIServer.MidT.WAF;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Verbs;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.ConsoleApplication;
using GRYLibrary.Core.Misc.FilePath;
using GRYLibrary.Core.Misc.MetaConfiguration;
using GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer
{
    /// <summary>
    /// Represents a webserver for a cloud-native HTTP-API-server
    /// </summary>
    public class APIServer<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>
        where PersistedApplicationSpecificConfiguration : new()
        where ApplicationSpecificConstants : new()
        where CommandlineParameterType : class, IAPIServerCommandlineParameter
    {
        private bool _MaintenanceModeIsImplementable = false;
        private bool _ManualMaintenanceModeEnabled = false;
        private bool _TechnicalMaintenanceModeEnabled = false;
        private IApplicationConstants<ApplicationSpecificConstants>? _ApplicationConstants = null;
        /// <summary>
        /// Enables maintenancemode desired by user
        /// </summary>
        private void SetManualMaintenanceModeEnabled(bool enabled, IGRYLog log)
        {
            this._ManualMaintenanceModeEnabled = enabled;
            this.UpdateMaintenancePageIndicator(log);
        }
        private bool GetManualMaintenanceModeEnabled()
        {
            return this._ManualMaintenanceModeEnabled;
        }
        /// <summary>
        /// Enables maintenancemode required by server
        /// </summary>
        private void SetTechnicalMaintenanceModeEnabled(bool enabled, IGRYLog log)
        {
            this._TechnicalMaintenanceModeEnabled = enabled;
            this.UpdateMaintenancePageIndicator(log);
        }

        private void UpdateMaintenancePageIndicator(IGRYLog log)
        {
            if (!this._MaintenanceModeIsImplementable)
            {
                return;
            }
            bool maintenanceModeEnabled = this._ManualMaintenanceModeEnabled || this._TechnicalMaintenanceModeEnabled;
            IApplicationConstants<ApplicationSpecificConstants> constants = GRYLibrary.Core.Misc.Utilities.AssertNotNull(this._ApplicationConstants, nameof(this._ApplicationConstants));
            string configFolder = constants.GetConfigurationFolder();
            GRYLibrary.Core.Misc.Utilities.AssertCondition(Directory.Exists(configFolder), $"Folder \"{configFolder}\" does not exist.");
            string InitializationFile = Path.Combine(configFolder, "MaintenanceMode.enabled");
            log.Log($"Set maintenance mode to {maintenanceModeEnabled}", LogLevel.Information);
            log.Log($"Maintenance-File: {InitializationFile}; {nameof(this._ManualMaintenanceModeEnabled)}: {this._ManualMaintenanceModeEnabled}; {nameof(this._TechnicalMaintenanceModeEnabled)}: {this._TechnicalMaintenanceModeEnabled}", LogLevel.Debug);
            try
            {
                if (maintenanceModeEnabled)
                {
                    GRYLibrary.Core.Misc.Utilities.EnsureFileExists(InitializationFile);
                }
                else
                {
                    GRYLibrary.Core.Misc.Utilities.EnsureFileDoesNotExist(InitializationFile);
                }
                GRYLibrary.Core.Misc.Utilities.AssertCondition(File.Exists(InitializationFile) == maintenanceModeEnabled, $"test1 failed");
            }
            catch (Exception e)
            {
                log.Log($"Error while setting maintenance-indicator-file", e);
            }
        }

        private APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> _Configuration;
        public APIServer()
        {
        }

        public static Func<CommandlineParameterType, GRYConsoleApplicationInitialInformation, int> CreateMain(Action<APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>> init, IGRYLog initialLog)
        {
            return (CommandlineParameterType commandlineParameter, GRYConsoleApplicationInitialInformation gryConsoleApplicationInitialInformation) =>
            {
                try
                {
                    APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> apiServerConfiguration = new APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>();
                    apiServerConfiguration.CommandlineParameter = commandlineParameter;
                    apiServerConfiguration.CancellationTokenSource = new CancellationTokenSource();
                    init(apiServerConfiguration);
                    return APIMain(commandlineParameter, gryConsoleApplicationInitialInformation, apiServerConfiguration, initialLog);
                }
                catch
                {
                    throw;
                }
            };
        }

        public static int APIMain(CommandlineParameterType commandlineParameter, GRYConsoleApplicationInitialInformation gryConsoleApplicationInitialInformation, APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> apiServerConfiguration, IGRYLog initialLogger)
        {
            try
            {

                #region Initialize default configuration-values
                apiServerConfiguration.InitializationInformation = new InitializationInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>
                {
                    CommandlineParameter = commandlineParameter,
                    ApplicationConstants = new ApplicationConstants<ApplicationSpecificConstants>(gryConsoleApplicationInitialInformation.ProgramName, gryConsoleApplicationInitialInformation.ProgramDescription, Version3.Parse(gryConsoleApplicationInitialInformation.ProgramVersion), gryConsoleApplicationInitialInformation.ExecutionMode, gryConsoleApplicationInitialInformation.Environment, new ApplicationSpecificConstants())
                };
                apiServerConfiguration.InitializationInformation.InitialLogger = initialLogger;
                apiServerConfiguration.InitializationInformation.BaseFolder = GetDefaultBaseFolder(apiServerConfiguration.InitializationInformation.ApplicationConstants, !apiServerConfiguration.CommandlineParameter.RealRun);
                apiServerConfiguration.InitializationInformation.ApplicationConstants.Initialize(apiServerConfiguration.InitializationInformation.BaseFolder);
                apiServerConfiguration.InitializationInformation.ApplicationConstants.KnownTypes.Add(typeof(PersistedApplicationSpecificConfiguration));
                apiServerConfiguration.InitializationInformation.InitialApplicationConfiguration = PersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration>.Create(new PersistedApplicationSpecificConfiguration(), gryConsoleApplicationInitialInformation.Environment);
                apiServerConfiguration.InitializationInformation.BasicInformationFile = AbstractFilePath.FromString("./BasicApplicationInformation.xml");
                apiServerConfiguration.InitializationInformation.InitialLogger.Log($"Base-folder: {apiServerConfiguration.InitializationInformation.ApplicationConstants.BaseFolder}", LogLevel.Debug);
                apiServerConfiguration.InitializationInformation.InitialLogger.Log($"Configuration-folder: {apiServerConfiguration.InitializationInformation.ApplicationConstants.ConfigurationFolder}", LogLevel.Debug);
                apiServerConfiguration.InitializationInformation.InitialLogger.Log($"Data-folder: {apiServerConfiguration.InitializationInformation.ApplicationConstants.GetDataFolder()}", LogLevel.Debug);
                apiServerConfiguration.InitializationInformation.InitialLogger.Log($"Log-folder: {apiServerConfiguration.InitializationInformation.ApplicationConstants.GetLogFolder()}", LogLevel.Debug);
                apiServerConfiguration.InitializationInformation.InitialLogger.Log($"Run {nameof(APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>.SetInitializationInformationAction)}...", LogLevel.Debug);
                apiServerConfiguration.SetInitializationInformationAction(apiServerConfiguration.InitializationInformation);
                #endregion

                #region Load configuration
                IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedAPIServerConfiguration = LoadConfiguration(apiServerConfiguration.InitializationInformation.ApplicationConstants.KnownTypes, apiServerConfiguration.InitializationInformation.ApplicationConstants.Environment, apiServerConfiguration.InitializationInformation.ApplicationConstants.ExecutionMode, apiServerConfiguration.InitializationInformation.ApplicationConstants.GetConfigurationFile(), apiServerConfiguration.InitializationInformation.ApplicationConstants.ThrowErrorIfConfigurationDoesNotExistInProduction, apiServerConfiguration.InitializationInformation.InitialApplicationConfiguration, out bool fileWasCreatedNew);
                bool isFirstInitializationRun = fileWasCreatedNew;
                bool initializeLogAsVerbose = apiServerConfiguration.CommandlineParameter.InitialVerboseValue && isFirstInitializationRun;
                GUtilities.AssertCondition(persistedAPIServerConfiguration != null, "Could not load persisted API-server configuration.");
                if (fileWasCreatedNew && apiServerConfiguration.InitializationInformation.ApplicationConstants.AdminHasToEnterInformationAfterInitialConfigurationFileGeneration)
                {
                    throw new InitializationException($"Configuration-file was created. You have to edit certain values there.");
                }
                #endregion

                #region Run APIServer
                APIServer<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> server = new APIServer<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>
                {
                    _Configuration = apiServerConfiguration
                };
                return server.Run(apiServerConfiguration, persistedAPIServerConfiguration, initializeLogAsVerbose);
                #endregion

            }
            catch
            {
                throw;
            }

        }
        public static string GetDefaultBaseFolder<AppConstantsType>(IApplicationConstants<AppConstantsType> applicationConstants, bool isTestRun)
        {
            return GetDefaultBaseFolder(Assembly.GetExecutingAssembly(), applicationConstants.ExecutionMode, applicationConstants.Environment, isTestRun);
        }
        public static string GetDefaultBaseFolder(Assembly executingAssembly, ExecutionMode executionMode, GRYEnvironment environment, bool isTestRun)
        {
            string programFolder = Core.Misc.Utilities.GetValue(Path.GetDirectoryName(executingAssembly.Location));
            string result = executionMode.Accept(new GetBaseFolder(environment, programFolder, isTestRun));
            return result;
        }

        #region Create or load config-file

        private static IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> LoadConfiguration<PersistedAppSpecificConfiguration>(
            ISet<Type> knownTypes, GRYEnvironment evironment, ExecutionMode executionMode, string configurationFile, bool throwErrorIfConfigurationDoesNotExistInProduction,
            PersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> initialConfiguration, out bool fileWasCreatedNew)
                where PersistedAppSpecificConfiguration : new()
        {
            if (throwErrorIfConfigurationDoesNotExistInProduction && evironment is Productive && !File.Exists(configurationFile))
            {
                throw new FileNotFoundException($"Configurationfile \"{configurationFile}\" does not exist.");
            }
            else
            {
                GetPersistedAPIServerConfigurationVisitor<PersistedAppSpecificConfiguration> visitor = new GetPersistedAPIServerConfigurationVisitor<PersistedAppSpecificConfiguration>(configurationFile, initialConfiguration, knownTypes);
                IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> result = executionMode.Accept(visitor);
                fileWasCreatedNew = visitor.FileWasCreatedNew;
                return result;
            }
        }
        private class GetPersistedAPIServerConfigurationVisitor<PersistedAppSpecificConfiguration> : IExecutionModeVisitor<IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration>>
                where PersistedAppSpecificConfiguration : new()
        {
            private readonly MetaConfigurationSettings<PersistedAPIServerConfiguration<PersistedAppSpecificConfiguration>, IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration>> _MetaConfiguration;
            private readonly ISet<Type> _KnownTypes;
            public bool FileWasCreatedNew { get; private set; }

            public GetPersistedAPIServerConfigurationVisitor(string file, PersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> initialValue, ISet<Type> knownTypes)
            {
                this._MetaConfiguration = new MetaConfigurationSettings<PersistedAPIServerConfiguration<PersistedAppSpecificConfiguration>, IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration>>()
                {
                    ConfigurationFormat = XML.Instance,
                    File = file,
                    InitialValue = initialValue
                };
                this._KnownTypes = knownTypes;
            }

            public IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> Handle(Analysis analysis)
            {
                return this._MetaConfiguration.InitialValue;
            }

            public IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> Handle(RunProgram runProgram)
            {
                return this.UsePersistedConfiguration();
            }

            public IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> Handle(TestRun testRun)
            {
                return this.UsePersistedConfiguration();
            }

            private IPersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> UsePersistedConfiguration()
            {
                //TODO add option to define config-file-migrations here
                PersistedAPIServerConfiguration<PersistedAppSpecificConfiguration> result = MetaConfigurationManager.GetConfiguration(this._MetaConfiguration, this._KnownTypes, out bool fileWasCreatedNew);
                this.FileWasCreatedNew = fileWasCreatedNew;
                return result;
            }
        }
        #endregion

        public int Run(APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> config, IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedAPIServerConfiguration, bool initializeLogAsVerbose)
        {
            IGRYLog logger = config.InitializationInformation.InitialLogger;
            this._ApplicationConstants = this._Configuration.InitializationInformation.ApplicationConstants;
            this._MaintenanceModeIsImplementable = true;
            try
            {
                this.CreateRequiredFolder(config.CommandlineParameter.RealRun);
                this.SetTechnicalMaintenanceModeEnabled(true, logger);
                var serverLog = this.GetApplicationLogger(persistedAPIServerConfiguration, initializeLogAsVerbose, config.CommandlineParameter.EnforceVerbose, this._ApplicationConstants.GetLogFolder());
                logger = serverLog.Logger;
                logger.Log($"Start {this._Configuration.InitializationInformation.ApplicationConstants.ApplicationName} (v{this._Configuration.InitializationInformation.ApplicationConstants.ApplicationVersion})", LogLevel.Information);
                logger.Log($"Environment: {this._Configuration.InitializationInformation.ApplicationConstants.Environment}", LogLevel.Debug);
                logger.Log($"Executionmode: {this._Configuration.InitializationInformation.ApplicationConstants.ExecutionMode}", LogLevel.Debug);
                this.EnsureCertificateIsAvailableIfRequired(persistedAPIServerConfiguration);
                WebApplication webApplication = this.CreateWebApplication(config, serverLog, persistedAPIServerConfiguration);
                Action runAction = () =>
                {
                    Task? waitTask = null;
                    this._Configuration.InitializationInformation.InitialLogger.Log($"Run {nameof(FunctionalInformationForWebApplication<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>.PreRun)}...", LogLevel.Debug);
                    this._Configuration.FunctionalInformationForWebApplication.PreRun();
                    try
                    {
                        Task abortListener = Task.Run(() =>
                        {
                            try
                            {
                                bool enabled = true;
                                while (enabled)
                                {
                                    Thread.Sleep(TimeSpan.FromSeconds(1));
                                    if (config.CancellationTokenSource.Token.IsCancellationRequested)
                                    {
                                        enabled = false;
                                        waitTask = webApplication.StopAsync();
                                    }
                                }
                            }
                            catch
                            {
                                throw;
                            }
                        });
                        this.SetTechnicalMaintenanceModeEnabled(false, logger);//TODO do this after webApplication.Run() and only when initializationservice says it is initialized
                        webApplication.Run();
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        GUtilities.AssertNotNull(waitTask, nameof(waitTask)).Wait();
                    }
                    catch (TaskCanceledException)//will be thrown when application will be stopped. This is expected behavior.
                    {
                        GUtilities.NoOperation();
                    }
                    try
                    {
                        logger.Log($"Service will be shutdown", LogLevel.Information);
                        webApplication.WaitForShutdownAsync().Wait();//catch required because this throws "Cannot access a disposed object. Object name: 'IServiceProvider'" for unknown reasons.
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Error while shutdown API-Server", ex, LogLevel.Warning);
                    }
                    this._Configuration.InitializationInformation.InitialLogger.Log($"Run {nameof(FunctionalInformationForWebApplication<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>.PostRun)}...", LogLevel.Debug);
                    this._Configuration.FunctionalInformationForWebApplication.PostRun();
                    logger.Log($"Service finished.", LogLevel.Information);
                };
                if (this._Configuration.FunctionalInformationForWebApplication.RunAsync)
                {
                    Task t = new Task(runAction);
                    t.Start();
                }
                else
                {
                    runAction();
                }
                return 0;
            }
            catch (Exception exception)
            {
                logger.Log("Fatal error occurred.", exception);
                return 1;
            }
        }

        private WebApplication CreateWebApplication(
            APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType> apiServerConfiguration,
            IServerLog serverLog,
            IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedAPIServerConfiguration)
        {
            try
            {
                var logger = serverLog.Logger;
                logger.Log($"BaseFolder: {apiServerConfiguration.InitializationInformation.BaseFolder}", LogLevel.Debug);
                WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    ApplicationName = this._Configuration.InitializationInformation.ApplicationConstants.ApplicationName,
                    EnvironmentName = this._Configuration.InitializationInformation.ApplicationConstants.Environment.GetType().Name
                });
                IServiceCollection services = builder.Services;
                IMvcBuilder mvcBuilder = services.AddControllers(mvcOptions =>
                {
                    mvcOptions.InputFormatters.Add(new ByteArrayInputFormatter());
                    mvcOptions.UseGeneralRoutePrefix(ServerConfiguration.APIRoutePrefix);
                });//TODO add handling for /robots.txt
                mvcBuilder = mvcBuilder.ConfigureApplicationPartManager(manager =>
                    {
                        manager.FeatureProviders.Clear();
                        manager.FeatureProviders.Add(new CustomControllerFeatureProvider<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>(this._Configuration, logger));
                    });
                mvcBuilder.AddApplicationPart(this.GetType().Assembly);
                builder.Services.AddSingleton<IAPIServerCommandlineParameter>((serviceProvider) => apiServerConfiguration.InitializationInformation.CommandlineParameter);
                builder.Services.AddSingleton<CommandlineParameterType>((serviceProvider) => apiServerConfiguration.InitializationInformation.CommandlineParameter);
                builder.Services.AddSingleton<IServerLog>(sp => serverLog);
                builder.Services.AddSingleton((serviceProvider) => persistedAPIServerConfiguration);
                builder.Services.AddSingleton((serviceProvider) => persistedAPIServerConfiguration.ServerConfiguration);
                builder.Services.AddSingleton((serviceProvider) => this._Configuration.InitializationInformation.ApplicationConstants);
                builder.Services.AddSingleton<IApplicationConstants>((serviceProvider) => this._Configuration.InitializationInformation.ApplicationConstants);

                apiServerConfiguration.FunctionalInformation = new FunctionalInformation<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>(
                    apiServerConfiguration.InitializationInformation,
                    builder,
                    persistedAPIServerConfiguration,
                    logger
                );
                apiServerConfiguration.InitializationInformation.InitialLogger.Log($"Run {nameof(APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>.SetFunctionalInformationAction)}...", LogLevel.Debug);
                apiServerConfiguration.SetFunctionalInformationAction(apiServerConfiguration.FunctionalInformation);

                #region Load middlewares
                List<Type> specialMiddlewares1 = [];
                List<Type> specialMiddlewares2 = [];
                List<Type> businessMiddlewares1 = [];
                List<Type> businessMiddlewares2 = [];

                IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedApplicationSpecificConfiguration = apiServerConfiguration.FunctionalInformation.PersistedAPIServerConfiguration;

                #region General Threat-Protection
                if (this._Configuration.InitializationInformation.ApplicationConstants.Environment is not Development)
                {
                    this.AddDefinedMiddleware((ISupportWebApplicationFirewallMiddleware c) => c.ConfigurationForWebApplicationFirewall, this._Configuration.InitializationInformation.ApplicationConstants.WebApplicationFirewallMiddleware, persistedApplicationSpecificConfiguration, specialMiddlewares1, logger);
                    this.AddDefinedMiddleware((ISupportObfuscationMiddleware c) => c.ConfigurationForObfuscationMiddleware, this._Configuration.InitializationInformation.ApplicationConstants.ObfuscationMiddleware, persistedApplicationSpecificConfiguration, specialMiddlewares1, logger);
                    this.AddDefinedMiddleware((ISupportCaptchaMiddleware c) => c.ConfigurationForCaptchaMiddleware, this._Configuration.InitializationInformation.ApplicationConstants.CaptchaMiddleware, persistedApplicationSpecificConfiguration, specialMiddlewares1, logger);
                }
                #endregion

                specialMiddlewares1.Add(typeof(GeneralMiddleware<PersistedApplicationSpecificConfiguration>));

                this.AddDefinedMiddleware((ISupportRequestLoggingMiddleware c) => c.ConfigurationForLoggingMiddleware, this._Configuration.InitializationInformation.ApplicationConstants.LoggingMiddleware, persistedApplicationSpecificConfiguration, specialMiddlewares1, logger);//outer (must be out of exception-handling-middleware, otherwise 4xx and 5xx can not be logged.)

                this.AddDefinedMiddleware((ISupportExceptionManagerMiddleware c) => c.ConfigurationForExceptionManagerMiddleware, this._Configuration.InitializationInformation.ApplicationConstants.ExceptionManagerMiddleware, persistedApplicationSpecificConfiguration, specialMiddlewares1, logger);//inner (must be inside of log-middleware)


                foreach (Type customMiddleware in this._Configuration.InitializationInformation.ApplicationConstants.CustomMiddlewares1)
                {
                    logger.Log($"Added custom middleware {customMiddleware.FullName}.", LogLevel.Information);
                    businessMiddlewares1.Add(customMiddleware);
                }

                #region Bussiness-implementation
                this.AddDefinedMiddleware((ISupportMaintenanceSiteMiddleware c) => c.ConfigurationForMaintenanceSiteMiddleware, this._Configuration.InitializationInformation.ApplicationConstants.MaintenanceSiteMiddleware, persistedApplicationSpecificConfiguration, specialMiddlewares2, logger);
                this.AddDefinedMiddleware((ISupportAuthenticationMiddleware c) => c.ConfigurationForAuthenticationMiddleware, this._Configuration.InitializationInformation.ApplicationConstants.AuthenticationMiddleware, persistedApplicationSpecificConfiguration, specialMiddlewares2, logger);
                this.AddDefinedMiddleware((ISupportAuthorizationMiddleware c) => c.ConfigurationForAuthorizationMiddleware, this._Configuration.InitializationInformation.ApplicationConstants.AuthorizationMiddleware, persistedApplicationSpecificConfiguration, specialMiddlewares2, logger);

                foreach (Type customMiddleware in this._Configuration.InitializationInformation.ApplicationConstants.CustomMiddlewares2)
                {
                    logger.Log($"Added custom middleware {customMiddleware.FullName}.", LogLevel.Information);
                    businessMiddlewares2.Add(customMiddleware);
                }
                #endregion

                #endregion

                builder.WebHost.ConfigureKestrel(kestrelOptions =>
                {
                    kestrelOptions.AllowSynchronousIO = true;
                    kestrelOptions.AddServerHeader = false;
                    Action<ListenOptions> lOptions = listenOptions =>
                    {
                        if (persistedApplicationSpecificConfiguration.ServerConfiguration.Protocol is HTTPS https)
                        {
                            string pfxFilePath = https.TLSCertificateInformation.CertificatePFXFile.GetPath(this._Configuration.InitializationInformation.ApplicationConstants.GetCertificateFolder());
                            string passwordFilePath = https.TLSCertificateInformation.CertificatePasswordFile.GetPath(this._Configuration.InitializationInformation.ApplicationConstants.GetCertificateFolder());
                            string password = File.ReadAllText(passwordFilePath, new UTF8Encoding(false));
                            X509Certificate2 certificate = new X509Certificate2(pfxFilePath, password);
                            if (this._Configuration.InitializationInformation.ApplicationConstants.Environment is Productive && GUtilities.IsSelfSIgned(certificate))
                            {
                                logger.Log($"The used certificate '{pfxFilePath}' is self-signed. Using self-signed certificates is not recommended in a productive environment.", LogLevel.Warning);
                            }
                            listenOptions.UseHttps(certificate);
                            string dnsName = certificate.GetNameInfo(X509NameType.DnsName, false);
                            if (this._Configuration.InitializationInformation.ApplicationConstants.Environment is not Development && dnsName != persistedApplicationSpecificConfiguration.ServerConfiguration.Domain)
                            {
                                logger.Log($"The used certificate has the DNS-name '{dnsName}' which differs from the domain '{persistedApplicationSpecificConfiguration.ServerConfiguration.Domain}' which is set in the configuration.", LogLevel.Warning);
                            }
                        }
                    };
                    if (apiServerConfiguration.InitializationInformation.ApplicationConstants.ListenOnEveryIP)
                    {
                        kestrelOptions.ListenAnyIP(persistedApplicationSpecificConfiguration.ServerConfiguration.Protocol.Port, lOptions);
                    }
                    else
                    {
                        kestrelOptions.ListenLocalhost(persistedApplicationSpecificConfiguration.ServerConfiguration.Protocol.Port, lOptions);
                    }
                });
                string appVersionString = $"v{this._Configuration.InitializationInformation.ApplicationConstants.ApplicationVersion}";

                bool hostAPIDocumentation = HostAPIDocumentation(this._Configuration.InitializationInformation.ApplicationConstants.Environment, persistedApplicationSpecificConfiguration.ServerConfiguration.HostAPISpecificationForInNonDevelopmentEnvironment, this._Configuration.InitializationInformation.ApplicationConstants.ExecutionMode);
                string apiUITitle = $"{this._Configuration.InitializationInformation.ApplicationConstants.ApplicationName} v{this._Configuration.InitializationInformation.ApplicationConstants.ApplicationVersion} API documentation";
                if (hostAPIDocumentation)
                {
                    builder.Services.AddEndpointsApiExplorer();

                    builder.Services.AddSwaggerGen(swaggerOptions =>
                    {
                        foreach (FilterDescriptor filter in this._Configuration.FunctionalInformation.Filter)
                        {
                            swaggerOptions.OperationFilterDescriptors.Add(filter);
                        }
                        OpenApiInfo openAPIInfo = new OpenApiInfo
                        {
                            Version = appVersionString,
                            Title = apiUITitle,
                            Description = this._Configuration.InitializationInformation.ApplicationConstants.ApplicationDescription,
                        };
                        if (this._Configuration.InitializationInformation.ApplicationConstants.CommonRoutesHostInformation is HostCommonRoutes)
                        {
                            openAPIInfo.TermsOfService = new Uri(persistedApplicationSpecificConfiguration.ServerConfiguration.GetServerAddress() + ServerConfiguration.TermsOfServiceURLSubPath);
                            openAPIInfo.Contact = new OpenApiContact
                            {
                                Name = "Contact",
                                Url = new Uri(persistedApplicationSpecificConfiguration.ServerConfiguration.GetServerAddress() + ServerConfiguration.ContactURLSubPath)
                            };
                            openAPIInfo.License = new OpenApiLicense
                            {
                                Name = "License",
                                Url = new Uri(persistedApplicationSpecificConfiguration.ServerConfiguration.GetServerAddress() + ServerConfiguration.LicenseURLSubPath)
                            };
                        }
                        swaggerOptions.SwaggerDoc(ServerConfiguration.APISpecificationDocumentName, openAPIInfo);
                        string xmlFilename = $"{this._Configuration.InitializationInformation.ApplicationConstants.ApplicationName}.xml";
                        swaggerOptions.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                        //TODO add support for swaggerOptions.MapType<SomeType>(() => ...);
                    });
                }

                builder.Services.AddLogging(c => c.ClearProviders());

                WebApplication app = builder.Build();
                if (this._Configuration.InitializationInformation.ApplicationConstants.UseWebSockets)
                {
                    app.UseWebSockets();
                }
                app.UseRouting();

                #region Add middlewares
                foreach (Type middleware in specialMiddlewares1)
                {
                    app.UseMiddleware(middleware);
                }
                foreach (Type middleware in businessMiddlewares1)
                {
                    app.UseMiddleware(middleware);
                }
                foreach (Type middleware in specialMiddlewares2)
                {
                    app.UseMiddleware(middleware);
                }
                if (persistedApplicationSpecificConfiguration.ServerConfiguration.Protocol is HTTPS)
                {
                    app.UseHsts();
                }
                foreach (Type middleware in businessMiddlewares2)
                {
                    app.UseMiddleware(middleware);
                }
                #endregion

                #region API Documentation
                string apiLink = persistedApplicationSpecificConfiguration.ServerConfiguration.GetServerAddress() + ServerConfiguration.APIRoutePrefix;
                if (hostAPIDocumentation)
                {
                    string openAPISpecificationRoute = $"/{ServerConfiguration.ResourcesSubPath}/{ServerConfiguration.APISpecificationDocumentName}";
                    string apiDocumentationSubRoute = $"{ServerConfiguration.ResourcesSubPath}/{ServerConfiguration.APISpecificationDocumentName}";
                    string entireAPIDocumentationRoute = $"{ServerConfiguration.APIRoutePrefix[1..]}/{apiDocumentationSubRoute}";

                    app.UseSwagger(options => options.RouteTemplate = $"{entireAPIDocumentationRoute}/{{documentName}}/{this._Configuration.InitializationInformation.ApplicationConstants.ApplicationName}.api.json");
                    app.UseSwaggerUI(options =>
                    {
                        string appVersionString = $"v{this._Configuration.InitializationInformation.ApplicationConstants.ApplicationVersion}";
                        string ui = $"{ServerConfiguration.APISpecificationDocumentName}/{this._Configuration.InitializationInformation.ApplicationConstants.ApplicationName}.api.json";
                        options.SwaggerEndpoint(ui, this._Configuration.InitializationInformation.ApplicationConstants.ApplicationName + " " + appVersionString);
                        options.RoutePrefix = entireAPIDocumentationRoute;
                        options.DocumentTitle = apiUITitle;
                        apiLink = $"{apiLink}/{apiDocumentationSubRoute}/index.html";
                    });
                }
                #endregion

                app.UseEndpoints(endpoints => endpoints.MapControllers());
                apiServerConfiguration.FunctionalInformationForWebApplication = new FunctionalInformationForWebApplication<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>(
                    apiServerConfiguration.InitializationInformation,
                    builder.Services,
                    persistedAPIServerConfiguration,
                    app
                );
                logger.Log($"Log-level of {logger.Configuration.Name}-log:", LogLevel.Information);
                foreach (GRYLogTarget target in logger.Configuration.LogTargets)
                {
                    string enabled = target.Enabled ? "enabled" : "disabled";
                    logger.Log($"- {target.GetType().Name} ({enabled}): " + string.Join(", ", target.LogLevels.Select(l => l.ToString())));
                }
                logger.Log($"Run {nameof(APIServerConfiguration<ApplicationSpecificConstants, PersistedApplicationSpecificConfiguration, CommandlineParameterType>.ConfigureWebApplication)}...", LogLevel.Debug);
                apiServerConfiguration.ConfigureWebApplication(apiServerConfiguration.FunctionalInformationForWebApplication);
                logger.Log($"The API will now be available under the following URL:", LogLevel.Information);
                logger.Log(apiLink, LogLevel.Information);
                if (this.GetManualMaintenanceModeEnabled())
                {
                    logger.Log($"Maintenancemode is enabled.", LogLevel.Information);
                }
                return app;
            }
            catch
            {
                throw;
            }
        }

        private void AddDefinedMiddleware<SupportDefinedMiddlewareType>(
            Func<SupportDefinedMiddlewareType, IMiddlewareConfiguration> getMiddlewareConfiguration,
            Type middlewareType,
            IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedApplicationSpecificConfiguration,
            List<Type> middlewares,
            IGRYLog logger
        ) where SupportDefinedMiddlewareType : ISupportedMiddleware
        {
            if (persistedApplicationSpecificConfiguration.ApplicationSpecificConfiguration is SupportDefinedMiddlewareType supportDefinedMiddlewareType)
            {
                IMiddlewareConfiguration middlewareConfiguration = getMiddlewareConfiguration(supportDefinedMiddlewareType);
                if (middlewareConfiguration == null)
                {
                    throw new NullReferenceException($"No middleware-configuration given for {typeof(SupportDefinedMiddlewareType).FullName}.");
                }
                else
                {
                    if (middlewareConfiguration.Enabled)
                    {
                        this._Configuration.FunctionalInformation.Filter.UnionWith(middlewareConfiguration.GetFilter());
                        if (middlewareType == null)
                        {
                            throw new NullReferenceException($"No middleware-type given for {typeof(SupportDefinedMiddlewareType).FullName}.");
                        }
                        else
                        {
                            if (middlewareType.IsAbstract || middlewareType.IsInterface)
                            {
                                throw new ArgumentException($"The type {middlewareType.FullName} can not be used as middleware because the type is not a nonabstract class.");
                            }
                            else
                            {
                                middlewares.Add(middlewareType);
                                logger.Log($"Added middleware {middlewareType.FullName}.", LogLevel.Information);
                                if (middlewareType.IsAssignableTo(typeof(MaintenanceSiteMiddleware)))
                                {
                                    IMaintenanceSiteConfiguration maintenanceSiteConfiguration = (IMaintenanceSiteConfiguration)middlewareConfiguration;
                                    if (maintenanceSiteConfiguration.MaintenanceModeEnabled)
                                    {
                                        this.SetManualMaintenanceModeEnabled(true, logger);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Log($"Middleware {middlewareType.FullName} is disabled.", LogLevel.Information);
                    }
                }
            }
        }

        #region Host API Documentation
        private static bool HostAPIDocumentation(GRYEnvironment environment, bool hostAPISpecificationForInNonDevelopmentEnvironment, ExecutionMode executionMode)
        {
            return executionMode.Accept(new GetHostAPIDocumentationVisitor(environment, hostAPISpecificationForInNonDevelopmentEnvironment));
        }

        private class GetHostAPIDocumentationVisitor : IExecutionModeVisitor<bool>
        {
            private readonly GRYEnvironment _Environment;
            private readonly bool _HostAPISpecificationForInNonDevelopmentEnvironment;

            public GetHostAPIDocumentationVisitor(GRYEnvironment environment, bool hostAPISpecificationForInNonDevelopmentEnvironment)
            {
                this._Environment = environment;
                this._HostAPISpecificationForInNonDevelopmentEnvironment = hostAPISpecificationForInNonDevelopmentEnvironment;
            }

            public bool Handle(Analysis analysis)
            {
                return true;// required for generation of OpenAPI-specification-json-file
            }

            public bool Handle(RunProgram runProgram)
            {
                if (this._Environment is Development)
                {
                    return true;
                }
                else
                {
                    return this._HostAPISpecificationForInNonDevelopmentEnvironment;
                }
            }

            public bool Handle(TestRun testRun)
            {
                return true;
            }
        }
        #endregion

        private void EnsureCertificateIsAvailableIfRequired(IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedApplicationSpecificConfiguration)
        {
            string certFolder = this._Configuration.InitializationInformation.ApplicationConstants.GetCertificateFolder();
            if (persistedApplicationSpecificConfiguration.ServerConfiguration.Protocol is HTTPS https)
            {
                string pfxFile = https.TLSCertificateInformation.CertificatePFXFile.GetPath(certFolder);
                string passwordFile = https.TLSCertificateInformation.CertificatePasswordFile.GetPath(certFolder);
                if (!File.Exists(pfxFile) && !File.Exists(passwordFile))
                {
                    GUtilities.EnsureFileExists(pfxFile);
                    File.WriteAllBytes(pfxFile, GUtilities.HexStringToByteArray(persistedApplicationSpecificConfiguration.ServerConfiguration.DevelopmentCertificatePFXHex));
                    GUtilities.EnsureFileExists(passwordFile);
                    File.WriteAllBytes(passwordFile, GUtilities.HexStringToByteArray(persistedApplicationSpecificConfiguration.ServerConfiguration.DevelopmentCertificatePasswordHex));
                }
                if (!File.Exists(pfxFile))
                {
                    throw new FileNotFoundException($"\"{pfxFile}\" does not exist.");
                }
                if (!File.Exists(passwordFile))
                {
                    throw new FileNotFoundException($"\"{passwordFile}\" does not exist.");
                }
            }
        }

        private IServerLog GetApplicationLogger(IPersistedAPIServerConfiguration<PersistedApplicationSpecificConfiguration> persistedApplicationSpecificConfiguration, bool initialVerboseValue, bool enforceVerbose, string logFolder)
        {
            ServerLog result = new ServerLog(persistedApplicationSpecificConfiguration.ApplicationLogConfiguration, logFolder);
            if (initialVerboseValue || enforceVerbose)
            {
                result.Logger.Configuration.AddLogLevel(LogLevel.Debug);
            }
            return result;
        }

        private void CreateRequiredFolder(bool isRealRun)
        {
            if (!isRealRun)
            {
                GUtilities.EnsureDirectoryDoesNotExist(this._Configuration.InitializationInformation.ApplicationConstants.GetConfigurationFolder());
            }
            GUtilities.EnsureDirectoryExists(this._Configuration.InitializationInformation.ApplicationConstants.GetConfigurationFolder());
            GUtilities.EnsureDirectoryExists(this._Configuration.InitializationInformation.ApplicationConstants.GetLogFolder());
            GUtilities.EnsureDirectoryExists(this._Configuration.InitializationInformation.ApplicationConstants.GetCertificateFolder());
        }
    }
}
