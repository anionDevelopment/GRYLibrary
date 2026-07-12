using System;
using System.Runtime.InteropServices;

namespace GRYLibrary.Core.Misc
{
    public static class ConsoleExtensions
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        public static void ShowConsoleWindow()
        {
            ShowWindow(GetConsoleWindow(), SW_SHOW);
        }

        public static void HideConsoleWindow()
        {
            ShowWindow(GetConsoleWindow(), SW_HIDE);
        }
    }
}
