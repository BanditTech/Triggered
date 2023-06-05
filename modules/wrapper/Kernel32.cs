using System;
using System.Runtime.InteropServices;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Provides a C# wrapper for the GetModuleHandle function in the kernel32.dll library.
    /// </summary>
    public static class Kernel32
    {
        /// <summary>
        /// Retrieves the handle of a loaded module (DLL) in the current process.
        /// </summary>
        /// <param name="lpModuleName">The name of the module or DLL. Pass null to retrieve the handle of the executable file of the current process.</param>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the specified module.
        /// If the function fails, the return value is IntPtr.Zero. To get extended error information, call Marshal.GetLastWin32Error().
        /// </returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
