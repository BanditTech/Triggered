using System;
using System.Diagnostics;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Provides utility methods related to handling Windows processes and windows.
    /// </summary>
    public static class Windows
    {
        /// <summary>
        /// Retrieves the handle (HWND) of the main window associated with the specified process.
        /// </summary>
        /// <param name="exeFileName">The name of the executable file (default: "PathOfExile").</param>
        /// <returns>The handle (HWND) of the main window if found; otherwise, IntPtr.Zero.</returns>
        public static IntPtr GetProcessHandle(string exeFileName = "PathOfExile")
        {
            Process[] processes = Process.GetProcessesByName(exeFileName);
            IntPtr hwnd = IntPtr.Zero;
            if (processes.Length > 0)
                hwnd = processes[0].MainWindowHandle;
            return hwnd;
        }
    }
}
