using ImGuiNET;
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
        private static Vector2 _start;
        /// <summary>
        /// The button used for making a selection.
        /// </summary>
        public const ImGuiMouseButton mouse_button = ImGuiMouseButton.Left;
        
        public static bool Rectangle(ref Rectangle rect)
        {
            // If true, we need to wait for the initial release of the button. 
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
                _start = ImGui.GetMousePos();
            }
            // We are dragging and received a release event.
            else if (clickStarted && ImGui.IsMouseReleased(mouse_button))
            {
                // Reset all the local variables and states
                clickStarted = false;
                clickCapturing = false;
                var _end = ImGui.GetMousePos();
                ImGui.GetIO().WantCaptureMouse = false;
                // Apply the values to the rectangle
                rect.X = (int)_start.X;
                rect.Y = (int)_start.Y;
                rect.Width = (int)(_end.X - _start.X + 1);
                rect.Height = (int)(_end.Y - _start.Y + 1);
                _start = default;
                // Notify completion
                return true;
            }
            // We are dragging the cursor awaiting a release
            if (clickStarted && ImGui.IsMouseDown(mouse_button))
                DrawRectangles(_start, ImGui.GetMousePos());

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

        private static void DrawRectangles(Vector2 start_pos, Vector2 end_pos)
        {
            ImDrawListPtr draw_list = ImGui.GetForegroundDrawList();
            draw_list.AddRect(start_pos, end_pos, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 130, 216, 255)));
            draw_list.AddRectFilled(start_pos, end_pos, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 130, 216, 50)));
        }
    }
}
