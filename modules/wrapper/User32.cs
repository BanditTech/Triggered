using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Wrapper for the user32.dll library
    /// </summary>
    internal static class User32
    {
        /// <summary>
        /// Retrieves the device context (DC) for the entire window, including title bar, menus, and scroll bars.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr hWnd);

        /// <summary>
        /// Releases the device context (DC) for the specified window.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an application-defined callback function.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        /// Retrieves the text of the specified window's title bar (if it has one).
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Utilize EnumWindows to match the title bar.
        /// </summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        internal static IntPtr GetWindowHandle(string windowName)
        {
            IntPtr hwnd = IntPtr.Zero;
            EnumWindows((IntPtr wnd, IntPtr param) =>
            {
                StringBuilder stringBuilder = new StringBuilder(256);
                GetWindowText(wnd, stringBuilder, stringBuilder.Capacity);
                if (stringBuilder.ToString() == windowName)
                {
                    hwnd = wnd;
                    return false; // Stop enumerating windows
                }
                return true; // Continue enumerating windows
            }, IntPtr.Zero);

            return hwnd;
        }

        /// <summary>
        /// Defines the callback function for EnumWindows.
        /// </summary>
        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
        /// </summary>
        internal struct RECT
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;
        }
    }
}
