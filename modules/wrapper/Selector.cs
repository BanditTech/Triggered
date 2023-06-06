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
                var point = new Point(_start.X, _start.Y);
                var hWnd = WindowFromPoint(point);
                ScreenToClient(hWnd, ref _start);
                ScreenToClient(hWnd, ref mousePos);
                point = new Point(_start.X, _start.Y);
                GetWindowRect(hWnd, out var rect);
                target.Start = CalculateCoordinate(point, rect.Rectangle, anchor);
                point = new Point(mousePos.X, mousePos.Y);
                target.End = CalculateCoordinate(point, rect.Rectangle, anchor);
                _start = default;
                // Notify completion
                return true;
            }
            return false;
        }

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

        private static void DrawRectangles(POINT start_pos, POINT end_pos)
        {
            ImDrawListPtr draw_list = ImGui.GetForegroundDrawList();
            draw_list.AddRect(start_pos.Vector2, end_pos.Vector2, 0xFFD88280);
            draw_list.AddRectFilled(start_pos.Vector2, end_pos.Vector2, 0x42D88280);
        }

        public static class InputBlocker
        {
            // Define the mouse hook ID and callback function
            private static IntPtr hookHandle;

            // Install the mouse hook and block the next mouse click
            public static void NextClick()
            {
                IntPtr moduleHandle = Kernel32.GetModuleHandle(null);
                hookHandle = SetWindowsHookEx(HookType.WH_MOUSE_LL, MouseHookProc, moduleHandle, 0);
            }

            // Mouse hook procedure to block mouse clicks
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
                        // Unhook the mouse hook
                        UnhookWindowsHookEx(hookHandle);
                    }
                    return new IntPtr(1);
                }
                return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            }
        }

    }
}
