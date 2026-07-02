using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GRYLibrary.Core.APIServer.Utilities
{
    //Represents an entire request-handling-object, including its response.
    public class Request
    {
        public string RequestId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool LogClientIP { get; set; }
        public IPAddress? ClientIPAddress { get; set; }
        public string Method { get; set; }
        public string Route { get; set; }
        public IHeaderDictionary RequestHeader { get; set; }
        public (string info, string? content, byte[] plainContent) RequestBody { get; set; }
        public IDictionary<object, object> InformationFromController { get; set; }
        public ushort ResponseStatusCode { get; set; }
        public IHeaderDictionary ResponseHeader { get; set; }
        public (string info, string? content, byte[] plainContent) ResponseBody { get; set; }
        public IQueryCollection Query { get; set; }
        public Request()
        {
        }
        public Request(string requestId, DateTimeOffset timestamp, IPAddress? clientIPAddress, string method, string route, IQueryCollection query, IHeaderDictionary requestHeader, (string info, string? content, byte[] plainContent) requestBody, IDictionary<object, object> informationFromController, ushort responseStatusCode, IHeaderDictionary responseHeader, (string info, string? content, byte[] plainContent) responseBody)
        {
            this.RequestId = requestId;
            this.Timestamp = timestamp;
            this.ClientIPAddress = clientIPAddress;
            this.Method = method;
            this.Route = route;
            this.RequestHeader = requestHeader;
            this.RequestBody = requestBody;
            this.InformationFromController = informationFromController;
            this.ResponseStatusCode = responseStatusCode;
            this.ResponseHeader = responseHeader;
            this.ResponseBody = responseBody;
            this.Query = query;
        }

        internal string GetFormattedQuery()
        {
            if (this.Query.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                return "?" + string.Join(",", this.Query.Select(kvp => kvp.Key + "=" + kvp.Value));
            }
        }
    }
}
