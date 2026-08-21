namespace GRYLibrary.Core.APIServer.CommonRoutes
{
    public class CommonRoutesInformation : ICommonRoutesInformation
    {
        /// <inheritdoc/>
        public string? TermsOfServiceLink { get; set; }

        /// <inheritdoc/>
        public string? ContactLink { get; set; }

        /// <inheritdoc/>
        public string? LicenseLink { get; set; }
    }
}
