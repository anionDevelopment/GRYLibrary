using System;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Core.APIServer.Utilities
{
    /// <remarks>
    /// This attributes implies <see cref="AuthenticateAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute
    {
        public ISet<string> Groups { get; private set; }
        public AuthorizeAttribute() : this(null)
        {
        }
        public AuthorizeAttribute(params string[]? groups)
        {
            if (groups == null)
            {
                this.Groups = new HashSet<string>();
            }
            else
            {
                this.Groups = groups.ToHashSet();
            }
        }
    }
}
