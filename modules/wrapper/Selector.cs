using ClickableTransparentOverlay.Win32;
using ImGuiNET;
using System;
using System.Drawing;
using System.Numerics;

namespace Triggered.modules.wrapper
{

    /// <summary>
    /// Helper class for selecting a rectangle with the mouse.
    /// </summary>
    public static class Selector
    {
        private static bool clickStarted = false;
        private static bool clickCapturing = false;
        private static User32.POINT _start;
        /// <summary>
        /// The button used for making a selection.
        /// </summary>
        public const ImGuiMouseButton mouse_button = ImGuiMouseButton.Left;
        
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
            }
            User32.POINT mousePos = new();
            User32.GetCursorPos(out mousePos);
            // We are dragging the cursor awaiting a release
            if (clickStarted && mouseDown)
                DrawRectangles(_start, mousePos);
            // We are starting a click event while the bool is false.
            else if (!clickStarted && mouseDown)
            {
                clickStarted = true;
                _start = mousePos;
            }
            // We are dragging and received a release event.
            else if (clickStarted && !mouseDown)
            {
                // Reset all the local variables and states
                clickStarted = false;
                clickCapturing = false;
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
            if (!clickCapturing && ImGui.IsMouseDown(mouse_button))
                return false;
            // The button is released and we can begin input blocking.
            if (!clickCapturing)
            {
                clickCapturing = true;
                ImGui.GetIO().WantCaptureMouse = true;
            }
            // We are starting a click event while the bool is false.
            if (!clickStarted && ImGui.IsMouseClicked(mouse_button))
            {
                clickStarted = true;
                point = ImGui.GetMousePos();
            }
            if (clickStarted && ImGui.IsMouseReleased(mouse_button))
            { 
                clickCapturing = false;
                ImGui.GetIO().WantCaptureMouse = false;
                return true; 
            }
            return false;
        }

        private static void DrawRectangles(User32.POINT start_pos, User32.POINT end_pos)
        {
            Vector2 start = new(start_pos.X,start_pos.Y);
            Vector2 end = new(end_pos.X,end_pos.Y);
            ImDrawListPtr draw_list = ImGui.GetForegroundDrawList();
            draw_list.AddRect(start, end, 0xFFD88240);
            draw_list.AddRectFilled(start, end, 0x42D88240);
        }
    }
}
