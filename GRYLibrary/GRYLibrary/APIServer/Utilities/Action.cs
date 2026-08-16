using System;

namespace GRYLibrary.Core.APIServer.Utilities
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        public string Action { get; private set; }
        public ActionAttribute() : this(null)
        {
        }
        public ActionAttribute(string action)
        {
            this.Action = action;
        }
    }
}
