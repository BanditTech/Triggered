using System;
using System.Drawing;
using System.Numerics;
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

            /// <summary>
            /// Define the calculated Width
            /// </summary>
            public readonly int Width { get => Math.Abs(Left - Right); }

            /// <summary>
            /// Define the calculated Height
            /// </summary>
            public readonly int Height { get => Math.Abs(Top - Bottom); }

            /// <summary>
            /// Produce the reference point of this rectangle
            /// </summary>
            public readonly Point Point => new Point(Math.Min(Left, Right), Math.Min(Top, Bottom));

            /// <summary>
            /// Produce the sizes of the rectangle.
            /// </summary>
            public readonly Size Size => new Size(Width, Height);

            /// <summary>
            /// Produce a Rectangle from this RECT.
            /// </summary>
            public readonly Rectangle Rectangle => new(Point, Size);
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

            /// <summary>
            /// Produce a Vector2 from this POINT
            /// </summary>
            public readonly Vector2 Vector2 => new(X, Y);
            
            /// <summary>
            /// Produce a Point from this POINT
            /// </summary>
            public readonly Point Point => new(X, Y);
        }

        /// <summary>
        /// Represents constants for Windows hook types.
        /// </summary>
        public static class HookType
        {
            /// <summary>
            /// Specifies the message filter hook type.
            /// </summary>
            public const int WH_MSGFILTER = -1;

            /// <summary>
            /// Specifies the journal record hook type.
            /// This hook type is used to record input messages posted to the system message queue.
            /// </summary>
            public const int WH_JOURNALRECORD = 0;

            /// <summary>
            /// Specifies the journal playback hook type.
            /// This hook type is used to play back a series of input messages previously recorded by a WH_JOURNALRECORD hook procedure.
            /// </summary>
            public const int WH_JOURNALPLAYBACK = 1;

            /// <summary>
            /// Specifies the keyboard hook type.
            /// This hook type enables an application to monitor keystrokes.
            /// </summary>
            public const int WH_KEYBOARD = 2;

            /// <summary>
            /// Specifies the get message hook type.
            /// This hook type enables an application to monitor messages posted to the system message queue.
            /// </summary>
            public const int WH_GETMESSAGE = 3;

            /// <summary>
            /// Specifies the call window procedure hook type.
            /// This hook type enables an application to monitor messages before they are processed by the receiving window procedure.
            /// </summary>
            public const int WH_CALLWNDPROC = 4;

            /// <summary>
            /// Specifies the CBT (computer-based training) hook type.
            /// This hook type enables an application to monitor and modify messages and notifications sent to a CBT-based window.
            /// </summary>
            public const int WH_CBT = 5;

            /// <summary>
            /// Specifies the system message filter hook type.
            /// This hook type enables an application to monitor and modify messages before they are dispatched to the system message queue.
            /// </summary>
            public const int WH_SYSMSGFILTER = 6;

            /// <summary>
            /// Specifies the mouse hook type.
            /// This hook type enables an application to monitor mouse messages.
            /// </summary>
            public const int WH_MOUSE = 7;

            /// <summary>
            /// Specifies the hardware hook type.
            /// This hook type enables an application to monitor low-level keyboard and mouse input events.
            /// </summary>
            public const int WH_HARDWARE = 8;

            /// <summary>
            /// Specifies the debug hook type.
            /// This hook type enables an application to receive debugging messages intended for other applications.
            /// </summary>
            public const int WH_DEBUG = 9;

            /// <summary>
            /// Specifies the shell hook type.
            /// This hook type enables an application to receive messages notifications for shell events.
            /// </summary>
            public const int WH_SHELL = 10;

            /// <summary>
            /// Specifies the foreground idle hook type.
            /// This hook type enables an application to perform tasks while the system is idle and no events are being processed.
            /// </summary>
            public const int WH_FOREGROUNDIDLE = 11;

            /// <summary>
            /// Specifies the call window procedure return hook type.
            /// This hook type enables an application to monitor messages after they have been processed by the receiving window procedure.
            /// </summary>
            public const int WH_CALLWNDPROCRET = 12;

            /// <summary>
            /// Specifies the keyboard low-level hook type.
            /// This hook type enables an application to monitor low-level keyboard input events.
            /// </summary>
            public const int WH_KEYBOARD_LL = 13;

            /// <summary>
            /// Specifies the mouse low-level hook type.
            /// This hook type enables an application to monitor low-level mouse input events.
            /// </summary>
            public const int WH_MOUSE_LL = 14;
        }

        /// <summary>
        /// Types of messages found in Windows Hook events.
        /// </summary>
        public static class MessageType
        {
            // Left Mouse Button
            public const int WM_LBUTTONDOWN = 0x0201;
            public const int WM_NCLBUTTONDOWN = 0x00A1;
            public const int WM_LBUTTONUP = 0x0202;
            public const int WM_NCLBUTTONUP = 0x00A2;
            public const int WM_LBUTTONDBLCLK = 0x0203;
            public const int WM_NCLBUTTONDBLCLK = 0x00A3;

            // Right Mouse Button
            public const int WM_RBUTTONDOWN = 0x0204;
            public const int WM_NCRBUTTONDOWN = 0x00A4;
            public const int WM_RBUTTONUP = 0x0205;
            public const int WM_NCRBUTTONUP = 0x00A5;
            public const int WM_RBUTTONDBLCLK = 0x0206;
            public const int WM_NCRBUTTONDBLCLK = 0x00A6;

            // Middle Mouse Button
            public const int WM_MBUTTONDOWN = 0x0207;
            public const int WM_NCMBUTTONDOWN = 0x00A7;
            public const int WM_MBUTTONUP = 0x0208;
            public const int WM_NCMBUTTONUP = 0x00A8;
            public const int WM_MBUTTONDBLCLK = 0x0209;
            public const int WM_NCMBUTTONDBLCLK = 0x00A9;

            // Keyboard Messages
            public const int WM_KEYDOWN = 0x0100;
            public const int WM_KEYUP = 0x0101;
            public const int WM_CHAR = 0x0102;

            // Window Messages
            public const int WM_CLOSE = 0x0010;
            public const int WM_DESTROY = 0x0002;
            public const int WM_SIZE = 0x0005;
            public const int WM_MOVE = 0x0003;

            // Control Messages
            public const int WM_COMMAND = 0x0111;
            public const int WM_NOTIFY = 0x004E;
            public const int WM_PAINT = 0x000F;
        }
    }
}
