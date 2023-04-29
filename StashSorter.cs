using ImGuiNET;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Triggered
{
    static class StashSorter
    {
        static StashSorter()
        {
            DumpExampleJson();
            UpdateStashSorterFile();
            UpdateTopGroups();
        }
        #region Setup Functions
        static void DumpExampleJson()
        {
            Group examplegroup = new Group();
            Element exampleelement = new Element();
            examplegroup.AddElement(exampleelement);

            TopGroup example1 = new TopGroup("Example 1 AND", "AND", default, default, default);
            example1.AddElement(exampleelement);
            example1.AddElement(exampleelement);
            example1.AddGroup(examplegroup);
            //TopGroup example2 = new TopGroup("Example 2 NOT", "NOT", default, default, default);
            //example2.AddElement(exampleelement);
            //TopGroup example3 = new TopGroup("Example 3 COUNT", "COUNT", default, default, default);
            //example3.AddElement(exampleelement);
            //TopGroup example4 = new TopGroup("Test Duplicate", "WEIGHT", default, default, default);
            //example4.AddElement(exampleelement);
            //TopGroup example5 = new TopGroup("Test Duplicate", "WEIGHT", default, default, default);
            //example5.AddElement(exampleelement);
            List<TopGroup> dumpthis = new List<TopGroup>();
            dumpthis.Add(example1);
            //dumpthis.Add(example2);
            //dumpthis.Add(example3);
            //dumpthis.Add(example4);
            //dumpthis.Add(example5);

            string jsonString = JSON.Str(dumpthis);
            File.WriteAllText("example.json", jsonString);
        }
        static void UpdateStashSorterFile()
        {
            // Load the JSON file into a string
            string jsonString = File.ReadAllText("example.json");
            // Deserialize the JSON into a list of IGroupElement objects
            App.StashSorterList = JSON.IGroupElementList(jsonString);
        }
        static void UpdateTopGroups()
        {
            // Fetch the GroupName of each TopGroup and save to a string array in App.TopGroups
            List<string> topGroupsList = new List<string>();
            foreach (IGroupElement group in App.StashSorterList)
            {
                if (group is TopGroup topGroup)
                {
                    topGroupsList.Add(topGroup.GroupName);
                }
            }
            App.TopGroups = topGroupsList.ToArray();
        }
        #endregion
        public static void Render()
        {
            // Create the main window
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 500), ImGuiCond.FirstUseEver);
            ImGui.Begin("Edit TopGroup");

            // Select the TopGroup
            ImGui.Combo("Top Group", ref App.SelectedGroup, App.TopGroups, App.TopGroups.Length);

            // We recurse the Group structure drawing them onto our menu
            RecursiveMenu(App.StashSorterList[App.SelectedGroup]);

            // End the main window
            ImGui.End();
        }
        static void RecursiveMenu(IGroupElement obj)
        {
            if (obj == null)
                return;

            if (obj is Group group)
            {
                ImGui.PushID(group.GetHashCode());
                if (ImGui.TreeNodeEx(group.GroupType, ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick))
                {
                    ImGui.PopID();
                    // Allow editing of Group values
                    ImGui.PushID(group.GetHashCode() + 1);
                    ImGui.InputText("GroupType", ref group.GroupType, 255);
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.PushID(group.GetHashCode() + 2);
                    ImGui.InputInt("Min", ref group.Min);
                    ImGui.PopID();

                    foreach (Element subElement in group.ElementList)
                    {
                        RecursiveMenu(subElement);
                    }
                    foreach (Group subGroup in group.GroupList)
                    {
                        RecursiveMenu(subGroup);
                    }

                    ImGui.TreePop();
                }
                ImGui.PopID();
            }
            else if (obj is Element leaf)
            {
                ImGui.PushID(leaf.GetHashCode());
                if (ImGui.TreeNodeEx($"Key: {leaf.Key}, Eval: {leaf.Eval}, Min: {leaf.Min}, Weight: {leaf.Weight}", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick))
                {
                    ImGui.PopID();
                    ImGui.PushID(leaf.GetHashCode() + 1);
                    ImGui.InputText("Key", ref leaf.Key, 255);
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.PushID(leaf.GetHashCode() + 2);
                    ImGui.InputText("Eval", ref leaf.Eval, 255);
                    ImGui.PopID();
                    ImGui.PushID(leaf.GetHashCode() + 3);
                    ImGui.InputText("Min", ref leaf.Min, 255);
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.PushID(leaf.GetHashCode() + 4);
                    ImGui.InputInt("Weight", ref leaf.Weight);
                    ImGui.PopID();
                    ImGui.TreePop();
                }
                ImGui.PopID();
            }
            else
                App.Log("This should not display");
        }
    }
}
