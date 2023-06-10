using System;
using System.Diagnostics;

namespace Triggered.modules.wrapper
{
    public static class Windows
    {
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
