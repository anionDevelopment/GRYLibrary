using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.MidT.RLog;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Verbs;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer.Mid.M05DLog
{
    /// <summary>
    /// Represents a middleware which logs the requests.
    /// </summary>
    public class DRequestLoggingMiddleware : RequestLoggingMiddleware
    {
        private readonly IGRYLog _RequestLogger;
        private readonly IGRYLog _Logger;
        private readonly IDRequestLoggingConfiguration _RequestLoggingSettings;
        private readonly IApplicationConstants _AppConstants;
        private readonly Encoding _Encoding = new UTF8Encoding(false);
        private readonly Counter _RequestCounterSum;
        private readonly Counter _RequestCounter2xx;
        private readonly Counter _RequestCounter3xx;
        private readonly Counter _RequestCounter4xx;
        private readonly Counter _RequestCounter5xx;
        private readonly Counter _RequestCounterOther;
        private readonly IServerConfiguration _ServerConfiguration;
        private readonly IAPIServerCommandlineParameter _CommandlineParameter;
        /// <inheritdoc/>
        public DRequestLoggingMiddleware(RequestDelegate next, IDRequestLoggingConfiguration requestLoggingSettings, IApplicationConstants appConstants, IServerLog logger, ITimeService timeService, IServerConfiguration serverConfiguration, IAPIServerCommandlineParameter commandlineParameter) : base(next, timeService)
        {
            this._RequestLoggingSettings = requestLoggingSettings;
            this._CommandlineParameter = commandlineParameter;
            this._AppConstants = appConstants;
            this._Logger = logger.Logger;
            this._ServerConfiguration = serverConfiguration;
            if (this._CommandlineParameter.RealRun)//for run-mode server-logs and request-logs should be separated, for test- and analysis-mode it should be visible in one single log
            {
                if (this._CommandlineParameter.EnforceVerbose)
                {
                    this._RequestLoggingSettings.RequestsLogConfiguration.AddLogLevel(LogLevel.Debug);
                }
                this._RequestLogger = new RequestsLog(this._RequestLoggingSettings.RequestsLogConfiguration, appConstants.GetLogFolder()).Logger;
            }
            else
            {
                this._RequestLogger = logger.Logger;
            }
            CounterConfiguration counterMetricConfig = new CounterConfiguration()
            {
                LabelNames = ["domain"],
            };
            this._RequestCounterSum = Metrics.CreateCounter("http_requests_sum", "Sum of all HTTP-requests", counterMetricConfig);
            this._RequestCounterSum.IncTo(0);
            this._RequestCounter2xx = Metrics.CreateCounter("http_requests_2xx", "Sum of all HTTP-requests with 2xx-response-statuscode", counterMetricConfig);
            this._RequestCounter2xx.IncTo(0);
            this._RequestCounter3xx = Metrics.CreateCounter("http_requests_3xx", "Sum of all HTTP-requests with 3xx-response-statuscode", counterMetricConfig);
            this._RequestCounter3xx.IncTo(0);
            this._RequestCounter4xx = Metrics.CreateCounter("http_requests_4xx", "Sum of all HTTP-requests with 4xx-response-statuscode", counterMetricConfig);
            this._RequestCounter4xx.IncTo(0);
            this._RequestCounter5xx = Metrics.CreateCounter("http_requests_5xx", "Sum of all HTTP-requests with 5xx-response-statuscode", counterMetricConfig);
            this._RequestCounter5xx.IncTo(0);
            this._RequestCounterOther = Metrics.CreateCounter("http_requests_other", "Sum of all HTTP-requests with other response-statuscode", counterMetricConfig);
            this._RequestCounterOther.IncTo(0);
        }
        /// <inheritdoc/>

        protected override void Log(HttpContext context, byte[] requestBodyBytes, byte[] responseBodyBytes)
        {
            try
            {
                DateTimeOffset moment = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset();
                (string info, string? content, byte[] plainContent) requestBody = BytesToString(requestBodyBytes, this._Encoding);
                (string info, string? content, byte[] plainContent) responseBody = BytesToString(responseBodyBytes, this._Encoding);
                string requestRoute = context.Request.Path;
                ushort responseHTTPStatusCode = (ushort)context.Response.StatusCode;
                IPAddress? clientIP = (IPAddress?)context.Items["ClientIPAddress"];
                string requestId = (string)context.Items["RequestId"];
                Request request = new Request(requestId, moment, clientIP, context.Request.Method, requestRoute, context.Request.Query, context.Request.Headers, requestBody, null/*TODO*/, responseHTTPStatusCode, context.Response.Headers, responseBody);
                TimeSpan? duration = context.Items.ContainsKey("Duration") ? (TimeSpan)context.Items["Duration"] : default;
                bool isAuthenticated;
                if (context.Items.ContainsKey(AuthenticationMiddleware.IsAuthenticatedInformationName))
                {
                    isAuthenticated = (bool)context.Items[AuthenticationMiddleware.IsAuthenticatedInformationName];
                }
                else
                {
                    isAuthenticated = false;
                }
                ClaimsPrincipal principal = isAuthenticated && context.User != null && context.User.Identity.IsAuthenticated ? context.User : null;
                if (this.ShouldBeLogged(request))
                {
                    //TODO add option to add this log-entry to a database
                    this.AddDataToMetrics(request);
                    IDictionary<string, IList<string?>> header = this.GetHeader(context.Request);
                    this.LogHTTPRequest(request, false, duration, principal, new HashSet<GRYLogTarget> { new Logging.GRYLogger.ConcreteLogTargets.Console() }, header);
                    this.LogHTTPRequest(request, this.ShouldLogEntireRequestContentInLogFile(request), duration, principal, new HashSet<GRYLogTarget> { new Logging.GRYLogger.ConcreteLogTargets.LogFile() }, header);
                }
            }
            catch
            {
                throw;
            }
        }

        private void AddDataToMetrics(Request request)
        {
            this._RequestCounterSum.WithLabels(this.GetDomain()).Inc();
            string domain = this.GetDomain();
            if (200 <= request.ResponseStatusCode && request.ResponseStatusCode < 300)
            {
                this._RequestCounter2xx.WithLabels(domain).Inc();
            }
            else if (300 <= request.ResponseStatusCode && request.ResponseStatusCode < 400)
            {
                this._RequestCounter3xx.WithLabels(domain).Inc();
            }
            else if (400 <= request.ResponseStatusCode && request.ResponseStatusCode < 500)
            {
                this._RequestCounter4xx.WithLabels(domain).Inc();
            }
            else if (500 <= request.ResponseStatusCode && request.ResponseStatusCode < 600)
            {
                this._RequestCounter5xx.WithLabels(domain).Inc();
            }
            else
            {
                this._RequestCounterOther.WithLabels(domain).Inc();
            }
        }

        private string GetDomain()
        {
            return this._ServerConfiguration.Domain;
        }

        private IDictionary<string, IList<string?>> GetHeader(HttpRequest request)
        {
            Dictionary<string, IList<string?>> result = [];
            foreach (string headerToLog in this._RequestLoggingSettings.LoggedHTTPRequeustHeader)
            {
                if (!result.TryGetValue(headerToLog, out IList<string?>? value))
                {
                    value = [];
                    result.Add(headerToLog, value);
                }
                if (request.Headers.TryGetValue(headerToLog, out Microsoft.Extensions.Primitives.StringValues values))
                {
                    foreach (string? item in values)
                    {
                        value.Add(item);
                    }
                }
            }
            return result;
        }

        public static (string info, string? content, byte[] plainContent) BytesToString(byte[] content, Encoding encoding)
        {
            if (content.Length == 0)
            {
                return ("Empty", null, content);
            }
            try
            {
                return ("UTF8-encoded-content", encoding.GetString(content), content);
            }
            catch
            {
                return ("Hex-encoded-content", GUtilities.ByteArrayToHexString(content), content);
            }
        }

        private void LogHTTPRequest(Request request, bool logFullRequest, TimeSpan? duration, ClaimsPrincipal user, ISet<GRYLogTarget> logTargets, IDictionary<string, IList<string?>> header)
        {
            try
            {
                LogLevel logLevel = this.GetLogLevel(request);
                string formatted;
                if (logFullRequest)
                {
                    formatted = this.FormatLogEntryFull(request, duration, user, this._RequestLoggingSettings.MaximalLengthofRequestBodies, this._RequestLoggingSettings.MaximalLengthOfResponseBodies, header);
                }
                else
                {
                    formatted = this.FormatLogEntrySummary(request, duration, user);
                }
                LogItem logItem = new LogItem(this._TimeService.GetCurrentLocalTimeAsDateTimeOffset(), formatted, logLevel)
                {
                    LogTargets = logTargets
                };
                this._RequestLogger.AddLogEntry(logItem);
            }
            catch (Exception exception)
            {
                this._Logger.Log("Error while logging request.", exception);
            }
        }

        public virtual LogLevel GetLogLevel(Request request)
        {
            return (request.ResponseStatusCode / 100) switch
            {
                2 => LogLevel.Information,
                3 => LogLevel.Debug,
                4 => LogLevel.Information,
                5 => LogLevel.Error,
                _ => LogLevel.Error,
            };
        }

        public virtual bool ShouldLogEntireRequestContentInLogFile(Request request)
        {
            if (request.ResponseStatusCode / 100 == 5)
            {
                return true;
            }
            return true;
        }

        private bool IsIgnored(string route)
        {
            foreach (string notLoggedRoute in this._RequestLoggingSettings.NotLoggedRoutes)
            {
                if (Regex.IsMatch(route, notLoggedRoute))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual string FormatLogEntryFull(Request request, TimeSpan? duration, ClaimsPrincipal user, uint maximalLengthofRequestBodies, uint maximalLengthofResponseBodies, IDictionary<string, IList<string?>> header)
        {
            string clientIPAsString = this.FormatIPAddress(request.ClientIPAddress);
            string result = $"Request received:{Environment.NewLine}"
                        + $"  Request-id: {request.RequestId}{Environment.NewLine}"
                        + $"  Timestamp: {this.FormatTimestamp(request.Timestamp)}{Environment.NewLine}"
                        + $"  Client-ip: {clientIPAsString}{Environment.NewLine}"
                        + $"  Request-details:{Environment.NewLine}"
                        + $"    Method: {request.Method}{Environment.NewLine}"
                        + $"    Route: {request.Route}{request.GetFormattedQuery()}{Environment.NewLine}";
            if (0 < header.Count)
            {
                result = result + $"  Header:{Environment.NewLine}";
                foreach (KeyValuePair<string, IList<string?>> kvp in header)
                {
                    string value = "{" + string.Join(", ", kvp.Value.Select(item => "\"" + item + "\"")) + "}";
                    result = result + $"    - {kvp.Key}: {value}{Environment.NewLine}";
                }
            }
            result = result + $"    Body: {this.FormatBody(request.RequestBody, maximalLengthofRequestBodies)}{Environment.NewLine}"
                        + $"  Response-details:{Environment.NewLine}"
                        + $"    Statuscode: {request.ResponseStatusCode}{Environment.NewLine}"
                        + $"    Body: {this.FormatBody(request.ResponseBody, maximalLengthofResponseBodies)}{Environment.NewLine}";
            if (user == null)
            {
                result = result + $"  Authentication: (anonymous){Environment.NewLine}";
            }
            else
            {
                if (user.Identity == null)
                {
                    result = result + $"  Authentication: (unknown identity){Environment.NewLine}";
                }
                else
                {
                    result = result + $"  Authentication: user \"{user.Identity.Name}\"{Environment.NewLine}";
                }
            }
            if (duration.HasValue)
            {
                result = result + $"  Duration: {GUtilities.DurationToUserFriendlyString(duration.Value, 5)}{Environment.NewLine}";
            }
            return result;
        }

        private string FormatBody((string info, string content, byte[] plainContent) body, uint maximalLengthofRequestBodies)
        {
            string result;
            if (body.content == null)
            {
                result = body.info;
            }
            else
            {
                result = $"{body.info} ({this.Truncate(body.content, maximalLengthofRequestBodies, (ulong)body.plainContent.LongLength)})";
            }
            return result;
        }

        public virtual bool ShouldBeLogged(Request request)
        {
            if (this._CommandlineParameter.EnforceVerbose)
            {
                return true;
            }
            if (this.IsIgnored(request.Route))
            {
                return false;
            }
            return true;
        }
        public virtual string FormatLogEntrySummary(Request request, TimeSpan? duration, ClaimsPrincipal user)
        {
            string clientIPAsString = this.FormatIPAddress(request.ClientIPAddress);
            string additionalInformationFinal;
            string additionalInformation = this.GetAdditionalInformation(request, clientIPAsString, duration, user);
            if (additionalInformation == null)
            {
                additionalInformation = string.Empty;
            }
            else
            {
                additionalInformation = $" ({additionalInformation})";
            }
            additionalInformationFinal = additionalInformation;
            return $"Request received: {this.FormatTimestamp(request.Timestamp)} Id: {request.RequestId}; {clientIPAsString} requested \"{request.Method} {request.Route}{request.GetFormattedQuery()}\" and got response-code {request.ResponseStatusCode}.{additionalInformationFinal}";
        }

        public virtual string GetAdditionalInformation(Request request, string clientIPAsString, TimeSpan? duration, ClaimsPrincipal user)
        {
            string result = null;
            if (user == null)
            {
                result = this.AddAdditionalInformtion(result, $"Authentication: (anonymous)");
            }
            else
            {
                result = this.AddAdditionalInformtion(result, $"Authentication: user \"{user.Identity.Name}\"");
            }
            if (duration.HasValue)
            {
                result = this.AddAdditionalInformtion(result, $"Duration: {GUtilities.DurationToUserFriendlyString(duration.Value, 0)}");
            }
            //add more additional information if desired
            return result;
        }

        private string AddAdditionalInformtion(string result, string message)
        {

            if (result == null)
            {
                return message;
            }
            else
            {
                return $"{result}; {message}";
            }
        }

        public virtual string FormatTimestamp(DateTimeOffset timestamp)
        {
            return GUtilities.FormatTimestamp(timestamp, this._RequestLoggingSettings.AddMillisecondsInLogTimestamps);
        }

        public virtual string Truncate(string value, uint maxLength, ulong originalLength)
        {
            if (value.Length <= maxLength)
            {
                return $"\"{value}\"";
            }
            else
            {
                return $"\"{value[..(int)maxLength]}...\" (Content truncated; Original length: {originalLength} bytes)";
            }
        }
        public virtual string FormatIPAddress(IPAddress? clientIP)
        {
            if (this._RequestLoggingSettings.LogClientIP)
            {
                if (clientIP == null)
                {
                    return "(IP-address not available)";
                }
                else
                {
                    return clientIP.ToString();
                }
            }
            else
            {
                return "(IP-address not saved)";
            }
        }
    }
}