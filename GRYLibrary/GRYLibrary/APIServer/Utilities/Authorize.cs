using System;
using System.Collections.Generic;
using System.Linq;

namespace GRYLibrary.Core.APIServer.Utilities
{
    /// <summary>
    /// Marks the annotated action as requiring an authorization-check against the given authorized groups.
    /// </summary>
    /// <remarks>
    /// This attributes implies <see cref="AuthenticateAttribute"/>.
    /// An empty set of groups does not disable the authorization-check: the check is still performed, with no group being accepted.
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
