using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ImGuiNET;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace Triggered
{
    public class ExampleAppLog
    {
        private const int BufSize = 2 * 1024;
        private readonly byte[] buffer = new byte[BufSize];
        private readonly List<List<(string text, string color)>> items = new List<List<(string text, string color)>>();
        private readonly object locker = new object();

        public bool AutoScroll { get; set; } = true;
        public int MaxLines { get; set; } = 1000;

        private readonly Dictionary<string, string> colorMap = new Dictionary<string, string>
        {
            // Primary colors
            {"red", "#FF0000"},
            {"green", "#00FF00"},
            {"blue", "#0000FF"},
    
            // Secondary colors
            {"orange", "#FFA500"},
            {"purple", "#800080"},
            {"greenish", "#00FF7F"},
            {"pink", "#FF69B4"},
            {"orange-red", "#FF4500"},
            {"turquoise", "#40E0D0"},
    
            // Neutrals
            {"black", "#000000"},
            {"white", "#FFFFFF"},
            {"gray", "#808080"},
            {"beige", "#F5F5DC"},
            {"silver", "#C0C0C0"},
    
            // Other colors
            {"yellow", "#FFFF00"},
            {"teal", "#008080"},
            {"navy", "#000080"},
            {"maroon", "#800000"},
            {"olive", "#808000"},
            {"lime", "#00FF00"},
            {"aqua", "#00FFFF"},
            {"indigo", "#4B0082"},
            {"coral", "#FF7F50"},
            {"crimson", "#DC143C"},
            {"chocolate", "#D2691E"},
            {"orchid", "#DA70D6"},
            {"plum", "#DDA0DD"},
            {"khaki", "#F0E68C"},
            {"salmon", "#FA8072"}
        };
        private readonly Regex colorRegex = new Regex(@"\[(c|color)=(red|green|blue|black|white|orange|purple|gold|brown|yellow|teal|navy|maroon|olive|lime|aqua|indigo|coral|crimson|chocolate|orchid|plum|khaki|salmon)\](.*?)\[\/(c|color)\]");
        public void Clear()
        {
            lock (locker)
            {
                items.Clear();
            }
        }

        public void AddLog(string fmt, params object[] args)
        {
            var log = string.Format(fmt, args);

            lock (locker)
            {
                var sentence = new List<(string text, string color)>();
                var matches = colorRegex.Matches(log);
                if (matches.Count > 0)
                {
                    var lastEndIndex = 0;
                    foreach (Match match in matches)
                    {
                        var color = match.Groups[2].Value.ToLowerInvariant();
                        var text = match.Groups[3].Value;

                        if (colorMap.TryGetValue(color, out var hexColor))
                        {
                            var startIndex = match.Index - lastEndIndex;
                            var endIndex = startIndex + text.Length;
                            sentence.Add((log.Substring(lastEndIndex, startIndex), null));
                            sentence.Add((text, hexColor));
                            lastEndIndex = match.Index + match.Length;
                        }
                    }
                    sentence.Add((log.Substring(lastEndIndex), null));
                }
                else
                {
                    sentence.Add((log, null));
                }
                items.Add(sentence);
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

            ImGui.Separator();
            ImGui.BeginChild("scrolling", new System.Numerics.Vector2(0, 0), false, ImGuiWindowFlags.HorizontalScrollbar);

            lock (locker)
            {
                foreach (var sentence in items)
                {
                    foreach (var (text, color) in sentence)
                    {
                        ImGui.SameLine();
                        var vecColor = ParseColor(color);
                        if (color != null)
                        {
                            ImGui.TextColored(vecColor, text);
                        }
                        else
                        {
                            ImGui.TextUnformatted(text);
                        }
                    }
                    ImGui.TextUnformatted("\n");
                }
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
                return new Vector4(0f, 0f, 0f, 1f);
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
            else if (color.Length == 6)
            {
                // Parse hex color code like RRGGBB
                var r = Convert.ToInt32(color.Substring(0, 2), 16);
                var g = Convert.ToInt32(color.Substring(2, 2), 16);
                var b = Convert.ToInt32(color.Substring(4, 2), 16);
                return new Vector4(r / 255f, g / 255f, b / 255f, 1f);
            }
            else
            {
                // Default to black
                return new Vector4(0f, 0f, 0f, 1f);
            }
        }
    }
}
