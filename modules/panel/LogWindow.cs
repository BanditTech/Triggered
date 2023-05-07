using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using NLog;

namespace Triggered.modules.panels
{
    /// <summary>
    /// Creates a log window in ImGui
    /// </summary>
    public class LogWindow
    {
        private readonly List<(string text, Vector4 color)> items = new List<(string text, Vector4 color)>();
        private readonly object locker = new object();
        private static readonly string[] logLevelNames = { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" };
        private static bool _autoscroll;
        private static int _maxlines;
        private static int _minLevel;
        private readonly Dictionary<string, string> colorMap = new Dictionary<string, string>
        {
            // LogLevel Color Assignments
            {"trace", "#808080"},
            {"debug", "#000080"},
            {"info", "#FFFFFF"},
            {"warn", "#FFA500"},
            {"error", "#FF0000"},
            {"fatal", "#8B0000"},
        };
        /// <summary>
        /// Instantiate the values of our local reference.
        /// </summary>
        public LogWindow()
        {
            var options = App.Options.MainMenu;
            _autoscroll = options.GetKey<bool>("LogAutoScroll");
            _maxlines = options.GetKey<int>("LogMaxLines");
            _minLevel = options.GetKey<int>("SelectedLogLevelIndex");
        }
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

            if (items.Count > _maxlines)
            {
                items.RemoveRange(0, items.Count - _maxlines);
            }
        }
        /// <summary>
        /// Create the log window with the specified title name.
        /// </summary>
        /// <param name="title"></param>
        public void Draw(string title)
        {
            if (!ImGui.Begin(title))
            {
                ImGui.End();
                return;
            }
            // reference the options
            var options = App.Options.MainMenu;
            // Clear Text Button
            if (ImGui.Button("Clear"))
                Clear();
            ImGui.SameLine();
            // non-functional Copy button
            if (ImGui.Button("Copy"))
                App.Log("yep, still doesnt work");
            ImGui.SameLine();
            // Autoscroll option
            if (ImGui.Checkbox("Auto-scroll", ref _autoscroll))
                options.SetKey("LogAutoScroll", _autoscroll);
            ImGui.SameLine();
            // Minimum log level
            ImGui.SetNextItemWidth(100);
            if (ImGui.Combo(">= Level", ref _minLevel, logLevelNames, logLevelNames.Length))
            {
                options.SetKey("SelectedLogLevelIndex", _minLevel);
                App.Log($"Changing minimum log level to {logLevelNames[_minLevel]}", LogLevel.Fatal);
            }
            ImGui.Separator();
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
            if (_autoscroll && !ImGui.IsWindowHovered())
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
