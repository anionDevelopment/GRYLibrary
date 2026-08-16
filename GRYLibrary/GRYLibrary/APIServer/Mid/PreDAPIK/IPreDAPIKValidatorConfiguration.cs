using GRYLibrary.Core.APIServer.Mid.M01APIK;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Mid.PreDAPIK
{
    public interface IPreDAPIKValidatorConfiguration : IAPIKeyValidatorConfiguration
    {
        public IList<string> AuthorizedAPIKeys { get; set; }
    }
}
