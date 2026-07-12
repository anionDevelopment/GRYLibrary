using Microsoft.EntityFrameworkCore;

namespace GRYLibrary.Core.APIServer.MFA
{
    [PrimaryKey(nameof(SecretKey))]
    public class TOTP : IMFAMethod
    {
        public string SecretKey { get; set; }
        public bool IsActicated { get; set; }
    }
}
