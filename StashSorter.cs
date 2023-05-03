using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json.Linq;

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
        static bool confirmRemove = false;
        static bool confirmAdd = false;
        static Type removeType;
        static string removeIndexer;
        static string addIndexer;
        static Vector4 EditingHighlight = new Vector4(0f, 0.772549f, 1f, 0.392157f); // #00C5FF64
        static Vector4 EditingBackground = new Vector4(0.0f, 1f, 0.9254902f, 0.1647059f); // #00FFEC2A
        static Vector4 RemoveButton = new Vector4(1.0f, 0.0f, 0.0f, 0.2f);
        static Vector4 AddButton = new Vector4(0.0f, 1.0f, 0.0f, 0.2f);
        static bool _showOptions = true;
        static Element _hoveredLeaf = null;
        static Group _hoveredGroup = null;
        static float _lineHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2f;
        static DateTime _lastHover = DateTime.MinValue;
        static JObject keyValuePairs = new JObject();
        static Type _addingType = typeof(Element);
        static Type _oldType = typeof(Element);
        static IGroupElement _clay;


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
                    topGroupsList.Add(topGroup.GroupName);
            }
            App.TopGroups = topGroupsList.ToArray();
        }
        #endregion

        public static void Render()
        {
            // Create the main window
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(500, 200), new Vector2(float.MaxValue, float.MaxValue));
            ImGui.Begin("Edit Stash Sorter");

            // Select the TopGroup
            ImGui.Text("Selected Filter:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.Combo("##Selected Filter", ref App.SelectedGroup, App.TopGroups, App.TopGroups.Length);

            if (App.StashSorterList[App.SelectedGroup] is TopGroup topGroup)
            {
                ImGui.Spacing();
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
                // int GroupName
                ImGui.Text("GroupName:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                ImGui.InputText("##GroupName", ref topGroup.GroupName, 256);
                ImGui.SameLine();
            }


            // We recurse the Group structure drawing them onto our menu
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.Spacing();
            RecursiveMenu(App.StashSorterList[App.SelectedGroup], "NONE");

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Separator();

            if (ImGui.CollapsingHeader("Editor color adjustment", ref _showOptions))
            {
                float space = ImGui.GetContentRegionAvail().X * 0.4f;
                ImGui.SetNextItemWidth(space);
                ImGui.ColorPicker4("##Highlight", ref EditingHighlight);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(space);
                ImGui.ColorPicker4("##Background", ref EditingBackground);
            }

            // End the main window
            ImGui.End();

            // finalize the drag action while the collection is not being looped
            if (_dragFinalize)
            {
                _dragStarted = false;
                _dragFinalize = false;
                object fetch = GetObjectByIndexer(_dragSource, _dragSourceType, true);
                InsertObjectByIndexer(_dragTarget,_dragTargetType,_dragSourceType,fetch);
                App.Log($"{fetch} {JSON.Str(fetch)}");
            }
            if (confirmRemove)
            {
                confirmRemove = false;
                _ = GetObjectByIndexer(removeIndexer, removeType, true);
            }
            if (_dragStarted && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                _dragStarted = false;
        }
        static void RecursiveMenu(IGroupElement obj,string parentType,string indexer = "0")
        {
            bool _hovered;
            if (obj == null)
                return;
            bool IsWeighted = parentType == "COUNT" || parentType == "WEIGHT";
            if (obj is Group group)
            {
                ImGui.PushID(group.GetHashCode());
                string str = IsWeighted ? $"{group.Weight}# {group.GroupType} Group" : $"{group.GroupType} Group";
                str += group.GroupType == "COUNT" ? $" with minimum of {group.Min} match value"
                    : group.GroupType == "WEIGHT" ? $" with minimum of {group.Min} weight" : "";
                ImGui.Spacing();
                // We want to exclude the topgroup from making a remove button
                if (group is not TopGroup)
                {
                    if (_hoveredGroup == group)
                    {
                        // Delete Button
                        ImGui.PushStyleColor(ImGuiCol.Button, RemoveButton);
                        if (ImGui.Button("X"))
                        {
                            removeType = typeof(Group);
                            removeIndexer = indexer;
                            ImGui.OpenPopup("DeleteItem");
                        }
                        ImGui.PopStyleColor(1);
                        _hovered = ImGui.IsItemHovered();
                        if (_hovered && _hoveredGroup == group)
                            _lastHover = DateTime.Now;
                        ImGui.SameLine();
                    }
                }
                // But any group should show the add button
                if (_hoveredGroup == group)
                {
                    // Add Button
                    ImGui.PushStyleColor(ImGuiCol.Button, AddButton);
                    if (ImGui.Button("+"))
                    {
                        addIndexer = indexer;
                        ImGui.OpenPopup("AddItem");
                    }
                    ImGui.PopStyleColor(1);
                    _hovered = ImGui.IsItemHovered();
                    if (_hovered && _hoveredGroup == group)
                        _lastHover = DateTime.Now;
                    ImGui.SameLine();
                }

                if (ImGui.BeginPopupModal("DeleteItem"))
                {
                    ImGui.Text("Are you sure you want to delete this item?");
                    if (ImGui.Button("Yes", new Vector2(120, 0)))
                    {
                        // delete the item
                        confirmRemove = true;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No", new Vector2(120, 0)))
                    {
                        removeIndexer = null;
                        removeType = null;
                        confirmRemove = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
                if (ImGui.BeginPopupModal("AddItem"))
                {
                    int clayType = Array.IndexOf(App.ObjectTypes, _addingType);
                    if (ImGui.Combo("##Eval", ref clayType, App.objectTypes, App.objectTypes.Length))
                    {
                        _addingType = App.ObjectTypes[clayType];
                    }
                    bool changed = _oldType != _addingType;
                    if (_clay == null || changed)
                    {
                        if (_addingType == typeof(Group))
                        {
                            _clay = new Group();
                        }
                        else if (_addingType == typeof(Element))
                        {
                            _clay = new Element();
                        }
                        _oldType = _addingType;
                        App.Log("Swap logic occuring");
                    }

                    if (_clay is Element _element)
                    {
                        ImGui.PushID(_clay.GetHashCode());
                        // string Key
                        ImGui.Text("Key:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        ImGui.InputText("##Key", ref _element.Key, 256);
                        ImGui.PopID();

                        // We only want to display the Weight field if the parent requires it
                        if (parentType == "COUNT" || parentType == "WEIGHT")
                        {
                            ImGui.PushID(_clay.GetHashCode()+1);
                            // int Weight
                            ImGui.Text("Weight:");
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(72);
                            ImGui.InputInt("##Weight", ref _element.Weight);
                            ImGui.SameLine();
                            ImGui.PopID();
                        }
                        // string Eval
                        ImGui.PushID(_clay.GetHashCode() + 2);
                        ImGui.Text("Eval:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(72);
                        int comparisonIndex = Array.IndexOf(App.EvalOptions, _element.Eval);
                        if (ImGui.Combo("##Eval", ref comparisonIndex, App.EvalOptions, App.EvalOptions.Length))
                            _element.Eval = App.EvalOptions[comparisonIndex];
                        ImGui.PopID();

                        // string Min
                        ImGui.PushID(_clay.GetHashCode() + 3);
                        ImGui.SameLine();
                        ImGui.Text("Min:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        ImGui.InputText("##Min", ref _element.Min, 256);
                        ImGui.PopID();
                    }

                    ImGui.Text($"Are you ready to insert the new {App.objectTypes[clayType]}?");
                    if (ImGui.Button("Yes", new Vector2(120, 0)))
                    {
                        // Add the item
                        confirmAdd = true;
                        if (_clay is Group)
                            group.Add(((Group)_clay).Clone());
                        else if (_clay is Element)
                            group.Add(((Element)_clay).Clone());
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No", new Vector2(120, 0)))
                    {
                        addIndexer = null;
                        confirmAdd = false;
                        _clay = null;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }


                bool isNodeOpen = ImGui.TreeNodeEx(str, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick);

                _hovered = ImGui.IsItemHovered();
                if (_hovered && _hoveredGroup != group)
                {
                    _lastHover = DateTime.Now;
                    _hoveredGroup = group;
                }
                else if (_hovered && _hoveredGroup == group)
                    _lastHover = DateTime.Now;
                else if (!ImGui.IsAnyItemHovered() && _hoveredGroup == group && (DateTime.Now - _lastHover).TotalSeconds > 1)
                    _hoveredGroup = null;
                else if (!_hovered && _hoveredGroup == group && _hoveredLeaf != null)
                    _hoveredGroup = null;


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

                ImGui.Spacing();
                if (_hoveredLeaf == leaf)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, RemoveButton);
                    // allow for deletion of an Element
                    if (ImGui.Button("X"))
                    {
                        removeType = typeof(Element);
                        removeIndexer = indexer;
                        ImGui.OpenPopup("DeleteItem");
                    }
                    ImGui.PopStyleColor(1);
                    _hovered = ImGui.IsItemHovered();
                    if (_hovered && _hoveredLeaf == leaf)
                        _lastHover = DateTime.Now;
                    ImGui.SameLine();
                }
                if (ImGui.BeginPopupModal("DeleteItem"))
                {
                    ImGui.Text("Are you sure you want to delete this item?");
                    if (ImGui.Button("Yes", new Vector2(120, 0)))
                    {
                        // delete the item
                        confirmRemove = true;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No", new Vector2(120, 0)))
                    {
                        removeIndexer = null;
                        removeType = null;
                        confirmRemove = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }

                ImGui.Selectable(str);

                _hovered = ImGui.IsItemHovered();
                if (_hovered && _hoveredLeaf != leaf)
                {
                    _lastHover = DateTime.Now;
                    _hoveredLeaf = leaf;
                }
                else if (_hovered && _hoveredLeaf == leaf)
                    _lastHover = DateTime.Now;
                else if (!ImGui.IsAnyItemHovered() && _hoveredLeaf == leaf && (DateTime.Now - _lastHover).TotalSeconds > 1)
                    _hoveredLeaf = null;
                else if (!_hovered && _hoveredLeaf == leaf && _hoveredGroup != null)
                    _hoveredLeaf = null;
                
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

        #region Field Editing Popups
        static void EditElement(string parentType)
        {
            float availableSpace = ImGui.GetContentRegionAvail().X - ImGui.GetTreeNodeToLabelSpacing();

            if (_rightClickedElement != null)
            {
                ImGui.BeginGroup();
                Vector2 padding = new Vector2(5.0f, 2.0f);
                // Draw an indicator on active Element
                ImGui.GetWindowDrawList().AddRectFilled(
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
                ImGui.GetWindowDrawList().AddRectFilled(
                    ImGui.GetItemRectMin() - padding,
                    ImGui.GetItemRectMax() + padding,
                    ImGui.GetColorU32(EditingBackground),
                    4.0f);
            }
        }
        static void EditGroup(string parentType)
        {
            float availableSpace = ImGui.GetContentRegionAvail().X - ImGui.GetTreeNodeToLabelSpacing();

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
