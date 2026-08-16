using GRYLibrary.Core.Misc.ConsoleApplication;

namespace GRYLibrary.Core.APIServer.Verbs
{
    public interface IAPIServerCommandlineParameter : ICommandlineParameter
    {
        public bool RealRun { get; set; }
        public bool InitialVerboseValue { get; set; }
        public bool EnforceVerbose { get; set; }
    }
}
