using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Misc.FilePath;

namespace GRYLibrary.Core.APIServer.Utilities
{
    public class GetProcolVisitor : IExecutionModeVisitor<Protocol>
    {
        private readonly HTTP _HTTP;
        private readonly string _Domain;

        public GetProcolVisitor(string domain)
        {
            this._Domain = domain;
            this._HTTP = new HTTP();
        }
        public GetProcolVisitor(string domain, ushort httpPort)
        {
            this._Domain = domain;
            this._HTTP = new HTTP(httpPort);
        }

        public Protocol Handle(Analysis analysis)
        {
            return this._HTTP;
        }

        public Protocol Handle(RunProgram runProgram)
        {
            return new HTTPS(new TLSCertificateInformation
            {
                CertificatePFXFile = AbstractFilePath.FromString($"./{this._Domain}.pfx"),
                CertificatePasswordFile = AbstractFilePath.FromString($"./{this._Domain}.password"),
            });
        }

        public Protocol Handle(TestRun testRun)
        {
            return this._HTTP;
        }
    }
}
