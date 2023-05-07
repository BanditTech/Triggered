using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Triggered.modules.wrapper;

namespace Triggered.modules.panels
{
    static class StashSorter
    {
        #region Static Values
        static string _dragTarget;
        static Type _dragTargetType;
        static string _dragSource;
        static Type _dragSourceType;
        static bool _dragStarted;
        static bool _dragFinalize;
        static Element _rightClickedElement = null;
        static Group _rightClickedGroup = null;
        static bool confirmRemove = false;
        static Type removeType;
        static string removeIndexer;
        static Vector4 EditingHighlight = new Vector4(0f, 0.772549f, 1f, 0.392157f); // #00C5FF64
        static Vector4 EditingBackground = new Vector4(0.0f, 1f, 0.9254902f, 0.1647059f); // #00FFEC2A
        static Vector4 RemoveButton = new Vector4(1.0f, 0.0f, 0.0f, 0.2f);
        static Vector4 RemoveButtonHover = new Vector4(1.0f, 0.0f, 0.0f, 0.6f);
        static Vector4 AddButton = new Vector4(0.0f, 1.0f, 0.0f, 0.2f);
        static Vector4 AddButtonHover = new Vector4(0.0f, 1.0f, 0.0f, 0.6f);
        static bool _showOptions = true;
        static Element _hoveredLeaf = null;
        static Group _hoveredGroup = null;
        static float _lineHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2f;
        static DateTime _lastHover = DateTime.MinValue;
        static Type _addingType = typeof(Element);
        static Type _oldType = typeof(Element);
        static AGroupElement _clay;
        static bool _shiftHeld = false;
        static object _popupModal;
        static string _selectedFile;
        static bool _fileOperation = false;
        #endregion

        #region Setup Functions
        static StashSorter()
        {
            DumpExampleJson();
            UpdateStashSorterFile();
            UpdateTopGroups();
        }
        static void DumpExampleJson()
        {
            // We construct a default Group/Element
            Group andGroup = new Group("AND");
            Group notGroup = new Group("NOT");
            Group countGroup = new Group("COUNT", 4);
            Group weightGroup = new Group("WEIGHT", 100);
            // We add some children to the group
            andGroup.AddElement(new Element("Both This", ">=", "1"));
            andGroup.AddElement(new Element("And This", ">=", "1"));
            notGroup.AddElement(new Element("Neither This", ">=", "1"));
            notGroup.AddElement(new Element("Nor This", ">=", "1"));
            countGroup.AddElement(new Element("Requires Both This", ">=", "1", 2));
            countGroup.AddElement(new Element("And This", ">=", "1", 2));
            countGroup.AddElement(new Element("Or just This", ">=", "1", 4));
            weightGroup.AddElement(new Element("This values at 10 per stat", ">=", "1", 10));
            weightGroup.AddElement(new Element("This values at 20 per stat", ">=", "1", 20));
            weightGroup.AddElement(new Element("This would require 100 of the stat", ">=", "100", 1));
            // Make our TopGroup to put them all in
            TopGroup example1 = new TopGroup("Example 1 AND", "AND", default, default, default);
            example1.AddGroup(andGroup);
            example1.AddGroup(notGroup);
            example1.AddGroup(countGroup);
            example1.AddGroup(weightGroup);
            // The file itself is a list of TopGroup
            List<TopGroup> dumpthis = new List<TopGroup>();
            dumpthis.Add(example1);
            // With the final structure, we can serialize
            string jsonString = JSON.Str(dumpthis);
            File.WriteAllText("example.json", jsonString);
        }
        static void UpdateStashSorterFile()
        {
            string jsonString;
            // if we have a selected file we will use that.
            if (_selectedFile != null && _selectedFile != "" && File.Exists(_selectedFile))
                jsonString = File.ReadAllText(_selectedFile);
            else // Load the example file
                jsonString = File.ReadAllText("example.json");
            // Store the filter in memory as deserialized objects
            App.StashSorterList = JSON.AGroupElementList(jsonString);
        }
        static void UpdateTopGroups()
        {
            // Fetch the GroupName of each TopGroup and save to a string array in App.TopGroups
            List<string> topGroupsList = new List<string>();
            foreach (AGroupElement group in App.StashSorterList)
            {
                if (group is TopGroup topGroup)
                    topGroupsList.Add(topGroup.GroupName);
            }
            App.TopGroups = topGroupsList.ToArray();
        }
        #endregion

        public static void Render()
        {
            var options = App.Options.StashSorter;
            // Create the main window
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(500, 200), new Vector2(float.MaxValue, float.MaxValue));
            ImGui.Begin("Edit Stash Sorter", ImGuiWindowFlags.MenuBar);

            // Select the TopGroup
            ImGui.Text("Selected Filter:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            int selectedGroup = options.GetKey<int>("SelectedGroup");
            if (ImGui.Combo("##Selected Filter", ref selectedGroup, App.TopGroups, App.TopGroups.Length))
                options.SetKey("SelectedGroup", selectedGroup);
            ImGui.Spacing();

            // Add adjustments for TopGroup values
            if (App.StashSorterList[selectedGroup] is TopGroup topGroup)
            {
                // int GroupName
                ImGui.Text("GroupName:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                ImGui.InputText("##GroupName", ref topGroup.GroupName, 256);
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
            }

            // Add spacing
            NewSection();

            // We recurse the Group structure drawing them onto our menu
            RecursiveMenu(App.StashSorterList[selectedGroup], "NONE");

            // Add spacing
            NewSection();

            // Color pickers for the Right Click editor
            if (ImGui.CollapsingHeader("Editor color adjustment", ref _showOptions))
            {
                float space = ImGui.GetContentRegionAvail().X * 0.4f;
                ImGui.SetNextItemWidth(space);
                ImGui.ColorPicker4("##Highlight", ref EditingHighlight);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(space);
                ImGui.ColorPicker4("##Background", ref EditingBackground);
            }

            DrawMenuBar();

            // End the main window
            ImGui.End();

            // finalize the drag action while the collection is not being looped
            if (_dragFinalize)
            {
                _dragStarted = false;
                _dragFinalize = false;
                object fetch = GetObjectByIndexer(_dragSource, _dragSourceType, !_shiftHeld);
                InsertObjectByIndexer(_dragTarget, _dragTargetType, _dragSourceType, fetch);
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

        static void RecursiveMenu(AGroupElement obj, string parentType, string indexer = "0")
        {
            bool _hovered;
            if (obj == null)
                return;
            bool IsWeighted = parentType == "COUNT" || parentType == "WEIGHT";
            if (obj is Group group)
            {
                // We need to push this ID here to associate everything with the tree node
                ImGui.PushID(group.GetHashCode());

                #region Hover Buttons
                // If we match this group, we draw the Add/Remove buttons
                if (_hoveredGroup == group)
                {
                    // Save our origin style
                    var style = ImGui.GetStyle();
                    var oldSpacing = style.ItemSpacing;
                    // We want to exclude the topgroup from making a remove button
                    if (group is not TopGroup)
                    {
                        // Remove Button
                        ImGui.PushStyleColor(ImGuiCol.Button, RemoveButton);
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, RemoveButtonHover);
                        if (ImGui.Button(" x "))
                        {
                            removeType = typeof(Group);
                            removeIndexer = indexer;
                            ImGui.OpenPopup("Remove Group");
                        }
                        ImGui.PopStyleColor(2);
                        // Check if we hover this button
                        _hovered = ImGui.IsItemHovered();
                        if (_hovered && _hoveredGroup == group)
                            _lastHover = DateTime.Now;

                        // Change the spacing to remove the gap
                        style.ItemSpacing = Vector2.Zero;
                        ImGui.SameLine();
                    }
                    // All groups have an Add Button
                    ImGui.PushStyleColor(ImGuiCol.Button, AddButton);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, AddButtonHover);
                    if (ImGui.Button(group is not TopGroup ? " + " : "   +   "))
                    {
                        _popupModal = group;
                        ImGui.OpenPopup("Add to Group");
                    }
                    ImGui.PopStyleColor(2);
                    // Restore the origin spacing
                    style.ItemSpacing = oldSpacing;
                    // Check if we hover this button
                    _hovered = ImGui.IsItemHovered();
                    if (_hovered && _hoveredGroup == group)
                        _lastHover = DateTime.Now;
                    ImGui.SameLine();
                }
                else
                {
                    // Draw an identifier button
                    // This also is a bandaid to get uniform line height
                    ImGui.Button(" Group ");
                    // Check if we hover this button
                    _hovered = ImGui.IsItemHovered();
                    if (_hovered && _hoveredGroup != group)
                    {
                        // Swap Group button to Add/Remove
                        _hoveredGroup = group;
                        _hoveredLeaf = null;
                        _lastHover = DateTime.Now;
                    }
                    ImGui.SameLine();
                }
                #endregion

                #region Popup Modals
                if (ImGui.BeginPopupModal("Remove Group"))
                {
                    ImGui.Text("Are you sure you want to delete this Group?");
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
                if (_popupModal == group && ImGui.BeginPopupModal("Add to Group"))
                {
                    int clayType = Array.IndexOf(App.ObjectTypes, _addingType);
                    if (ImGui.Combo("##Eval", ref clayType, App.objectTypes, App.objectTypes.Length))
                        _addingType = App.ObjectTypes[clayType];
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
                    }

                    if (_clay is Element _element)
                    {
                        bool weightedGroup = group.GroupType == "COUNT" || group.GroupType == "WEIGHT";
                        bool weightedParent = parentType == "COUNT" || parentType == "WEIGHT";

                        ImGui.PushID(_clay.GetHashCode());
                        // string Key
                        ImGui.Text("Key:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        ImGui.InputText("##Key", ref _element.Key, 256);
                        ImGui.PopID();

                        // We only want to display the Weight field if the parent requires it
                        if (weightedGroup)
                        {
                            ImGui.PushID(_clay.GetHashCode() + 1);
                            // int Weight
                            ImGui.Text("Weight:");
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(72);
                            ImGui.InputInt("##Weight", ref _element.Weight);
                            ImGui.PopID();
                            ImGui.SameLine();
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
                        ImGui.Text("Min:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        ImGui.InputText("##Min", ref _element.Min, 256);
                        ImGui.PopID();
                    }
                    else if (_clay is Group _group)
                    {
                        bool weightedGroup = _group.GroupType == "COUNT" || _group.GroupType == "WEIGHT";
                        bool weightedParent = group.GroupType == "COUNT" || group.GroupType == "WEIGHT";

                        if (weightedParent)
                        {
                            // int Weight
                            ImGui.Text("Weight:");
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(72);
                            ImGui.InputInt("##Weight", ref _group.Weight);
                        }
                        // string Type
                        ImGui.Text("Type:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(72);
                        int comparisonIndex = Array.IndexOf(App.GroupTypes, _group.GroupType);
                        if (ImGui.Combo("##GroupType", ref comparisonIndex, App.GroupTypes, App.GroupTypes.Length))
                            _group.GroupType = App.GroupTypes[comparisonIndex];

                        if (weightedGroup)
                        {
                            // int Min
                            ImGui.SameLine();
                            ImGui.Text("Min:");
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(72);
                            ImGui.InputInt("##Min", ref _group.Min);
                        }
                    }

                    ImGui.Text($"Are you ready to insert the new {App.objectTypes[clayType]}?");
                    if (ImGui.Button("Yes", new Vector2(120, 0)))
                    {
                        // Add the item
                        if (_clay is Group _group)
                            group.Add(_group.Clone());
                        else if (_clay is Element _leaf)
                            group.Add(_leaf.Clone());
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No", new Vector2(120, 0)))
                    {
                        _clay = null;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
                #endregion

                // Change the display text depending on the group and parent
                string str = IsWeighted ? $"{group.Weight}# {group.GroupType} Group" : $"{group.GroupType} Group";
                str += group.GroupType == "COUNT" ? $" with minimum of {group.Min} match value"
                    : group.GroupType == "WEIGHT" ? $" with minimum of {group.Min} weight" : "";
                // Create our tree node to allow collapsable states
                bool isTreeNodeOpen = ImGui.TreeNodeEx(str, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick);

                #region Hover Button Logic
                // Check if we are hovering the tree node
                _hovered = ImGui.IsItemHovered();
                if (_hovered && _hoveredGroup != group)
                {
                    _lastHover = DateTime.Now;
                    _hoveredGroup = group;
                }
                else if (_hovered && _hoveredGroup == group)
                    _lastHover = DateTime.Now;
                else if (!ImGui.IsAnyItemHovered() && _hoveredGroup == group && (DateTime.Now - _lastHover).TotalSeconds > .2f)
                    _hoveredGroup = null;
                else if (!_hovered && _hoveredGroup == group && _hoveredLeaf != null)
                    _hoveredGroup = null;
                #endregion

                #region Drag and Drop
                if (ImGui.BeginDragDropTarget())
                {
                    _dragTarget = indexer;
                    _dragTargetType = typeof(Group);
                    if (CanDrop() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                    {
                        _shiftHeld = ImGui.GetIO().KeyShift;
                        _dragFinalize = true;
                    }
                    ImGui.EndDragDropTarget();
                }
                if (group is not TopGroup && ImGui.BeginDragDropSource())
                {
                    _dragStarted = true;
                    _dragSource = indexer;
                    _dragSourceType = typeof(Group);
                    ImGui.SetDragDropPayload("COMPONENT", IntPtr.Zero, 0);
                    ImGui.Text($"{indexer}");
                    ImGui.EndDragDropSource();
                }
                #endregion

                #region Right Click Edit
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    if (_rightClickedGroup == group)
                        _rightClickedGroup = null;
                    else
                    {
                        _rightClickedElement = null;
                        _rightClickedGroup = group;
                    }
                }
                if (_rightClickedGroup != null && _rightClickedGroup == group)
                {
                    // Draw an indicator on active Group
                    var drawList = ImGui.GetWindowDrawList();
                    drawList.AddRectFilled(
                        ImGui.GetItemRectMin(),
                        ImGui.GetItemRectMax(),
                        ImGui.GetColorU32(EditingHighlight),
                        4.0f);

                    // Draw Import and Export Buttons at the end of the row
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Import ").X - ImGui.CalcTextSize("Export ").X - ImGui.GetStyle().FramePadding.X * 4);
                    if (ImGui.Button("Import"))
                    {
                        var import = ImGui.GetClipboardText();
                        var Jobj = JSON.AGroupElement(import);
                        if (Jobj is Group _group)
                        {
                            App.Log($"Importing group from clipboard:\n" +
                                $"{import}");
                            group.Add(_group);
                        }
                        else if (Jobj is Element _element)
                        {
                            App.Log($"Importing element from clipboard:\n" +
                                $"{import}");
                            group.Add(_element);
                        }
                    }
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Export ").X - ImGui.GetStyle().FramePadding.X * 2);
                    if (ImGui.Button("Export"))
                    {
                        string json = group.ToJson();
                        ImGui.SetClipboardText(json);
                        App.Log(json);
                    }
                    EditGroup(parentType);
                }
                #endregion

                // Only PopID after all logic is complete
                ImGui.PopID();


                // If the Tree is open, we draw its children
                if (isTreeNodeOpen)
                {
                    int i = -1;
                    foreach (Element subElement in group.ElementList)
                    {
                        i++;
                        RecursiveMenu(subElement, group.GroupType, $"{indexer}_{i}");
                    }
                    i = -1;
                    foreach (Group subGroup in group.GroupList)
                    {
                        i++;
                        RecursiveMenu(subGroup, group.GroupType, $"{indexer}_{i}");
                    }
                    ImGui.TreePop();
                }
            }
            else if (obj is Element leaf)
            {
                // We need to push this ID here to associate everything with the selectable
                ImGui.PushID(leaf.GetHashCode());

                #region Hover Buttons
                // If we match this leaf, we draw the Remove button
                if (_hoveredLeaf == leaf)
                {
                    // Remove Button
                    ImGui.PushStyleColor(ImGuiCol.Button, RemoveButton);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, RemoveButtonHover);
                    if (ImGui.Button("   x   "))
                    {
                        removeType = typeof(Element);
                        removeIndexer = indexer;
                        ImGui.OpenPopup("Delete Element");
                    }
                    ImGui.PopStyleColor(2);
                    // Check if we hover this button
                    _hovered = ImGui.IsItemHovered();
                    if (_hovered && _hoveredLeaf == leaf)
                        _lastHover = DateTime.Now;
                    ImGui.SameLine();
                }
                else
                {
                    // Draw an identifier button
                    ImGui.Button("Element");
                    // Check if we hover this button
                    _hovered = ImGui.IsItemHovered();
                    if (_hovered && _hoveredLeaf != leaf)
                    {
                        // Swap Element button to Remove
                        _hoveredLeaf = leaf;
                        _hoveredGroup = null;
                        _lastHover = DateTime.Now;
                    }
                    ImGui.SameLine();
                }
                #endregion

                #region Popup Modal
                if (ImGui.BeginPopupModal("Delete Element"))
                {
                    ImGui.Text("Are you sure you want to delete this Element?");
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
                #endregion

                // Build the string we will use to display
                string str = IsWeighted ? $"{leaf.Weight}# " : "";
                str += $"{leaf.Key} {leaf.Eval} {leaf.Min}";

                if (_rightClickedElement != null && _rightClickedElement == leaf)
                {

                    ImGui.PushItemWidth(100f);
                    // We make our Selectable
                    ImGui.Selectable(str, false, ImGuiSelectableFlags.AllowItemOverlap | ImGuiSelectableFlags.AllowDoubleClick);
                }
                else
                {
                    // We make our Selectable
                    ImGui.Selectable(str);
                }

                #region Hover Button Logic
                _hovered = ImGui.IsItemHovered();
                if (_hovered && _hoveredLeaf != leaf)
                {
                    _lastHover = DateTime.Now;
                    _hoveredLeaf = leaf;
                }
                else if (_hovered && _hoveredLeaf == leaf)
                    _lastHover = DateTime.Now;
                else if (!ImGui.IsAnyItemHovered() && _hoveredLeaf == leaf && (DateTime.Now - _lastHover).TotalSeconds > .2f)
                    _hoveredLeaf = null;
                else if (!_hovered && _hoveredLeaf == leaf && _hoveredGroup != null)
                    _hoveredLeaf = null;
                #endregion

                #region Drag and Drop
                if (ImGui.BeginDragDropTarget())
                {
                    _dragTarget = indexer;
                    _dragTargetType = typeof(Element);
                    if (CanDrop() && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                    {
                        _shiftHeld = ImGui.GetIO().KeyShift;
                        _dragFinalize = true;
                    }
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
                #endregion

                #region Right Click Edit
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    if (_rightClickedElement == leaf)
                        _rightClickedElement = null;
                    else
                    {
                        _rightClickedGroup = null;
                        _rightClickedElement = leaf;
                    }
                }
                if (_rightClickedElement != null && _rightClickedElement == leaf)
                {
                    // Draw an indicator on active Element
                    ImGui.GetWindowDrawList().AddRectFilled(
                        ImGui.GetItemRectMin(),
                        ImGui.GetItemRectMax(),
                        ImGui.GetColorU32(EditingHighlight),
                        4.0f);

                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Export ").X - ImGui.GetStyle().FramePadding.X * 2);
                    if (ImGui.Button("Export"))
                    {
                        string json = leaf.ToJson();
                        ImGui.SetClipboardText(json);
                        App.Log(json);
                    }
                    EditElement(parentType);
                }
                #endregion

                // Only PopID after all logic is complete
                ImGui.PopID();
            }
            else
                App.Log("This should not display", NLog.LogLevel.Error);
        }

        static void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.MenuItem("Hide"))
                {
                    var mainMenu = App.Options.MainMenu;
                    var value = mainMenu.GetKey<bool>("Display_StashSorter");
                    mainMenu.SetKey("Display_StashSorter", !value);
                }
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Load"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            var ahk = new AHK();
                            _selectedFile = ahk.SelectFile();
                            App.Log($"Loaded file path has been changed to {_selectedFile}");
                            UpdateStashSorterFile();
                            IsFileOperating(false);
                        });
                    }
                    NewSection(1);
                    if (ImGui.MenuItem("Save"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            string example = Path.Combine(AppContext.BaseDirectory, "examplesave.json");
                            bool validSelection = _selectedFile != null && _selectedFile != "" && File.Exists(_selectedFile);
                            string path = validSelection ? _selectedFile : example;
                            File.WriteAllText(path, JSON.Str(App.StashSorterList));
                            App.Log($"Saved file to {path}");
                            IsFileOperating(false);
                        });
                    }
                    if (ImGui.MenuItem("Save As"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            bool validSelection;
                            var ahk = new AHK();
                            string result = ahk.SelectFile(default, "S0");
                            validSelection = result != null && result != "";
                            if (validSelection)
                            {
                                _selectedFile = result;
                                App.Log($"Saving file to selected path at {_selectedFile}");
                                File.WriteAllText(_selectedFile, JSON.Str(App.StashSorterList));
                            }
                            else
                            {
                                App.Log("Issue making a selection, nothing was saved");
                            }
                            IsFileOperating(false);
                        });
                    }
                    NewSection(1);
                    if (ImGui.MenuItem("Reload"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            App.Log($"Saved file to ");
                            IsFileOperating(false);
                        });
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Import"))
                {
                    if (ImGui.MenuItem("Group"))
                    {
                        // handle Group action
                    }
                    if (ImGui.MenuItem("Element"))
                    {
                        // handle Element action
                    }
                    NewSection(1);
                    if (ImGui.BeginMenu("Overwrite"))
                    {
                        if (ImGui.MenuItem("TopGroup"))
                        {
                            // handle TopGroup action
                        }
                        NewSection(1);
                        if (ImGui.MenuItem("Full List"))
                        {
                            // handle TopGroup action
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Export"))
                {
                    if (ImGui.MenuItem("TopGroup"))
                    {
                        // handle Group action
                    }
                    NewSection(1);
                    if (ImGui.MenuItem("Full List"))
                    {
                        // handle Element action
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
        }

        #region Utility Helpers
        static bool IsFileOperating(bool reset = false)
        {
            if (reset)
            {
                _fileOperation = false;
                return false;
            }
            if (_fileOperation)
                return true;
            if (!_fileOperation)
                _fileOperation = true;
            return false;
        }
        static void Spacers(int count)
        {
            for (int i = 0; i < count; i++)
                ImGui.Spacing();
        }
        static void NewSection(int count = 2, bool separate = true)
        {
            Spacers(count);
            if (separate)
                ImGui.Separator();
            Spacers(count);
        }
        #endregion

        #region Movement Helpers
        static object GetObjectByIndexer(string indexer, Type type, bool pop = false)
        {
            if (indexer == "0")
                throw new ArgumentException("TopGroup should never be a drag source.");

            string[] indices = indexer.Split('_');
            int length = indices.Length;
            object target = App.StashSorterList[App.Options.StashSorter.GetKey<int>("SelectedGroup")];
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
                        if (index > ((dynamic)target).ElementList.Count - 1)
                            index = ((dynamic)target).ElementList.Count - 1;
                        parent = target;
                        target = ((Group)target).ElementList[index];
                        if (pop)
                            ((Group)parent).ElementList.RemoveAt(index);
                    }
                    else if (type == typeof(Group))
                    {
                        // final indexer digit is in GroupList array
                        if (index > ((dynamic)target).GroupList.Count - 1)
                            index = ((dynamic)target).GroupList.Count - 1;
                        parent = target;
                        target = ((Group)target).GroupList[index];
                        if (pop)
                            ((Group)parent).GroupList.RemoveAt(index);
                    }
                    else
                        throw new ArgumentException($"Expecting GROUP or ELEMENT type but received {type}");
                }
                else
                {
                    if (index > ((dynamic)target).GroupList.Count - 1)
                        index = ((dynamic)target).GroupList.Count - 1;
                    target = ((dynamic)target).GroupList[index];
                }
            }
            if (type == typeof(Group))
                return (Group)target;
            else if (type == typeof(Element))
                return (Element)target;
            return target;
        }
        static void InsertObjectByIndexer(string indexer, Type targetType, Type sourceType, object obj)
        {
            int selectedGroup = App.Options.StashSorter.GetKey<int>("SelectedGroup");
            if (indexer == "0")
            {
                if (sourceType == typeof(Group))
                    ((TopGroup)App.StashSorterList[selectedGroup]).Add(((Group)obj).Clone());
                else if (sourceType == typeof(Element))
                    ((TopGroup)App.StashSorterList[selectedGroup]).Add(((Element)obj).Clone());
                return;
            }

            string[] indices = indexer.Split('_');
            int length = indices.Length;
            Group target = (TopGroup)App.StashSorterList[selectedGroup];

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
                        if (key > ((dynamic)target).GroupList.Count - 1)
                            key = ((dynamic)target).GroupList.Count - 1;
                        target = target.GroupList[key];
                        key = 0;
                    }
                    // Depending on our source type, we insert the correct object type
                    if (sourceType == typeof(Element))
                        target.Insert(key, ((Element)obj).Clone());
                    else if (sourceType == typeof(Group))
                        target.Insert(key, ((Group)obj).Clone());
                }
                else
                {
                    if (key > ((dynamic)target).GroupList.Count - 1)
                        key = ((dynamic)target).GroupList.Count - 1;
                    target = ((dynamic)target).GroupList[key];
                }
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
            string source = StripIndexerElement(_dragSourceType, _dragSource);
            string target = StripIndexerElement(_dragTargetType, _dragTarget);

            if (_dragSourceType == typeof(Element))
                return true;

            return source != target && _dragStarted && !IsChildObject(source, target);
        }
        #endregion

        #region Right Click Edit Functions
        static void EditElement(string parentType)
        {
            float availableSpace = ImGui.GetContentRegionAvail().X - ImGui.GetTreeNodeToLabelSpacing();

            if (_rightClickedElement != null)
            {
                ImGui.BeginGroup();
                Vector2 padding = new Vector2(5.0f, 2.0f);

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
