using System;

namespace GRYLibrary.Core.APIServer.Utilities
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthenticateAttribute : Attribute
    {
    }
}
