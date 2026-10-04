using GRYLibrary.Core.APIServer.MidT.RLog;
using GRYLibrary.Core.Logging.GRYLogger;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Mid.M05DLog
{
    public interface IDRequestLoggingConfiguration : IRequestLoggingConfiguration
    {
        public bool AddMillisecondsInLogTimestamps { get; set; }
        public bool LogClientIP { get; set; }
        public GRYLogConfiguration RequestsLogConfiguration { get; set; }
        public uint MaximalLengthofRequestBodies { get; set; }
        public uint MaximalLengthOfResponseBodies { get; set; }
        public ISet<string> NotLoggedRoutes { get; set; }
        public ISet<string> LoggedHTTPRequeustHeader { get; set; }

        /// <summary>
        /// Routes (as regular-expressions) whose request-body is not written to the log-file. The route is still
        /// logged, only its request-body is replaced by a placeholder. Empty by default.
        /// </summary>
        public ISet<string> RoutesWhereRequestBodyIsNotLogged { get; set; }

        /// <summary>
        /// Routes (as regular-expressions) whose response-body is not written to the log-file. The route is still
        /// logged, only its response-body is replaced by a placeholder. Empty by default. Use this for example for the
        /// login-route, whose response-body contains the issued access-token.
        /// </summary>
        public ISet<string> RoutesWhereResponseBodyIsNotLogged { get; set; }
    }
}