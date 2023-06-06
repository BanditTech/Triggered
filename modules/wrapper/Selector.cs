using ClickableTransparentOverlay.Win32;
using ImGuiNET;
using System;
using System.Drawing;
using System.Numerics;
using static Triggered.modules.wrapper.PointScaler;
using static Triggered.modules.wrapper.User32;

namespace Triggered.modules.wrapper
{

    /// <summary>
    /// Helper class for selecting a rectangle with the mouse.
    /// </summary>
    public static class Selector
    {
        private static bool clickCapturing = false;
        private static POINT _start;
        private static bool _dragging = false;
        private static bool _release = false;

        /// <summary>
        /// The button used for making a selection.
        /// </summary>
        public const ImGuiMouseButton mouse_button = ImGuiMouseButton.Left;

        /// <summary>
        /// Allow the user to Select a rectangle.
        /// This rectangle will be converted to relative position.
        /// This relative position is then converted into a Coordinate.
        /// Start and End Coordinates are then joined to produce the ScaledRectangle
        /// </summary>
        /// <param name="target"></param>
        /// <param name="anchor"></param>
        /// <returns>True when complete, false otherwise</returns>
        public static bool ScaledRectangle(ref ScaledRectangle target, AnchorPosition anchor)
        {
            // If true, we need to wait for the initial release of the button. 
            bool mouseDown = Utils.IsKeyPressed(VK.LBUTTON);
            if (!clickCapturing && mouseDown)
                return false;

            // The button is released and we can begin input blocking.
            if (!clickCapturing)
            {
                clickCapturing = true;
                InputBlocker.NextClick();
            }

            // We can return if we are awaiting our first click.
            if (clickCapturing && !_dragging && !_release)
                return false;

            GetCursorPos(out var mousePos);
            // We are dragging the cursor awaiting a release
            if (_dragging)
                DrawRectangles(_start, mousePos);
            // We received a release event.
            else if (_release)
            {
                // Reset all the local variables and states
                clickCapturing = false;
                _release = false;
                // Apply the values to the rectangle
                var hWnd = WindowFromPoint(_start.Point);
                ScreenToClient(hWnd, ref _start);
                ScreenToClient(hWnd, ref mousePos);
                GetWindowRect(hWnd, out var rect);
                target.Start = CalculateCoordinate(_start.Point, rect.Rectangle, anchor);
                target.End = CalculateCoordinate(mousePos.Point, rect.Rectangle, anchor);
                _start = default;
                // Notify completion
                return true;
            }
            return false;
        }

        /// <summary>
        /// Allow the user to Select a Point.
        /// This point is then converted into a Coordinate.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="anchor"></param>
        /// <returns>True when complete, false otherwise</returns>
        public static bool Coordinate(ref Coordinate coord, AnchorPosition anchor)
        {
            // If true, we need to wait for the initial release of the button. 
            bool mouseDown = Utils.IsKeyPressed(VK.LBUTTON);
            if (!clickCapturing && mouseDown)
                return false;

            // The button is released and we can begin input blocking.
            if (!clickCapturing)
            {
                clickCapturing = true;
                InputBlocker.NextClick();
            }
            // We can return if we are awaiting our first click.
            if (_release)
            {
                // Reset all the local variables and states
                clickCapturing = false;
                _release = false;
                // Apply the values to the rectangle
                var point = new Point(_start.X, _start.Y);
                var hWnd = WindowFromPoint(point);
                ScreenToClient(hWnd, ref _start);
                point = new Point(_start.X, _start.Y);
                GetWindowRect(hWnd,out var rect);
                coord = CalculateCoordinate(point,rect.Rectangle,anchor);
                _start = default;
                // Notify completion
                return true;
            }
            return false;
        }

        /// <summary>
        /// Allow the user to Select a Rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>True when complete, false otherwise</returns>
        public static bool Rectangle(ref Rectangle rect)
        {
            // If true, we need to wait for the initial release of the button. 
            bool mouseDown = Utils.IsKeyPressed(VK.LBUTTON);
            if (!clickCapturing && mouseDown)
                return false;

            // The button is released and we can begin input blocking.
            if (!clickCapturing)
            {
                clickCapturing = true;
                InputBlocker.NextClick();
            }
            // We can return if we are awaiting our first click.
            if (clickCapturing && !_dragging && !_release)
                return false;

            GetCursorPos(out var mousePos);
            // We are dragging the cursor awaiting a release
            if (_dragging)
                DrawRectangles(_start, mousePos);
            // We received a release event.
            else if (_release)
            {
                // Reset all the local variables and states
                clickCapturing = false;
                _release = false;
                // Apply the values to the rectangle
                rect.X = Math.Min(_start.X, mousePos.X);
                rect.Y = Math.Min(_start.Y, mousePos.Y);
                rect.Width = Math.Abs(mousePos.X - _start.X)+1;
                rect.Height =  Math.Abs(mousePos.Y - _start.Y)+1;
                _start = default;
                // Notify completion
                return true;
            }
            return false;
        }

        /// <summary>
        /// Allow the user to Select a Point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>True when complete, false otherwise</returns>
        public static bool Point(ref Vector2 point)
        {
            // If we have not begun the click capture and the mouse is already down, we need to wait for the initial release. 
            if (!clickCapturing && Utils.IsKeyPressed(VK.LBUTTON))
                return false;
            // The button is released and we can begin input blocking.
            if (!clickCapturing)
            {
                clickCapturing = true;
                InputBlocker.NextClick();
            }

            if (_release)
            { 
                clickCapturing = false;
                _release = false;
                point.X = _start.X;
                point.Y = _start.Y;
                return true; 
            }
            return false;
        }

        /// <summary>
        /// Draw two overlapping rectangles.
        /// One makes the outline, while the other is the body.
        /// </summary>
        /// <param name="start_pos"></param>
        /// <param name="end_pos"></param>
        private static void DrawRectangles(POINT start_pos, POINT end_pos)
        {
            ImDrawListPtr draw_list = ImGui.GetForegroundDrawList();
            draw_list.AddRect(start_pos.Vector2, end_pos.Vector2, 0xFFD88280);
            draw_list.AddRectFilled(start_pos.Vector2, end_pos.Vector2, 0x42D88280);
        }

        /// <summary>
        /// Blocks the user input for its next action.
        /// The next click and release will be captured.
        /// The captured event fires our callback logic.
        /// </summary>
        public static class InputBlocker
        {
            /// <summary>
            /// Defines the handle to the hook.
            /// We save this for removing it later.
            /// </summary>
            private static IntPtr hookHandle;

            /// <summary>
            /// Installs a mouse hook at a low level.
            /// This will filter messages related to Left button up and down.
            /// On the click Up event we release the hook.
            /// </summary>
            public static void NextClick()
            {
                IntPtr moduleHandle = Kernel32.GetModuleHandle(null);
                hookHandle = SetWindowsHookEx(HookType.WH_MOUSE_LL, MouseHookProc, moduleHandle, 0);
            }

            /// <summary>
            /// Mouse hook procedure that is called when a mouse event occurs.
            /// </summary>
            /// <param name="nCode">The hook code. If nCode is less than 0, the procedure must pass the message to the CallNextHookEx function without further processing and return the value returned by CallNextHookEx.</param>
            /// <param name="wParam">The identifier of the mouse message. It can be one of the following values: MessageType.WM_LBUTTONDOWN, MessageType.WM_LBUTTONUP.</param>
            /// <param name="lParam">A pointer to an MSLLHOOKSTRUCT structure that contains information about the mouse event.</param>
            /// <returns>
            /// An IntPtr value. If nCode is less than 0, the hook procedure must return the value returned by CallNextHookEx.
            /// If nCode is greater than or equal to 0 and the hook procedure did not process the message, it is recommended that you call CallNextHookEx and return the value it returns.
            /// </returns>
            private static IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && (wParam == MessageType.WM_LBUTTONDOWN || wParam == MessageType.WM_LBUTTONUP))
                {
                    if (wParam == MessageType.WM_LBUTTONDOWN)
                    {
                        _dragging = true;
                        GetCursorPos(out var mousePos);
                        _start = mousePos;
                    }
                    else if (wParam == MessageType.WM_LBUTTONUP)
                    {
                        _release = true;
                        _dragging = false;
                        // Removes our hook from the stack.
                        UnhookWindowsHookEx(hookHandle);
                    }
                    return new IntPtr(1);
                }
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }
        }

    }
}
