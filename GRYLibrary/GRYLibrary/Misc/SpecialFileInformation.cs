
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using GRYLibrary.Core.OperatingSystem;
using GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GRYLibrary.Core.Misc
{
    public static class SpecialFileInformation
    {
        #region Execute or open file
        public static bool FileIsExecutable(string file)
        {
            return OperatingSystem.OperatingSystem.GetCurrentOperatingSystem().Accept(new FileIsExecutableVisitor(file));
        }

        public static ExternalProgramExecutor ExecuteFile(string file)
        {
            if (FileIsExecutable(file))
            {
                ExternalProgramExecutor result = new(file, string.Empty);
                result.Run();
                return result;
            }
            else
            {
                throw new Exception($"File '{file}' can not be executed");
            }
        }

        public static void OpenFileWithDefaultProgram(string file)
        {
            new ExternalProgramExecutor(new ExternalProgramExecutorConfiguration { Program = file, WaitingState = new RunAsynchronously() }).Run();
        }

        private class FileIsExecutableVisitor : IOperatingSystemVisitor<bool>
        {
            private readonly string _File;

            public FileIsExecutableVisitor(string file)
            {
                this._File = file;
            }

            public bool Handle(OSX operatingSystem)
            {
                return true;
            }

            public bool Handle(GRYLibrary.Core.OperatingSystem.ConcreteOperatingSystems.Windows operatingSystem)
            {
                string fileToLower = this._File.ToLower();
                return fileToLower.EndsWith(".exe")
                    || fileToLower.EndsWith(".cmd")
                    || fileToLower.EndsWith(".bat");
            }

            public bool Handle(Linux operatingSystem)
            {
                return true;
            }
        }
        #endregion

        #region Get file extension on windows
        public static string GetDefaultProgramToOpenFile(string extensionWithDot)
        {
            return FileExtentionInfo(AssocStr.Executable, extensionWithDot);
        }

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);
        private static string FileExtentionInfo(AssocStr assocStr, string extensionWithDot)
        {
#pragma warning disable CA1806 // Do not ignore method results
            uint pcchOut = 0;
            AssocQueryString(AssocF.Verify, assocStr, extensionWithDot, null, null, ref pcchOut);
            StringBuilder pszOut = new((int)pcchOut);
            AssocQueryString(AssocF.Verify, assocStr, extensionWithDot, null, pszOut, ref pcchOut);
#pragma warning restore CA1806 // Do not ignore method results
            return pszOut.ToString();
        }

        [Flags]
        private enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
#pragma warning disable CA1069 // Enums values should not be duplicated
            Open_ByExeName = 0x2,
#pragma warning restore CA1069 // Enums values should not be duplicated
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        private enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }
        #endregion

    }
}