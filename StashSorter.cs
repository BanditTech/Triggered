using ImGuiNET;
using System.Collections.Generic;
using System.IO;

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
            example1.AddElement(examplegroup);
            example1.AddElement(exampleelement);
            TopGroup example2 = new TopGroup("Example 2 NOT", "NOT", default, default, default);
            example2.AddElement(exampleelement);
            TopGroup example3 = new TopGroup("Example 3 COUNT", "COUNT", default, default, default);
            example3.AddElement(exampleelement);
            TopGroup example4 = new TopGroup("Test Duplicate", "WEIGHT", default, default, default);
            example4.AddElement(exampleelement);
            TopGroup example5 = new TopGroup("Test Duplicate", "WEIGHT", default, default, default);
            example5.AddElement(exampleelement);
            List<TopGroup> dumpthis = new List<TopGroup>();
            dumpthis.Add(example1);
            dumpthis.Add(example2);
            dumpthis.Add(example3);
            dumpthis.Add(example4);
            dumpthis.Add(example5);

            string jsonString = JSON.Min(dumpthis);
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

            // The main program loop would reside here
            // We need a recursive function to draw the groups and elements
            RecursiveMenu(App.StashSorterList);

            // End the main window
            ImGui.End();
        }
        static void RecursiveMenu(object obj)
        {
            if (obj == null)
                return;

            if (obj is TopGroup topGroup)
            {
                // Draw the TopGroup fields
                ImGui.InputText("Group Name", ref topGroup.GroupName, 256);
                ImGui.InputInt("Stash Tab", ref topGroup.StashTab);
                ImGui.InputInt("Strictness", ref topGroup.Strictness);

                // Draw the element list as a drag and drop interface
                ImGui.Text("Element List:");
                if (ImGui.BeginDragDropTarget())
                {
                    if (ImGui.IsDragDropPayloadBeingAccepted())
                    {
                        // Handle the dropped element
                        IGroupElement element = *(IGroupElement*)ImGui.GetDragDropPayload().Data;
                        if (element is Element)
                        {
                            topGroup.ElementList.Add(element);
                        }
                        else if (element is Group)
                        {
                            topGroup.ElementList.Add(element);
                        }
                    }
                    ImGui.EndDragDropTarget();
                }
                foreach (IGroupElement element in topGroup.ElementList)
                {
                    RecursiveMenu(element);
                }
            }
            else if (obj is Group)
            {
                // Draw the Group fields
                Group group = (Group)obj;
                ImGui.InputText("Group Type", ref group.GroupType, 256);
                ImGui.InputInt("Min", ref group.Min);

                // Draw the element list as a drag and drop interface
                ImGui.Text("Element List:");
                if (ImGui.BeginDragDropTarget())
                {
                    if (ImGui.IsDragDropPayloadBeingAccepted())
                    {
                        // Handle the dropped element
                        IGroupElement element = *(IGroupElement*)ImGui.GetDragDropPayload().Data;
                        if (element is Element)
                        {
                            group.ElementList.Add(element);
                        }
                        else if (element is Group)
                        {
                            group.ElementList.Add(element);
                        }
                    }
                    ImGui.EndDragDropTarget();
                }
                foreach (IGroupElement element in group.ElementList)
                {
                    RecursiveMenu(element);
                }
            }
            else if (obj is Element)
            {
                // Draw the Element fields
                Element element = (Element)obj;
                ImGui.InputText("Key", ref element.Key, 256);
                ImGui.InputText("Eval", ref element.Eval, 256);
                ImGui.InputText("Min", ref element.Min, 256);
                ImGui.InputInt("Weight", ref element.Weight);
            }
        }
    }
}
