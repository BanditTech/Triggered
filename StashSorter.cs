using ImGuiNET;
using Newtonsoft.Json.Linq;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Triggered
{
    static class StashSorter
    {
        static string _dragTarget;
        static Type _dragTargetType;
        static string _dragSource;
        static Type _dragSourceType;
        static bool _dragFinalize;
        #region Setup Functions
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
            examplegroup.AddElement(exampleelement);
            examplegroup.AddElement(exampleelement);

            Group demoGroup = new Group();
            demoGroup.AddGroup(examplegroup);
            demoGroup.AddElement(exampleelement);

            TopGroup example1 = new TopGroup("Example 1 AND", "AND", default, default, default);
            example1.AddElement(exampleelement);
            example1.AddElement(exampleelement);
            example1.AddGroup(demoGroup);
            example1.AddGroup(examplegroup);
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
            ImGui.Begin("Edit Stash Sorter");

            // Select the TopGroup
            ImGui.Combo("Selected Filter", ref App.SelectedGroup, App.TopGroups, App.TopGroups.Length);

            // We recurse the Group structure drawing them onto our menu
            RecursiveMenu(App.StashSorterList[App.SelectedGroup]);

            // End the main window
            ImGui.End();

            // finalize the drag action while the collection is not being looped
            if (_dragFinalize)
            {
                _dragFinalize = false;
                object fetch = GetObjectByIndexer(_dragSource, _dragSourceType, true);
                InsertObjectByIndexer(_dragTarget,_dragTargetType,_dragSourceType,fetch);
                App.Log($"{fetch} {JSON.Str(fetch)}");
            }
        }
        static void RecursiveMenu(IGroupElement obj,string indexer = "0")
        {
            if (obj == null)
                return;
            if (obj is Group group)
            {
                ImGui.PushID(group.GetHashCode());
                bool isNodeOpen = ImGui.TreeNodeEx($"{group.GroupType} {group.Min}", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick);
                if (ImGui.BeginDragDropTarget())
                {
                    _dragTarget = indexer;
                    _dragTargetType = typeof(Group);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        _dragFinalize = true;
                    ImGui.EndDragDropTarget();
                }
                if (group is not TopGroup && ImGui.BeginDragDropSource())
                {
                    _dragSource = indexer;
                    _dragSourceType = typeof(Group);
                    ImGui.SetDragDropPayload("COMPONENT",IntPtr.Zero,0);
                    ImGui.Text($"{indexer}");
                    ImGui.EndDragDropSource();
                }
                ImGui.PopID();
                if (isNodeOpen)
                {
                    int i = -1;
                    foreach (Element subElement in group.ElementList)
                    {
                        i++;
                        RecursiveMenu(subElement,$"{indexer}_{i}");
                    }
                    i = -1;
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
                ImGui.PushID(leaf.GetHashCode());
                bool isNodeOpen = ImGui.TreeNodeEx($"Key: {leaf.Key}, Eval: {leaf.Eval}, Min: {leaf.Min}, Weight: {leaf.Weight}", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick);
                if (ImGui.BeginDragDropTarget())
                {
                    _dragTarget = indexer;
                    _dragTargetType = typeof(Element);
                    if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        _dragFinalize = true;
                    ImGui.EndDragDropTarget();
                }
                if (ImGui.BeginDragDropSource())
                {
                    _dragSource = indexer;
                    _dragSourceType = typeof(Element);
                    ImGui.SetDragDropPayload("COMPONENT", IntPtr.Zero, 0);
                    ImGui.Text($"{indexer}");
                    ImGui.EndDragDropSource();
                }
                ImGui.PopID();
                if (isNodeOpen)
                {
                    ImGui.TreePop();
                }
            }
            else
                App.Log("This should not display",NLog.LogLevel.Error);
        }
        static object GetObjectByIndexer(string indexer, Type type, bool pop = false)
        {
            if (indexer == "0")
                throw new ArgumentException("TopGroup should never be a drag source.");

            string[] indices = indexer.Split('_');
            int length = indices.Length;
            object target = App.StashSorterList[App.SelectedGroup];
            object parent;

            // Iterate over the index keys, skipping the first one (which is always 0)
            for (int i = 1; i < length; i++)
            {
                int index = int.Parse(indices[i]);

                // If we have not reached the end of the list, its a group
                if (i == length - 1)
                {
                    if (type == typeof(Element))
                    {
                        // final indexer digit is in ElementList array
                        parent = target;
                        target = ((Group)target).ElementList[index];
                        if (pop)
                            ((Group)parent).ElementList.RemoveAt(index);
                    }
                    else if (type == typeof(Group))
                    {
                        // final indexer digit is in GroupList array
                        parent = target;
                        target = ((Group)target).GroupList[index];
                        if (pop)
                            ((Group)parent).GroupList.RemoveAt(index);
                    }
                    else
                        throw new ArgumentException($"Expecting GROUP or ELEMENT type but received {type}");
                }
                else
                    target = ((dynamic)target).GroupList[index];
            }
            if (type == typeof(Group))
                return (Group)target;
            else if (type == typeof(Element))
                return (Element)target;
            return target;
        }
        static void InsertObjectByIndexer(string indexer, Type targetType, Type sourceType, object obj)
        {
            if (indexer == "0")
            {
                if (sourceType == typeof(Group))
                    ((TopGroup)App.StashSorterList[App.SelectedGroup]).Add((Group)obj);
                else if (sourceType == typeof(Element))
                    ((TopGroup)App.StashSorterList[App.SelectedGroup]).Add((Element)obj);
                return;
            }

            string[] indices = indexer.Split('_');
            int length = indices.Length;
            Group target = (TopGroup)App.StashSorterList[App.SelectedGroup];

            // Iterate over the index keys, skipping the first one (which is always 0)
            for (int i = 1; i < length; i++)
            {
                int key = int.Parse(indices[i]);

                // If we are not at the last key, it is always a group
                if (i == length - 1)
                {
                    // If the last target key is a group, we finish drilling down
                    if (targetType == typeof(Group))
                    {
                        target = (target).GroupList[key];
                        key = 0;
                    }
                    // Depending on our source type, we insert the correct object type
                    if (sourceType == typeof(Element))
                        target.Insert(key,(Element)obj);
                    else if (sourceType == typeof(Group))
                        target.Insert(key,(Group)obj);
                }
                else
                    target = ((dynamic)target).GroupList[key];
            }
        }
    }
}
