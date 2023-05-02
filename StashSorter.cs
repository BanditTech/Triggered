using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Triggered
{
    static class StashSorter
    {
        static string _dragTarget;
        static Type _dragTargetType;
        static string _dragSource;
        static Type _dragSourceType;
        static bool _dragStarted;
        static bool _dragFinalize;
        static bool _editWindowOpen = false;
        static Element _rightClickedElement = null;
        static Group _rightClickedGroup = null;

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
            ImGui.SetNextWindowSizeConstraints(new Vector2(500, 200), new Vector2(float.MaxValue, float.MaxValue));
            ImGui.Begin("Edit Stash Sorter");

            // Select the TopGroup
            ImGui.Combo("Selected Filter", ref App.SelectedGroup, App.TopGroups, App.TopGroups.Length);

            // We recurse the Group structure drawing them onto our menu
            RecursiveMenu(App.StashSorterList[App.SelectedGroup], "NONE");

            // End the main window
            ImGui.End();

            // finalize the drag action while the collection is not being looped
            if (_dragFinalize)
            {
                _dragStarted = false;
                _dragFinalize = false;
                App.Log($"Source: {_dragSource} Target: {_dragTarget}");
                App.Log($"After making adjustment\n" +
                    $"Source: {StripIndexerElement(_dragSourceType,_dragSource)} Target: {StripIndexerElement(_dragTargetType,_dragTarget)}");
                object fetch = GetObjectByIndexer(_dragSource, _dragSourceType, true);
                InsertObjectByIndexer(_dragTarget,_dragTargetType,_dragSourceType,fetch);
                App.Log($"{fetch} {JSON.Str(fetch)}");
            }
            if (_dragStarted && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                _dragStarted = false;
        }
        static void RecursiveMenu(IGroupElement obj,string parentType,string indexer = "0")
        {
            if (obj == null)
                return;
            bool IsWeighted = parentType == "COUNT" || parentType == "WEIGHT";
            if (obj is Group group)
            {
                ImGui.PushID(group.GetHashCode());
                string str = IsWeighted ? $"{group.Weight}# {group.GroupType} Group" : $"{group.GroupType} Group";
                str += group.GroupType == "COUNT" ? $" with minimum of {group.Min} match value"
                    : group.GroupType == "WEIGHT" ? $" with minimum of {group.Min} weight"
                    : "";
                bool isNodeOpen = ImGui.TreeNodeEx(str, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick);
                if (ImGui.BeginDragDropTarget())
                {
                    _dragTarget = indexer;
                    _dragTargetType = typeof(Group);
                    if (CanDrop() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        _dragFinalize = true;
                    ImGui.EndDragDropTarget();
                }
                if (group is not TopGroup && ImGui.BeginDragDropSource())
                {
                    _dragStarted = true;
                    _dragSource = indexer;
                    _dragSourceType = typeof(Group);
                    ImGui.SetDragDropPayload("COMPONENT",IntPtr.Zero,0);
                    ImGui.Text($"{indexer}");
                    ImGui.EndDragDropSource();
                }
                ImGui.PopID();
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    if (_rightClickedGroup == group)
                        _rightClickedGroup = null;
                    else
                        _rightClickedGroup = group;
                }
                if (_rightClickedGroup != null && _rightClickedGroup == group)
                    EditGroup(parentType);

                if (isNodeOpen)
                {
                    int i = -1;
                    foreach (Element subElement in group.ElementList)
                    {
                        i++;
                        RecursiveMenu(subElement,group.GroupType,$"{indexer}_{i}");
                    }
                    i = -1;
                    foreach (Group subGroup in group.GroupList)
                    {
                        i++;
                        RecursiveMenu(subGroup,group.GroupType,$"{indexer}_{i}");
                    }
                    ImGui.TreePop();
                }
            }
            else if (obj is Element leaf)
            {
                ImGui.PushID(leaf.GetHashCode());
                string str = IsWeighted ? $"{leaf.Weight}# " : "";
                str += $"{leaf.Key} {leaf.Eval} {leaf.Min}";
                ImGui.Selectable(str);
                if (ImGui.BeginDragDropTarget())
                {
                    _dragTarget = indexer;
                    _dragTargetType = typeof(Element);
                    if (CanDrop() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                        _dragFinalize = true;
                    ImGui.EndDragDropTarget();
                }
                if (ImGui.BeginDragDropSource())
                {
                    _dragStarted = true;
                    _dragSource = indexer;
                    _dragSourceType = typeof(Element);
                    ImGui.SetDragDropPayload("COMPONENT", IntPtr.Zero, 0);
                    ImGui.Text($"{indexer}");
                    ImGui.EndDragDropSource();
                }
                ImGui.PopID();
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    if (_rightClickedElement == leaf)
                        _rightClickedElement = null;
                    else
                        _rightClickedElement = leaf;
                }
                if (_rightClickedElement != null && _rightClickedElement == leaf)
                    EditElement(parentType);
            }
            else
                App.Log("This should not display",NLog.LogLevel.Error);
        }

        #region Movement Helpers
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
        static bool IsChildObject(string sourceIndexer, string targetIndexer)
        {
            string[] sourceKeys = sourceIndexer.Split('_');
            string[] targetKeys = targetIndexer.Split('_');

            // Check if the target object is a child of the source object
            for (int i = 1; i < sourceKeys.Length; i++)
            {
                // If the source and target have different group keys at the same level, they are not related
                if (targetKeys.Length <= i || sourceKeys[i] != targetKeys[i])
                {
                    return false;
                }
            }

            // If the target has more group keys than the source, it is a child of the source
            return targetKeys.Length > sourceKeys.Length;
        }
        static string StripIndexerElement(Type type, string indexer)
        {
            if (type == typeof(Element))
                return indexer.Substring(0, indexer.Length - 2);
            return indexer;
        }
        static bool CanDrop()
        {
            string source = StripIndexerElement(_dragSourceType,_dragSource);
            string target = StripIndexerElement(_dragTargetType,_dragTarget);

            if (_dragSourceType == typeof(Element))
                return true;

            return source != target && _dragStarted && !IsChildObject(source, target);
        }
        #endregion

        #region Edit Popups
        static Vector4 EditingHighlight = new Vector4(0.0f, 0.5f, 0.9f, 0.3f);
        static Vector4 EditingBackground = new Vector4(0.0f, 0.0f, 0.8f, 0.2f);
        static void EditElement(string parentType)
        {
            float availableSpace = ImGui.GetContentRegionAvail().X - ImGui.GetTreeNodeToLabelSpacing();

            if (_rightClickedElement != null)
            {
                ImGui.BeginGroup();
                Vector2 padding = new Vector2(5.0f, 2.0f);
                // Draw an indicator on active Element
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(
                    ImGui.GetItemRectMin(),
                    ImGui.GetItemRectMax(),
                    ImGui.GetColorU32(EditingHighlight),
                    4.0f);

                if (parentType == "COUNT" || parentType == "WEIGHT")
                {
                    // int Weight
                    ImGui.Text("Weight:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(72);
                    ImGui.InputInt("##Weight", ref _rightClickedElement.Weight);
                    ImGui.SameLine();
                }
                // string Key
                ImGui.Text("Key:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(availableSpace);
                ImGui.InputText("##Key", ref _rightClickedElement.Key, 256);

                // string Eval
                ImGui.Text("Eval:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(72);
                int comparisonIndex = Array.IndexOf(App.EvalOptions, _rightClickedElement.Eval);
                if (ImGui.Combo("##Eval", ref comparisonIndex, App.EvalOptions, App.EvalOptions.Length))
                    _rightClickedElement.Eval = App.EvalOptions[comparisonIndex];
                // string Min
                ImGui.SameLine();
                ImGui.Text("Min:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(availableSpace);
                ImGui.InputText("##Min", ref _rightClickedElement.Min, 256);

                ImGui.EndGroup();
                // Draw an indicator on active Element
                var boxList = ImGui.GetWindowDrawList();
                boxList.AddRectFilled(
                    ImGui.GetItemRectMin() - padding,
                    ImGui.GetItemRectMax() + padding,
                    ImGui.GetColorU32(EditingBackground),
                    4.0f);
            }
        }
        static void EditGroup(string parentType)
        {
            float availableSpace = ImGui.GetContentRegionAvail().X - ImGui.GetTreeNodeToLabelSpacing();
            float other = (availableSpace) * 0.2f;

            if (_rightClickedGroup != null)
            {
                ImGui.BeginGroup();
                // Draw an indicator on active Group
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(
                    ImGui.GetItemRectMin(),
                    ImGui.GetItemRectMax(),
                    ImGui.GetColorU32(EditingHighlight),
                    4.0f);

                if (_rightClickedGroup is TopGroup topGroup)
                {
                    // int GroupName
                    ImGui.Text("GroupName:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(availableSpace * 0.85f);
                    ImGui.InputText("##GroupName", ref topGroup.GroupName, 256);
                    // int Min
                    ImGui.Text("Stash Tab:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(72);
                    ImGui.InputInt("##StashTab", ref topGroup.StashTab);
                    if (topGroup.StashTab > 999)
                        topGroup.StashTab = 999;
                    else if (topGroup.StashTab < 0)
                        topGroup.StashTab = 0;
                    // int Strictness
                    ImGui.SameLine();
                    ImGui.Text("Strictness:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(72);
                    ImGui.InputInt("##Strictness", ref topGroup.Strictness);
                    if (topGroup.Strictness > 99)
                        topGroup.Strictness = 99;
                    else if (topGroup.Strictness < 0)
                        topGroup.Strictness = 0;
                    ImGui.SameLine();
                }

                // string Type
                ImGui.SetNextItemWidth(72);
                int comparisonIndex = Array.IndexOf(App.GroupTypes, _rightClickedGroup.GroupType);
                if (ImGui.Combo("##GroupType", ref comparisonIndex, App.GroupTypes, App.GroupTypes.Length))
                    _rightClickedGroup.GroupType = App.GroupTypes[comparisonIndex];
                if (_rightClickedGroup.GroupType == "COUNT" || _rightClickedGroup.GroupType == "WEIGHT")
                {
                    // int Min
                    ImGui.SameLine();
                    ImGui.Text("Min:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(72);
                    ImGui.InputInt("##Min", ref _rightClickedGroup.Min);
                }
                if (parentType == "COUNT" || parentType == "WEIGHT")
                {
                    // int Weight
                    ImGui.SameLine();
                    ImGui.Text("Weight:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(72);
                    ImGui.InputInt("##Weight", ref _rightClickedGroup.Weight);
                }

                ImGui.EndGroup();
                var boxList = ImGui.GetWindowDrawList();
                boxList.AddRectFilled(
                    ImGui.GetItemRectMin(),
                    ImGui.GetItemRectMax(),
                    ImGui.GetColorU32(EditingBackground),
                    4.0f);
            }
        }
        #endregion
    }
}
