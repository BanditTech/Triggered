using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Triggered.modules.panel
{
    /// <summary>
    /// Draw a textbox which allows for a popup menu when clicked
    /// </summary>
    public class AffixFilter
    {
        private static List<string> filteredItems = new List<string>();
        private static string[] listNames = {
            "Pseudo",
            "Explicit",
            "Implicit",
            "Fractured",
            "Enchant",
            "Scourge",
            "Crafted",
            "Crucible",
            "Veiled",
            "Monster",
            "Delve",
            "Ultimatum",
        };
        private static int selectedListIndex = 0;
        private static JObject jsonData; // JObject to store the parsed JSON data

        /// <summary>
        /// Primary function of the class, draws the textbox which can be clicked on.
        /// When clicked, the popup list allows for selection of the filtered options.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool DrawTextBox(ref string input)
        {
            float availableSpace = ImGui.GetContentRegionAvail().X - ImGui.GetTreeNodeToLabelSpacing();
            bool isInputTextEnterPressed = ImGui.InputText("##input", ref input, 256, ImGuiInputTextFlags.EnterReturnsTrue);
            var min = ImGui.GetItemRectMin();
            var size = ImGui.GetItemRectSize();
            if (size.X > availableSpace)
                size.X = availableSpace;
            bool isInputTextActivated = ImGui.IsItemActivated();
            bool popupCompleted = false;

            if (isInputTextActivated)
            {
                ImGui.SetNextWindowPos(new Vector2(min.X - 8, min.Y - 8));
                ImGui.OpenPopup("##popup");
            }

            if (ImGui.BeginPopup("##popup", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
            {
                if (isInputTextActivated)
                    ImGui.SetKeyboardFocusHere(0);
                ImGui.SetNextItemWidth(size.X - 100);
                ImGui.InputText("##input_popup", ref input, 256);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                ImGui.Combo("##listCombo", ref selectedListIndex, listNames, listNames.Length);
                filteredItems.Clear();
                // Select items based on the selected list index
                string[] selectedItems = GetSelectedListItems(selectedListIndex);

                if (string.IsNullOrEmpty(input))
                    foreach (string item in selectedItems)
                        filteredItems.Add(item);
                else
                {
                    var parts = input.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string str in selectedItems)
                    {
                        bool allPartsMatch = true;
                        foreach (string part in parts)
                        {
                            if (!str.Contains(part, StringComparison.OrdinalIgnoreCase))
                            {
                                allPartsMatch = false;
                                break;
                            }
                        }
                        if (allPartsMatch)
                            filteredItems.Add(str);
                    }
                }
                ImGui.BeginChild("scrolling_region", new Vector2(size.X, size.Y * 10), false, ImGuiWindowFlags.HorizontalScrollbar);
                foreach (string item in filteredItems)
                {
                    if (ImGui.Selectable(item))
                    {
                        App.Log("Selected popup");
                        input = item;
                        ImGui.CloseCurrentPopup();
                        popupCompleted = true;
                        break;
                    }
                }
                ImGui.EndChild();

                if (isInputTextEnterPressed || ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Escape)))
                {
                    App.Log("Closing popup");
                    ImGui.CloseCurrentPopup();
                    popupCompleted = true;
                }

                ImGui.EndPopup();
            }
            return popupCompleted;
        }

        private static string[] GetSelectedListItems(int index)
        {
            if (jsonData == null)
                LoadJsonData();

            List<string> mergedItems = new List<string>();

            jsonData["result"][index]["entries"]
                .Select(entry =>
                {
                    string text = entry["text"].ToString();

                    if (entry["option"] != null)
                    {
                        JArray options = (JArray)entry["option"]["options"];
                        if (options != null && options.Count > 0)
                        {
                            if (text.Contains("#"))
                            {
                                return options
                                    .Select(option => text.Replace("#", option["text"].ToString()))
                                    .ToList();
                            }
                            else if (text.StartsWith("Grants Summon Harbinger Skill"))
                            {
                                return options
                                    .Select(option => $"Grants Summon {option["text"]} Skill")
                                    .ToList();
                            }
                            else
                            {
                                return new List<string> { text }
                                    .Concat(options.Select(option => option["text"].ToString()))
                                    .ToList();
                            }
                        }
                    }
                    return new List<string> { text };
                })
                .ToList()
                .ForEach(items => mergedItems.AddRange(items));

            return mergedItems.ToArray();
        }

        private static void LoadJsonData()
        {
            // Build our string to locate the file
            string path = Path.Combine(AppContext.BaseDirectory, "data", "PathofExile", "stats.json");
            // Load the JSON file into a string
            string jsonString = File.ReadAllText(path);

            // Parse the JSON data into a JObject
            jsonData = JObject.Parse(jsonString);
        }
    }
}
