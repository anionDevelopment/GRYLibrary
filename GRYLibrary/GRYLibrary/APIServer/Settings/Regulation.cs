using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Settings
{
    public abstract class Regulation
    {
        public virtual IList<Type> RequiredServices { get; set; } = [];//TODO enforce this service
    }
}
