using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using NLog;

namespace Triggered
{
    public class ExampleAppLog
    {
        private readonly List<(string text, Vector4 color)> items = new List<(string text, Vector4 color)>();
        private readonly object locker = new object();
        public static LogLevel[] logLevels = { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal };
        public static string[] logLevelNames = { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" };
        public static int logLevelIndex = 0;
        public bool AutoScroll { get; set; } = true;
        public int MaxLines { get; set; } = 1000;

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
        public void Clear()
        {
            lock (locker)
            {
                items.Clear();
            }
        }
        public void AddLog(string log, LogLevel level)
        {
            var levelStr = level.ToString().ToLowerInvariant();
            Vector4 color = ParseColor(levelStr);

            lock (locker)
            {
                items.Add((log, color));
            }

            if (items.Count > MaxLines)
            {
                items.RemoveRange(0, items.Count - MaxLines);
            }
        }
        public void Draw(string title, bool autoScroll)
        {
            if (!ImGui.Begin(title))
            {
                ImGui.End();
                return;
            }

            if (ImGui.Button("Clear"))
            {
                Clear();
            }
            ImGui.SameLine();
            var copy = ImGui.Button("Copy");
            ImGui.SameLine();
            var shouldAutoScroll = AutoScroll;
            ImGui.Checkbox("Auto-scroll", ref shouldAutoScroll);
            AutoScroll = shouldAutoScroll;
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            if (ImGui.Combo(">= Level", ref App.SelectedLogLevelIndex, logLevelNames, logLevels.Length))
            {
                LogLevel selectedLogLevel = logLevels[App.SelectedLogLevelIndex];
                App.Log($"Changing minimum log level to {selectedLogLevel}", LogLevel.Fatal);
                App.LogWindowMinimumLogLevel = selectedLogLevel;
            }

            ImGui.Separator();
            ImGui.BeginChild("scrolling", new System.Numerics.Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);

            List<(string text, Vector4 color)> displayItems;

            lock (locker)
            {
                displayItems = items.ToList();
            }

            foreach (var (text, color) in displayItems)
            {
                ImGui.TextColored(color, text);
            }

            if (autoScroll && AutoScroll)
            {
                ImGui.SetScrollHereY(1.0f);
            }
            ImGui.EndChild();
            ImGui.End();
        }
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
