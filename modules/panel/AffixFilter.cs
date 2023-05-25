using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Triggered.modules.panel
{
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

        public static bool DrawTextBox(ref string input)
        {
            bool isInputTextEnterPressed = ImGui.InputText("##input", ref input, 32, ImGuiInputTextFlags.EnterReturnsTrue);
            var min = ImGui.GetItemRectMin();
            var size = ImGui.GetItemRectSize();
            bool isInputTextActivated = ImGui.IsItemActivated();
            bool popupCompleted = false;

            if (isInputTextActivated)
            {
                ImGui.SetNextWindowPos(new Vector2(min.X, min.Y));
                ImGui.OpenPopup("##popup");
            }

            if (ImGui.BeginPopup("##popup", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
            {
                if (isInputTextActivated)
                    ImGui.SetKeyboardFocusHere(0);
                ImGui.InputText("##input_popup", ref input, 32);
                ImGui.SameLine();
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
                ImGui.BeginChild("scrolling_region", new Vector2(size.X * 2, size.Y * 10), false, ImGuiWindowFlags.HorizontalScrollbar);
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
