using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Triggered
{
    public class DropdownBoxUtility
    {
        private static string input = string.Empty;
        private static List<string> filteredItems = new List<string>();
        private static string[] listNames = { "List 1", "List 2", "List 3" };
        private static int selectedListIndex = 0;

        public static void DrawDropdownBox()
        {
            bool isInputTextEnterPressed = ImGui.InputText("##input", ref input, 32, ImGuiInputTextFlags.EnterReturnsTrue);
            var min = ImGui.GetItemRectMin();
            var size = ImGui.GetItemRectSize();
            bool isInputTextActivated = ImGui.IsItemActivated();

            if (isInputTextActivated)
            {
                ImGui.SetNextWindowPos(new Vector2(min.X,min.Y));
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
                    var parts = input.Split(" ",StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string str in selectedItems)
                    {
                        bool allPartsMatch = true;
                        foreach (string part in parts)
                        {
                            if (!str.Contains(part))
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
            // Add logic to retrieve items based on the selected list index
            switch (index)
            {
                case 0:
                    return new string[] { "cats", "dogs", "rabbits", "turtles" };
                case 1:
                    return new string[] { "apples", "oranges", "bananas", "grapes" };
                case 2:
                    return new string[] { "carrots", "broccoli", "peas", "corn" };
                default:
                    return new string[0];
            }
        }
    }
}
