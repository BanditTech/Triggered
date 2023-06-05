using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Wrapper for the user32.dll library
    /// </summary>
    public static class User32
    {
        /// <summary>
        /// Retrieves the device context (DC) for the entire window, including title bar, menus, and scroll bars.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        /// <summary>
        /// Releases the device context (DC) for the specified window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window.
        /// </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

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

        /// <summary>
        /// Retrieves the handle of the window located at the specified point on the screen.
        /// </summary>
        /// <param name="point">The coordinates of the point on the screen.</param>
        /// <returns>The handle of the window located at the specified point.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

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
        public static extern int EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        /// Retrieves the text of the specified window's title bar (if it has one).
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd,ref StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Retrieves the length of the title text of a window.
        /// </summary>
        /// <param name="hWnd">The handle of the window.</param>
        /// <returns>The length of the window's title text.</returns>
        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// This function installs an application-defined hook procedure into a system hook chain.
        /// The hook procedure monitors mouse events and provides the ability to intercept and modify them.
        /// </summary>
        /// <param name="idHook">The type of hook procedure to be installed.</param>
        /// <param name="callback">A pointer to the hook procedure.</param>
        /// <param name="hMod">A handle to the DLL containing the hook procedure.</param>
        /// <param name="dwThreadId">The identifier of the thread with which the hook procedure is to be associated.</param>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the hook procedure.
        /// If the function fails, the return value is NULL.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, MouseHookCallback callback, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// Represents the callback method for the mouse hook. This method is called whenever a mouse event occurs.
        /// </summary>
        /// <param name="nCode">A code that the hook procedure uses to determine how to process the message.</param>
        /// <param name="wParam">The identifier of the mouse message.</param>
        /// <param name="lParam">A pointer to a MOUSEHOOKSTRUCT structure containing information about the mouse event.</param>
        /// <returns>
        /// If the hook procedure processed the message, it may return a nonzero value
        /// to prevent the system from passing the message to the rest of the hook chain or the target window procedure.
        /// </returns>
        public delegate IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="hhk">A handle to the hook to be removed.</param>
        /// <returns>If the function succeeds, the return value is true. If the function fails, the return value is false.</returns>
        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        /// <summary>
        /// This function passes the hook information to the next hook procedure in the current hook chain.
        /// </summary>
        /// <param name="hhk">A handle to the current hook.</param>
        /// <param name="nCode">The code that the hook procedure uses to determine how to process the message.</param>
        /// <param name="wParam">The identifier of the mouse message.</param>
        /// <param name="lParam">A pointer to a MOUSEHOOKSTRUCT structure containing information about the mouse event.</param>
        /// <returns>The return value is the result of the next hook procedure in the hook chain.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Retrieves the title of a window given its handle.
        /// </summary>
        /// <param name="hWnd">The handle of the window.</param>
        /// <returns>The title of the window as a string.</returns>
        public static string GetWindowTitle(IntPtr hWnd)
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
        public static IntPtr GetWindowHandle(string windowName)
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
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// Represents the coordinates of a rectangle's edges.
        /// </summary>
        public struct RECT
        {
            /// <summary>
            /// The x-coordinate of the left edge of the rectangle.
            /// </summary>
            public int Left;

            /// <summary>
            /// The y-coordinate of the top edge of the rectangle.
            /// </summary>
            public int Top;

            /// <summary>
            /// The x-coordinate of the right edge of the rectangle.
            /// </summary>
            public int Right;

            /// <summary>
            /// The y-coordinate of the bottom edge of the rectangle.
            /// </summary>
            public int Bottom;
        }

        /// <summary>
        /// Represents a point in two-dimensional space.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// The x-coordinate of the point.
            /// </summary>
            public int X;

            /// <summary>
            /// The y-coordinate of the point.
            /// </summary>
            public int Y;
        }

        /// <summary>
        /// Specifies the mouse low-level hook type.
        /// </summary>
        public const int WH_MOUSE_LL = 14;

        /// <summary>
        /// Specifies the mouse move message.
        /// </summary>
        public const int WM_MOUSEMOVE = 0x0200;

        /// <summary>
        /// Specifies the left button down message.
        /// </summary>
        public const int WM_LBUTTONDOWN = 0x0201;

        /// <summary>
        /// Specifies the left button up message.
        /// </summary>
        public const int WM_LBUTTONUP = 0x0202;

        /// <summary>
        /// Specifies the right button down message.
        /// </summary>
        public const int WM_RBUTTONDOWN = 0x0204;

        /// <summary>
        /// Specifies the right button up message.
        /// </summary>
        public const int WM_RBUTTONUP = 0x0205;

    }
}
