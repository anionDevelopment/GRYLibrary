using System.Threading.Tasks;

namespace GRYLibrary.Core.APIServer.BaseServices
{
    public interface IIteratingBackgroundService
    {
        /// <summary>Whether the service also runs when the execution-mode is not <c>RunProgram</c>.</summary>
        public bool RunAlsoWhenTheExecutionModeIsNotRunProgram { get; set; }
        public void StartAsync();
        public Task Stop();
    }
}
