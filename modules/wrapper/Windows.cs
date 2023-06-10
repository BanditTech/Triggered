using System;
using System.Diagnostics;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Provides utility methods related to handling Windows processes and windows.
    /// </summary>
    public static class Windows
    {
        private static string[] PathOfExileProcessNames = {
            "PathOfExile",
            "PathOfExile_x64",
            "PathOfExileSteam",
            "PathOfExile_x64Steam",
            "PathOfExile_KG",
            "PathOfExile_x64_KG",
            "PathOfExile_x64EGS",
            "PathOfExileEGS"
        };
        private static string foundProcess = FindGameProcess();

        /// <summary>
        /// Retrieves the handle (HWND) of the main window associated with the specified process.
        /// </summary>
        /// <param name="exeFileName">The name of the executable file (default: "PathOfExile").</param>
        /// <returns>The handle (HWND) of the main window if found; otherwise, IntPtr.Zero.</returns>
        public static IntPtr GetProcessHandle()
        {
            // Validate the process exists and is found
            if (foundProcess == null)
            {
                foundProcess = FindGameProcess();
                if (foundProcess == null)
                    return IntPtr.Zero;
            }

            Process[] processes = Process.GetProcessesByName(foundProcess);
            IntPtr hwnd = IntPtr.Zero;
            if (processes.Length > 0)
                hwnd = processes[0].MainWindowHandle;
            return hwnd;
        }

        /// <summary>
        /// Determine the process name
        /// </summary>
        /// <returns></returns>
        public static string FindGameProcess()
        {
            foreach (string processName in PathOfExileProcessNames)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    return processName;
                }
            }
            return null;
        }
    }
}
