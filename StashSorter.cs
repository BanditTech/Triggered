using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Triggered
{
    static class StashSorter
    {
        #region Setup Functions
        static bool firstRun = true;
        static StashSorter()
        {
            DumpExampleJson();
            UpdateStashSorterFile();
            UpdateTopGroups();
        }
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

            if (firstRun)
                firstRun = false;
            // End the main window
            ImGui.End();
        }
        static Element FindElementById(Group parentGroup, int id)
        {
            foreach (Element element in parentGroup.ElementList)
            {
                if (element.GetHashCode() == id)
                {
                    return element;
                }
            }

            foreach (Group group in parentGroup.GroupList)
            {
                Element foundElement = FindElementById(group, id);
                if (foundElement != null)
                {
                    return foundElement;
                }
            }

            return null;
        }
        static Group FindGroupById(Group parentGroup, int id)
        {
            foreach (Group group in parentGroup.GroupList)
            {
                if (group.GetHashCode() == id)
                {
                    return group;
                }
                else
                {
                    Group foundGroup = FindGroupById(group, id);
                    if (foundGroup != null)
                    {
                        return foundGroup;
                    }
                }
            }
            return null;
        }

        static void RecursiveMenu(IGroupElement obj,string indexer = "0")
        {
            if (obj == null)
                return;
            if (obj is Group group)
            {
                bool isNodeOpen = ImGui.TreeNodeEx($"{group.GroupType} {group.Min}", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick);
                if (group is not TopGroup && ImGui.BeginDragDropSource())
                {
                    //idk? ImGui.SetDragDropPayload()
                    ImGui.Text($"{group.GroupType} {group.Min}");
                    ImGui.EndDragDropSource();
                }
                if (ImGui.BeginDragDropTarget())
                {
                    ImGuiPayloadPtr payload = ImGui.GetDragDropPayload();
                    ImGui.EndDragDropTarget();
                }
                if (isNodeOpen)
                {
                    int i = 0;
                    foreach (Element subElement in group.ElementList)
                    {
                        i++;
                        RecursiveMenu(subElement,$"{indexer}_{i}");
                    }
                    i = 0;
                    foreach (Group subGroup in group.GroupList)
                    {
                        i++;
                        RecursiveMenu(subGroup,$"{indexer}_{i}");
                    }
                    ImGui.TreePop();
                }
            }
            else if (obj is Element leaf)
            {
                bool isNodeOpen = ImGui.TreeNodeEx($"Key: {leaf.Key}, Eval: {leaf.Eval}, Min: {leaf.Min}, Weight: {leaf.Weight}", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick);
                if (ImGui.BeginDragDropSource())
                {
                    //idk? ImGui.SetDragDropPayload()
                    ImGui.Text($"Key: {leaf.Key}, Eval: {leaf.Eval}, Min: {leaf.Min}, Weight: {leaf.Weight}");
                    ImGui.EndDragDropSource();
                }
                if (ImGui.BeginDragDropTarget())
                {
                    ImGuiPayloadPtr payload = ImGui.GetDragDropPayload();
                    ImGui.EndDragDropTarget();
                }
                if (isNodeOpen)
                {
                    ImGui.TreePop();
                }
            }
            else
            {
                App.Log("This should not display",NLog.LogLevel.Error);
            }
        }
    }
}
