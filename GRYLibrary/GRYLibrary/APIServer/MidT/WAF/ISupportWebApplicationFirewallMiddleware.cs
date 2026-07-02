namespace GRYLibrary.Core.APIServer.MidT.WAF
{
    public interface ISupportWebApplicationFirewallMiddleware : ISupportedMiddleware
    {
        public IWebApplicationFirewallConfiguration ConfigurationForWebApplicationFirewall { get; }
    }
}
