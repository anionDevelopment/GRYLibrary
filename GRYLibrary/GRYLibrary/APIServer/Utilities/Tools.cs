using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Services;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.APIServer.Verbs;
using GRYLibrary.Core.Crypto;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.ConsoleApplication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using HashAlgorithm = GRYLibrary.Core.Crypto.HashAlgorithm;
using SHA256 = GRYLibrary.Core.Crypto.SHA256;

namespace GRYLibrary.Core.APIServer.Utilities
{
    public static class Tools
    {
        public static (byte[] requestBody, byte[] responseBody) ExecuteNextMiddlewareAndGetRequestAndResponseBody(HttpContext context, RequestDelegate next, Func<byte[], byte[]> requestBodyUpdater = null, Func<byte[], byte[]> responseBodyUpdater = null)
        {
            byte[] requestBody = GetRequestBody(context.Request, requestBodyUpdater);
            byte[] responseBody = default;
            Stream originalBody = context.Response.Body;

            using (MemoryStream memStream = new MemoryStream())
            {
                context.Response.Body = memStream;
                try
                {
                    next(context).Wait();
                }
                catch
                {
                    throw;
                }

                memStream.Position = 0;
                responseBody = GUtilities.StreamToByteArray(memStream);
                string temp1 = new UTF8Encoding(false).GetString(responseBody);
                if (responseBodyUpdater != null)
                {
                    responseBody = responseBodyUpdater(responseBody);
                }
                string temp2 = new UTF8Encoding(false).GetString(responseBody);
                MemoryStream memStream2 = new MemoryStream(responseBody);
                memStream2.CopyToAsync(originalBody).Wait();
            }

            context.Response.Body = originalBody;
            return (requestBody, responseBody);
        }

        public static byte[] GetRequestBody(HttpRequest request, Func<byte[], byte[]> requestBodyUpdater = null)
        {
            request.EnableBuffering();
            byte[] result = GUtilities.StreamToByteArray(request.Body);
            if (requestBodyUpdater != null)
            {
                result = requestBodyUpdater(result);
            }
            request.Body = new MemoryStream(result);
            return result;
        }

        public static string GetDefaultDomainValue(string codeUnitName)
        {
            return $"{codeUnitName.ToLower()}.test.local";
        }

        public static bool IsAPIDocumentationRequest(HttpContext context)
        {
            return context.Request.Path.ToString().StartsWith($"{ServerConfiguration.APIRoutePrefix}/{ServerConfiguration.ResourcesSubPath}/{ServerConfiguration.APISpecificationDocumentName}/");
        }

        public static bool IsMaintenanceRequest(HttpContext context)
        {
            return context.Request.Path.ToString().StartsWith($"{ServerConfiguration.APIRoutePrefix}/Other/Maintenance/");
        }

        public static int RunAPIServer<GCodeUnitSpecificCommandlineParameter, GCodeUnitSpecificConstants, GCodeUnitSpecificConfiguration>(string codeUnitName, string codeUnitDescription, Version3 codeUnitVersion, GRYEnvironment environmentTargetType, ExecutionMode executionMode, string[] commandlineArguments, string? additionalHelpText, Action<APIServerConfiguration<GCodeUnitSpecificConstants, GCodeUnitSpecificConfiguration, GCodeUnitSpecificCommandlineParameter>> initializer, IGRYLog? log)
            where GCodeUnitSpecificConfiguration : new()
            where GCodeUnitSpecificConstants : new()
            where GCodeUnitSpecificCommandlineParameter : class, IAPIServerCommandlineParameter, new()
        {
            int exitCode = 0;
            Exception? exception = null;//for debugging-purposes
            try
            {
                if (Debugger.IsAttached)
                {
                    try
                    {
                        Console.Clear();
                    }
                    catch
                    {
                        GRYLibrary.Core.Misc.Utilities.NoOperation();
                    }
                }
                log = log ?? GeneralLogger.CreateUsingConsole();
                GRYConsoleApplication<GCodeUnitSpecificCommandlineParameter> consoleApp = new GRYConsoleApplication<GCodeUnitSpecificCommandlineParameter>(new VerbParser<GCodeUnitSpecificCommandlineParameter>(APIServer<GCodeUnitSpecificConstants, GCodeUnitSpecificConfiguration, GCodeUnitSpecificCommandlineParameter>.CreateMain(initializer, log)), codeUnitName, codeUnitVersion.ToString(), codeUnitDescription, true, executionMode, environmentTargetType, additionalHelpText, log);
                exitCode = consoleApp.Main(commandlineArguments);
            }
            catch (Exception ex)
            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                exception = ex;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                exitCode = 1;
            }
            return exitCode;
        }

        public static void WaitUntilDatabaseIsAvailable(IReconnectableDatabase reconnectableDatabase, IGeneralLogger logger, uint maximalAmountOfAttempts = 24, uint initialAmountOfSecondsToWait = 10)
        {
            reconnectableDatabase.SetLogConnectionAttemptErrors(false);
            (bool, Exception?) isAvailableResult;
            try
            {
                isAvailableResult = reconnectableDatabase.IsAvailable();//check if the database is already available
            }
            catch (Exception e)
            {
                isAvailableResult = (false, e);
            }
            uint amoutnOfFails = 0;
            if (!isAvailableResult.Item1)
            {
                logger.Log("Wait until database is available...", LogLevel.Information);
                Thread.Sleep(TimeSpan.FromSeconds(initialAmountOfSecondsToWait));
                reconnectableDatabase.SetLogConnectionAttemptErrors(true);
                while (!isAvailableResult.Item1)
                {
                    try
                    {
                        isAvailableResult = reconnectableDatabase.IsAvailable();
                    }
                    catch (Exception ex)
                    {
                        isAvailableResult = (false, ex);
                    }
                    if (!isAvailableResult.Item1)
                    {
                        logger.Log($"Database is not available yet because of the reason \"{isAvailableResult.Item2!.Message}\".", LogLevel.Information);
                        amoutnOfFails = amoutnOfFails + 1;
                        if (amoutnOfFails == maximalAmountOfAttempts)
                        {
                            throw new DependencyNotAvailableException("Database is not available.", isAvailableResult.Item2!);
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }

                }
            }
            logger.Log("Connected. Database is now available.", LogLevel.Information);
        }

        public static bool TryGetAuthentication(ICredentialsProvider credentialsProvider, HttpContext context, out string accessToken)
        {
            try
            {
                if (credentialsProvider.ContainsCredentials(context))
                {
                    string secret = credentialsProvider.ExtractSecret(context);
                    if (!string.IsNullOrEmpty(secret))
                    {
                        accessToken = secret;
                        return true;
                    }
                }
            }
            catch
            {
                GUtilities.NoOperation();
            }
            accessToken = null;
            return false;
        }
        private readonly static IList<HashAlgorithm> _HashAlgorithms = [new SHA256(), new SHA256PureCSharp()];
        public static HashAlgorithm GetHashAlgorithm(string passwordHashAlgorithmIdentifier)
        {
            byte[] passwordHashAlgorithmIdentifierBytes = GUtilities.PadLeft(new UTF8Encoding(false).GetBytes(passwordHashAlgorithmIdentifier), 10);
            foreach (HashAlgorithm algorithm in _HashAlgorithms)
            {
                if (algorithm.GetIdentifier() == passwordHashAlgorithmIdentifierBytes)
                {
                    return algorithm;
                }
            }
            throw new KeyNotFoundException($"Unknown algorithm: {passwordHashAlgorithmIdentifier}");
        }
        public static void CheckSingleExternalService(IGeneralLogger logger, string name, IExternalService service, ref HealthStatus result, IList<string> messages, bool logIfNotAvailable, bool serviceIsRequired)
        {
            CheckSingleExternalService(logger, name, service.IsAvailable, ref result, messages, logIfNotAvailable, serviceIsRequired);
        }

        public static void CheckSingleExternalService(IGeneralLogger logger, string name, Func<(bool, Exception?)> isAvailable, ref HealthStatus result, IList<string> messages, bool logIfNotAvailable, bool serviceIsRequired)
        {
            (bool available, Exception? error) = isAvailable();
            if (!available)
            {
                string message = $"Service {name} is not available. Reason: " + error!.Message;
                messages.Add(message);
                if (logIfNotAvailable)
                {
                    logger.Log(message, LogLevel.Warning);
                }
                result = CalculateFinalHealthStatus(result, serviceIsRequired);
                return;
            }
            //more checks can be added
        }

        private static HealthStatus CalculateFinalHealthStatus(HealthStatus resultUntilNow, bool dependencyIsRequired)
        {
            if (resultUntilNow == HealthStatus.Unhealthy)
            {
                return resultUntilNow;
            }
            else
            {
                if (dependencyIsRequired)
                {
                    return HealthStatus.Unhealthy;
                }
                else
                {
                    return HealthStatus.Degraded;
                }
            }
        }
#pragma warning disable IDE0060 // Remove unused parameter
        public static Task<HealthCheckResult> CheckHealthAsync(IGeneralLogger logger, Func<(HealthStatus, IEnumerable<string>)> check, HealthCheckContext context, CancellationToken cancellationToken, IInitializationService? initializationService)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return Task.FromResult(GetHealthCheckResult(logger, check, initializationService));//TODO pass cancellationToken
        }

        private static HealthCheckResult GetHealthCheckResult(IGeneralLogger logger, Func<(HealthStatus, IEnumerable<string>)> check, IInitializationService? initializationService)
        {
            if (initializationService == null)
            {
                return CalculateHealthState(logger, check);//TODO pass cancellationToken
            }
            else
            {
                return initializationService.GetInitializationState().Accept(new InitializationStateVisitor(logger, check));//TODO pass cancellationToken
            }
        }

        public static User GetUser(ClaimsPrincipal principal, IAuthenticationService authenticationService)
        {
            User result = authenticationService.GetUser(principal.Claims.Where(claim => claim.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").First().Value);
            return result;
        }
        private class InitializationStateVisitor : IInitializationStateVisitor<HealthCheckResult>
        {
            private readonly IGeneralLogger _Logger;
            private readonly Func<(HealthStatus, IEnumerable<string>)> _Check;
            public InitializationStateVisitor(IGeneralLogger logger, Func<(HealthStatus, IEnumerable<string>)> check)
            {
                this._Logger = logger;
                this._Check = check;
            }
            public HealthCheckResult Handle(InitializationFailed initializationFailed)
            {
                return HealthCheckResult.Unhealthy("Initialization failed");
            }

            public HealthCheckResult Handle(Uninitialized uninitialized)
            {
                return HealthCheckResult.Degraded("Not initialized yet");
            }

            public HealthCheckResult Handle(Initializing initializing)
            {
                return HealthCheckResult.Healthy("Initializing");
            }

            public HealthCheckResult Handle(Initialized initialized)
            {
                return CalculateHealthState(this._Logger, this._Check);//TODO pass cancellationToken
            }

        }
        private static HealthCheckResult CalculateHealthState(IGeneralLogger logger, Func<(HealthStatus, IEnumerable<string>)> check)
        {
            //TODO consider cancellationToken
            (HealthStatus result, IEnumerable<string> messages) result = check();

            LogLevel loglevel;
            string message;
            if (result.result.Equals(HealthStatus.Healthy))
            {
                message = "Service is healthy.";
                loglevel = LogLevel.Debug;
            }
            else if (result.result.Equals(HealthStatus.Degraded))
            {
                message = "Service is degraded.";
                loglevel = LogLevel.Warning;
            }
            else if (result.result.Equals(HealthStatus.Unhealthy))
            {
                message = "Service is unhealthy.";
                loglevel = LogLevel.Error;
            }
            else
            {
                throw new InternalAlgorithmException($"Unknown healthstatus: {(int)result.result}");
            }

            if (0 < result.messages.Count())
            {
                message = $"{message} The following messages occurred: " + string.Join(", ", result.messages.Select(message => "\"" + message + "\""));
            }
            logger.Log(message, loglevel);
            HealthCheckResult healthCheckResult;
            if (result.result.Equals(HealthStatus.Healthy))
            {
                healthCheckResult = HealthCheckResult.Healthy(message);
            }
            else if (result.result.Equals(HealthStatus.Degraded))
            {
                healthCheckResult = HealthCheckResult.Degraded(message);
            }
            else if (result.result.Equals(HealthStatus.Unhealthy))
            {
                healthCheckResult = HealthCheckResult.Unhealthy(message);
            }
            else
            {
                throw new InternalAlgorithmException($"Undknown healthstatus: {(int)result.result}");
            }
            return healthCheckResult;
        }

        public static TimeSpan GetTimeout(TimeSpan? debugTimeout)
        {
            if (debugTimeout == null)
            {
                debugTimeout = TimeSpan.FromHours(1);
            }
            if (Debugger.IsAttached)
            {
                return debugTimeout.Value;
            }
            else
            {
                return TimeSpan.FromMinutes(2);
            }
        }
    }
}