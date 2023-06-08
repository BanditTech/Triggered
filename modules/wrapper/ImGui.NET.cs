using ImGuiNET;
using System;
using System.Numerics;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Utility methods for ImGui integration in C#.
    /// </summary>
    public static class ImGuiNet
    {
        private static Vector4 defaultColor = new Vector4(0.5f, 0.5f, 0.5f, 1f);

        /// <summary>
        /// Displays centered colored text using the default color.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        public static void CenteredColorText(string text)
        {
            CenteredColorText(defaultColor, text);
        }

        /// <summary>
        /// Displays centered colored text using the specified color.
        /// </summary>
        /// <param name="color">The color of the text.</param>
        /// <param name="text">The text to be displayed.</param>
        public static void CenteredColorText(Vector4 color, string text)
        {
            // Calculate the width of the text
            Vector2 textSize = ImGui.CalcTextSize(text);

            // Calculate the position to center the text
            var windowWidth = ImGui.GetContentRegionAvail().X;
            var calcMiddle = (windowWidth - textSize.X) * 0.5f;
            // Display the text
            ImGui.Indent(calcMiddle);
            ImGui.TextColored(color, text);
            ImGui.Unindent(calcMiddle);
        }

        /// <summary>
        /// Displays centered text using ImGui.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        public static void CenteredText(string text)
        {
            // Calculate the width of the text
            Vector2 textSize = ImGui.CalcTextSize(text);

            // Calculate the position to center the text
            var windowWidth = ImGui.GetContentRegionAvail().X;
            var calcMiddle = (windowWidth - textSize.X) * 0.5f;
            // Display the text
            ImGui.Indent(calcMiddle);
            ImGui.Text(text);
            ImGui.Unindent(calcMiddle);
        }
    }
}
