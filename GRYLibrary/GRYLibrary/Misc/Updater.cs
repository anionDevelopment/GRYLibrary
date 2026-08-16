using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GRYLibrary.Core.Misc
{
    public class Updater
    {
        private readonly string _CurrentLocation;
        private readonly string _AppName;
        private readonly IList<string> _Locations;
        private readonly Func<Version> _GetLatestVersion;
        private readonly Func<byte[]> _GetArchiveOfLatestVersion;
        private byte[] _ArchiveOfLatestVersion = null;
        public string CommandlineArgumentsForNewInstance { get; set; } = null;
        public Updater(IList<string> locations, Func<Version> getLatestVersion, Func<byte[]> getArchiveOfLatestVersion)
        {
            this._CurrentLocation = this.GetCurrentLocation();
            this._AppName = this.GetAppName();
            this._Locations = locations;
            this._GetLatestVersion = getLatestVersion;
            this._GetArchiveOfLatestVersion = getArchiveOfLatestVersion;
        }

        private string GetAppName()
        {
            return Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private string GetCurrentLocation()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        public void Update()
        {
            if (!this._Locations.Contains(this._CurrentLocation))
            {
                throw new ArgumentException("The current location must be contained in the list of all locations.");
            }
            int minimumRequiredAmount = 2;
            if (this._Locations.Count < minimumRequiredAmount)
            {
                throw new ArgumentException($"At least {minimumRequiredAmount} locations are required.");
            }
            Version latestVersion = this._GetLatestVersion();
            Version versionOfCurrentLocation = this.GetVersionOfLocation(this._CurrentLocation);
            bool currentVersionIsOutdated = !latestVersion.Equals(versionOfCurrentLocation);
            string locationForRestart = null;
            foreach (string location in this._Locations)
            {
                if (location != this._CurrentLocation)
                {
                    Version versionOfLocation = this.GetVersionOfLocation(location);
                    if (!latestVersion.Equals(versionOfLocation))
                    {
                        this.UpdateLocation(location);
                    }
                }
                locationForRestart = location;
            }
            if (currentVersionIsOutdated)
            {
                this.StartLocation(locationForRestart);
                this.StopCurrentLocation();
            }
        }


        private Version GetVersionOfLocation(string location)
        {
            return this.GetVersionOfDLLFile(this.GetProductDLLFile(location));
        }

        private string GetProductDLLFile(string location)
        {
            return Path.Combine(location, $"{this._AppName}.dll");
        }

        private Version GetVersionOfDLLFile(string file)
        {
            return new Version(FileVersionInfo.GetVersionInfo(file).ProductVersion);
        }

        private void StopCurrentLocation()
        {
            Environment.Exit(0);
        }

        private void StartLocation(string location)
        {
            string filename = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location);
            ExternalProgramExecutor externalProgramExecutor = new ExternalProgramExecutor(Path.Combine(location, filename), this.CommandlineArgumentsForNewInstance);
            externalProgramExecutor.Configuration.WaitingState = new RunAsynchronously();
            externalProgramExecutor.Run();
        }

        private void UpdateLocation(string location)
        {
            this.DownloadLatestVersionIfRequired();
            throw new NotImplementedException();//TODO replace program in "location" by "_ArchiveOfLatestVersion"
        }

        private void DownloadLatestVersionIfRequired()
        {
            if (this._ArchiveOfLatestVersion == null)
            {
                this._ArchiveOfLatestVersion = this._GetArchiveOfLatestVersion();
            }
        }
    }
}