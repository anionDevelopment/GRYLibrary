namespace GRYLibrary.Core.APIServer.CommonRoutes
{
    public interface ICommonRoutesInformation
    {
        /// <summary>
        /// The address the route "TermsOfService" redirects to, or null when this application has none. A link which
        /// is null means that the route is not hosted at all, because a route which exists and answers with nothing
        /// would only look like an information which is missing.
        /// </summary>
        public string? TermsOfServiceLink { get; set; }

        /// <summary>The address the route "Contact" redirects to, or null when this application has none.</summary>
        public string? ContactLink { get; set; }

        /// <summary>The address the route "License" redirects to, or null when this application has none.</summary>
        public string? LicenseLink { get; set; }
    }
}
