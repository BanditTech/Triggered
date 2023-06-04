﻿using System;
using System.Drawing;
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
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Retrieves the rectangle that represents the position and size of a window.
        /// </summary>
        /// <param name="targetWindow">The handle of the target window.</param>
        /// <returns>A Rectangle object representing the window's position and size.</returns>
        public static Rectangle GetWindowRectangle(IntPtr targetWindow)
        {
            RECT windowRect;
            GetWindowRect(targetWindow, out windowRect);
            return new Rectangle(
                windowRect.Left, 
                windowRect.Top, 
                windowRect.Right - windowRect.Left + 1,
                windowRect.Bottom - windowRect.Top + 1
            );
        }

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        /// <summary>
        /// Retrieves the position of the cursor, in screen coordinates.
        /// </summary>
        /// <param name="lpPoint">The position of the cursor.</param>
        /// <returns>
        /// Returns true if successful, or false otherwise.
        /// </returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// Translates the screen coordinates of a specified point on the screen to client coordinates.
        /// </summary>
        /// <param name="hWnd">A handle to the window whose client coordinates should be retrieved.</param>
        /// <param name="lpPoint">The screen coordinates to be translated.</param>
        /// <returns>
        /// Returns true if successful, or false otherwise.
        /// </returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        /// <summary>
        /// Retrieves the position of the mouse relative to the specified window.
        /// </summary>
        /// <param name="targetWindow">A handle to the window to which the mouse position is relative.</param>
        /// <returns>
        /// Returns a Point object representing the mouse position relative to the specified window.
        /// </returns>
        public static Point GetRelativeMousePosition(IntPtr targetWindow)
        {
            POINT point;
            GetCursorPos(out point);
            ScreenToClient(targetWindow, ref point);
            return new Point(point.X, point.Y);
        }

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an application-defined callback function.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        /// Retrieves the text of the specified window's title bar (if it has one).
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowText(IntPtr hWnd,ref StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// Retrieves the title of a window given its handle.
        /// </summary>
        /// <param name="hWnd">The handle of the window.</param>
        /// <returns>The title of the window as a string.</returns>
        internal static string GetWindowTitle(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd) + 1;
            var titleBuilder = new StringBuilder(length);
            // Retrieve the window's title and store it in the titleBuilder.
            GetWindowText(hWnd,ref titleBuilder, length);
            return titleBuilder.ToString();
        }

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
                int length = GetWindowTextLength(wnd) + 1;
                StringBuilder stringBuilder = new StringBuilder(length);
                GetWindowText(wnd,ref stringBuilder, stringBuilder.Capacity);
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

        /// <summary>
        /// Represents a point in two-dimensional space.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
