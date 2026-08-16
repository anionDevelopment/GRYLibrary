using GRYLibrary.Core.APIServer.Settings.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.APIServer.Utilities
{
    public sealed class TestServer : IDisposable
    {
        public static readonly TimeSpan WaitTimeoutOfProcessStartUntilServiceIsAvailable = TimeSpan.FromSeconds(30);
        private readonly Process _Process;
        public string APIKey { get; private set; }
        public string ProgramFile { get; private set; }

        public TestServer(string programFile, string apiKey) : this(programFile, apiKey, WaitTimeoutOfProcessStartUntilServiceIsAvailable)
        {
        }

        public TestServer(string programFile, string apiKey, TimeSpan timoutOfProcessStartUntilServiceIsAvailable)
        {
            this.ProgramFile = programFile;
            this.APIKey = apiKey;

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = this.ProgramFile,
                    Arguments = "--TestRun",
                    WorkingDirectory = Path.GetDirectoryName(this.ProgramFile),
                },
                EnableRaisingEvents = true
            };
            //TODO terminate existing container if available and if desired
            this._Process = process;
            process.Start();
            GUtilities.WaitUntilPortIsAvailable("127.0.0.1", HTTP.DefaultPort, timoutOfProcessStartUntilServiceIsAvailable);
        }

        public void Dispose()
        {
            this._Process.Kill();
            this._Process.Dispose();
        }
    }
}
