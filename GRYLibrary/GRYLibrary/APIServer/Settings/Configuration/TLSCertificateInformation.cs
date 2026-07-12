using GRYLibrary.Core.Misc.FilePath;

namespace GRYLibrary.Core.APIServer.Settings.Configuration
{
    public class TLSCertificateInformation
    {
        public string GetCertificatePasswordFile(string certificateFolder)
        {
            return this.CertificatePasswordFile.GetPath(certificateFolder);
        }

        public string GetCertificatePFXFile(string certificateFolder)
        {
            return this.CertificatePFXFile.GetPath(certificateFolder);
        }

        public AbstractFilePath CertificatePFXFile { get; set; } = default;
        public AbstractFilePath CertificatePasswordFile { get; set; } = default;
    }
}
