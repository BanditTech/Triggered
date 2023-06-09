using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using NLog;
using Triggered.modules.options;

namespace Triggered.modules.panel
{
    /// <summary>
    /// Creates a log window in ImGui
    /// </summary>
    public class LogWindow
    {
        private readonly List<(string text, Vector4 color)> items = new List<(string text, Vector4 color)>();
        private readonly object locker = new object();
        private static Options_Panel Panel => App.Options.Panel;
        private static Options_Log Opts => App.Options.Log;
        private static bool panelOpen = true;
        private readonly Dictionary<string, string> colorMap = new Dictionary<string, string>
        {
            // LogLevel Color Assignments
            {"trace", "#808080"},
            {"debug", "#1010FF"},
            {"info", "#FFFFFF"},
            {"warn", "#FFA500"},
            {"error", "#FF0000"},
            {"fatal", "#8B0000"},
        };

        /// <summary>
        /// Wipes the item list to display a fresh window.
        /// </summary>
        public void Clear()
        {
            lock (locker)
            {
                items.Clear();
            }
        }

        /// <summary>
        /// Adds an entry to the list of log events which are displayed.
        /// It adds the tuple of text/color to the item list.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level"></param>
        public void AddLog(string log, LogLevel level)
        {
            var levelStr = level.ToString().ToLowerInvariant();
            Vector4 color = ParseColor(levelStr);

            lock (locker)
            {
                items.Add((log, color));
            }

            var maxLines = Opts.GetKey<int>("WindowMaxLines");
            if (items.Count > maxLines)
            {
                items.RemoveRange(0, items.Count - maxLines);
            }
        }

        /// <summary>
        /// Create the log window with the specified title name.
        /// </summary>
        /// <param name="title"></param>
        public void Draw(string title)
        {
            if (!Panel.GetKey<bool>("LogWindow"))
                return;
            if (!panelOpen)
            {
                panelOpen = true;
                Panel.SetKey("LogWindow",false);
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(400, 200), ImGuiCond.FirstUseEver);
            // Adjust window background transparency
            float transparency = Opts.GetKey<float>("Transparency");
            ImGui.PushStyleColor(ImGuiCol.Border,
                new Vector4(ImGui.GetStyle().Colors[(int)ImGuiCol.Border].X,
                            ImGui.GetStyle().Colors[(int)ImGuiCol.Border].Y,
                            ImGui.GetStyle().Colors[(int)ImGuiCol.Border].Z,
                            transparency));
            ImGui.PushStyleColor(ImGuiCol.WindowBg,
                new Vector4(ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg].X,
                            ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg].Y,
                            ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg].Z,
                            transparency));
            var open = ImGui.Begin(title, ref panelOpen);
            ImGui.PopStyleColor(2);
            if (!open)
            {
                ImGui.End();
                return;
            }
            // We start a scroll area to contain the text data
            ImGui.BeginChild("scrolling", new Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);
            // In order to avoid async data issues, we make a locked duplicate
            List<(string text, Vector4 color)> displayItems;
            lock (locker)
            {
                displayItems = items.ToList();
            }
            // Draw the text in its specified color
            foreach (var (text, color) in displayItems)
                ImGui.TextColored(color, text);
            // Determine if we should scroll down the window.
            if (Opts.GetKey<bool>("ScrollToBottom") && !ImGui.IsWindowHovered())
                ImGui.SetScrollHereY(1.0f);
            // We end the scroll area
            ImGui.EndChild();
            // We end the window
            ImGui.End();
        }

        /// <summary>
        /// Simple helper function which parses the text name and returns a color.
        /// A better implimentation might have allowed for custom fields.
        /// The colorMap is only filled with LogLevel names.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private Vector4 ParseColor(string color)
        {
            if (color == null)
            {
                return new Vector4(1f, 1f, 1f, 1f);
            }
            else if (colorMap.TryGetValue(color.ToLowerInvariant(), out var hexColor))
            {
                // Parse color word
                return ParseColor(hexColor);
            }
            else if (color.StartsWith("#") && color.Length == 7)
            {
                // Parse hex color code like #RRGGBB
                var r = Convert.ToInt32(color.Substring(1, 2), 16);
                var g = Convert.ToInt32(color.Substring(3, 2), 16);
                var b = Convert.ToInt32(color.Substring(5, 2), 16);
                return new Vector4(r / 255f, g / 255f, b / 255f, 1f);
            }
            else
            {
                // Default to white
                return new Vector4(1f, 1f, 1f, 1f);
            }
        }
    }
}
