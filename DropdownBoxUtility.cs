using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Triggered
{
    public class DropdownBoxUtility
    {
        private static string input = string.Empty;
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

        public static void DrawDropdownBox()
        {
            bool isInputTextEnterPressed = ImGui.InputText("##input", ref input, 32, ImGuiInputTextFlags.EnterReturnsTrue);
            var min = ImGui.GetItemRectMin();
            var size = ImGui.GetItemRectSize();
            bool isInputTextActivated = ImGui.IsItemActivated();

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
                            if (!str.Contains(part,StringComparison.OrdinalIgnoreCase))
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
                        break;
                    }
                }
                ImGui.EndChild();

                if (isInputTextEnterPressed || ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.Escape)))
                {
                    App.Log("Closing popup");
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }

        private static string[] GetSelectedListItems(int index)
        {
            // Load JSON data if not already loaded
            if (jsonData == null)
                LoadJsonData();

            return jsonData["result"][index]["entries"]
                .Select(entry => entry["text"]?.ToString())
                .ToArray();
        }

        private static void LoadJsonData()
        {
            // Load the JSON file into a string
            string jsonString = File.ReadAllText("stats.json");

            // Parse the JSON data into a JObject
            jsonData = JObject.Parse(jsonString);
        }
    }
}
