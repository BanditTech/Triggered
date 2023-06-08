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
        /// <param name="percentage">The position on the screen.</param>
        public static void CenteredColorText(string text, float percentage = 0f)
        {
            CenteredColorText(defaultColor, text, percentage);
        }

        /// <summary>
        /// Displays centered colored text using the specified color.
        /// </summary>
        /// <param name="color">The color of the text.</param>
        /// <param name="text">The text to be displayed.</param>
        /// <param name="percentage">The position on the screen.</param>
        public static void CenteredColorText(Vector4 color, string text, float percentage = 0f)
        {
            // Assign the default of .5 of the available space
            if (percentage == 0f)
                percentage = .5f;

            // Calculate the width of the text
            Vector2 textSize = ImGui.CalcTextSize(text);

            // Calculate the position to center the text
            var windowWidth = ImGui.GetContentRegionAvail().X;
            var calcMiddle = (windowWidth - textSize.X) * percentage;
            // Display the text
            ImGui.Indent(calcMiddle);
            ImGui.TextColored(color, text);
            ImGui.Unindent(calcMiddle);
        }

        /// <summary>
        /// Displays centered text using ImGui.
        /// </summary>
        /// <param name="text">The text to be displayed.</param>
        /// <param name="percentage">The position on the screen.</param>
        public static void CenteredText(string text,float percentage = 0f)
        {
            // Assign the default of half of the available space
            if (percentage == 0f)
                percentage = .5f;

            // Calculate the width of the text
            Vector2 textSize = ImGui.CalcTextSize(text);

            // Calculate the position to center the text
            var windowWidth = ImGui.GetContentRegionAvail().X;
            var calcMiddle = (windowWidth - textSize.X) * percentage;
            // Display the text
            ImGui.Indent(calcMiddle);
            ImGui.Text(text);
            ImGui.Unindent(calcMiddle);
        }

        /// <summary>
        /// Inserts spacing between ImGui elements.
        /// </summary>
        /// <param name="count">The number of spacers to insert.</param>
        public static void Spacers(int count)
        {
            for (int i = 0; i < count; i++)
                ImGui.Spacing();
        }

        /// <summary>
        /// Inserts spacing between ImGui elements.
        /// </summary>
        /// <param name="count">The number of spacers to insert.</param>
        public static void SameLineSpacers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ImGui.SameLine();
                ImGui.Spacing();
            }
            ImGui.SameLine();
        }

        /// <summary>
        /// Creates a new section in the ImGui interface with optional separators and spacers.
        /// </summary>
        /// <param name="count">The number of spacers to insert before and after the section.</param>
        /// <param name="separate">Specifies whether to include a separator between the spacers.</param>
        public static void NewSection(int count = 2, bool separate = true)
        {
            Spacers(count);
            if (separate)
                ImGui.Separator();
            Spacers(count);
        }

    }
}
