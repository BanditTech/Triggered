using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Triggered.modules.wrapper;
using Triggered.modules.struct_filter;
using Triggered.modules.options;

namespace Triggered.modules.panel
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
        static string verticalMovement = null;
        static string verticalIndexer = null;
        static Type verticalType = null;
        static Type removeType;
        static string removeIndexer;
        static Vector4 EditingHighlight = new Vector4(0f, 0.772549f, 1f, 0.392157f);
        static Vector4 EditingBackground = new Vector4(0.0f, 1f, 0.9254902f, 0.1647059f);
        static Vector4 RemoveButton = new Vector4(1.0f, 0.0f, 0.0f, 0.2f);
        static Vector4 RemoveButtonHover = new Vector4(1.0f, 0.0f, 0.0f, 0.6f);
        static Vector4 AddButton = new Vector4(0.0f, 1.0f, 0.0f, 0.2f);
        static Vector4 AddButtonHover = new Vector4(0.0f, 1.0f, 0.0f, 0.6f);
        static bool _showOptions = true;
        static Element _hoveredLeaf = null;
        static Group _hoveredGroup = null;
        static DateTime _lastHover = DateTime.MinValue;
        static Type _addingType = typeof(Element);
        static Type _oldType = typeof(Element);
        static AGroupElement _clay;
        static bool _shiftHeld = false;
        static object _popupModal;
        static string _selectedFile;
        static bool _fileOperation = false;
        public static Type[] ObjectTypes = new Type[] { typeof(Group), typeof(Element) };
        public static readonly string[] objectTypes = new string[] { "Group", "Element" };
        public static readonly string[] EvalOptions = new string[] { ">=", ">", "=", "<", "<=", "~=", ">0<", ">0<=", "!=" };
        public static readonly string[] GroupTypes = new string[] { "AND", "NOT", "COUNT", "WEIGHT" };
        private static Options_StashSorter Opts => App.Options.StashSorter;
        #endregion

        #region Setup Functions
        static StashSorter()
        {
            DumpExampleJson();
            UpdateStashSorterFile();
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
            TopGroup example1 = new TopGroup("Example 1 AND", "AND");
            example1.AddGroup(andGroup);
            example1.AddGroup(notGroup);
            example1.AddGroup(countGroup);
            example1.AddGroup(weightGroup);
            // The file itself is a list of TopGroup
            List<TopGroup> dumpthis = new List<TopGroup>();
            dumpthis.Add(example1);
            // With the final structure, we can serialize
            string jsonString = JSON.Str(dumpthis);
            File.WriteAllText("save\\StashSorter.json", jsonString);
        }
        static void UpdateStashSorterFile()
        {
            string jsonString;
            // if we have a selected file we will use that.
            if (_selectedFile != null && _selectedFile != "" && File.Exists(_selectedFile))
                jsonString = File.ReadAllText(_selectedFile);
            else // Load the default file
                jsonString = File.ReadAllText("save\\StashSorter.json");
            // Store the filter in memory as deserialized objects
            App.StashSorterList = JSON.AGroupElementList(jsonString);
            UpdateTopGroups();
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

        /// <summary>
        /// Primary ImGui menu loop for StashSorter
        /// </summary>
        public static void Render()
        {
            if (!App.Options.MainMenu.GetKey<bool>("Display_StashSorter"))
                return;
            // Create the main window
            ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(500, 200), new Vector2(float.MaxValue, float.MaxValue));
            ImGui.Begin("Edit Stash Sorter", ImGuiWindowFlags.MenuBar);

            // Select the TopGroup
            ImGui.Text("Selected Filter:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            int selectedGroup = Opts.GetKey<int>("SelectedGroup");
            if (ImGui.Combo("##Selected Filter", ref selectedGroup, App.TopGroups, App.TopGroups.Length))
                Opts.SetKey("SelectedGroup", selectedGroup);
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
                // we will adjust the target key if the source group is also in the same key level of the target
                var adjustKey = AdjustTargetKey(_dragSource, _dragTarget, _dragSourceType == typeof(Group),_shiftHeld);
                InsertObjectByIndexer(adjustKey, _dragTargetType, _dragSourceType, fetch);
            }
            if (confirmRemove)
            {
                confirmRemove = false;
                _ = GetObjectByIndexer(removeIndexer, removeType, true);
            }
            if (_dragStarted && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                _dragStarted = false;
            if (verticalMovement != null)
            {
                string parent = verticalIndexer.Substring(0, verticalIndexer.Length - 2);
                int index = int.Parse(verticalIndexer.Substring(verticalIndexer.Length - 1, 1));
                Group parentObj = (Group)GetObjectByIndexer(parent, typeof(Group), false);
                object list = null;
                if (verticalType == typeof(Group))
                    list = parentObj.GroupList;
                else if (verticalType == typeof(Element))
                    list = parentObj.ElementList;

                var Slist = (System.Collections.IList)list;

                if (verticalMovement == "up")
                {
                    var holdingVar = Slist[index];
                    if (index - 1 >= 0)
                    {
                        Slist.RemoveAt(index);
                        Slist.Insert(index - 1,holdingVar);
                    }
                }
                else if (verticalMovement == "down")
                {
                    var holdingVar = Slist[index];
                    if (index + 1 < Slist.Count)
                    {
                        Slist.RemoveAt(index);
                        Slist.Insert(index + 1, holdingVar);
                    }
                }
                verticalMovement = null;
            }
        }

        /// <summary>
        /// This method does the heavy lifting of drawing the filter menu.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parentType"></param>
        /// <param name="indexer"></param>
        static unsafe void RecursiveMenu(AGroupElement obj, string parentType, string indexer = "0")
        {
            bool _hovered;
            if (obj == null)
                return;
            bool IsWeighted = parentType == "COUNT" || parentType == "WEIGHT";

            #region Draw Group
            // Group Section
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
                    int clayType = Array.IndexOf(ObjectTypes, _addingType);
                    if (ImGui.Combo("##Eval", ref clayType, objectTypes, objectTypes.Length))
                        _addingType = ObjectTypes[clayType];
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

                        ImGui.PushID(_clay.GetHashCode()+10);
                        // string Key
                        ImGui.Text("Key:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        AffixFilter.DrawTextBox(ref _element.Key);
                        ImGui.PopID();

                        // We only want to display the Weight field if the parent requires it
                        if (weightedGroup)
                        {
                            ImGui.PushID(_clay.GetHashCode() + 11);
                            // float Weight
                            ImGui.Text("Weight:");
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(72);
                            ImGui.InputFloat("##Weight", ref _element.Weight);
                            ImGui.PopID();
                            ImGui.SameLine();
                        }
                        // string Eval
                        ImGui.PushID(_clay.GetHashCode() + 12);
                        ImGui.Text("Eval:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(72);
                        int comparisonIndex = Array.IndexOf(EvalOptions, _element.Eval);
                        if (ImGui.Combo("##Eval", ref comparisonIndex, EvalOptions, EvalOptions.Length))
                            _element.Eval = EvalOptions[comparisonIndex];
                        ImGui.PopID();

                        // string Min
                        ImGui.PushID(_clay.GetHashCode() + 13);
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
                            // float Weight
                            ImGui.Text("Weight:");
                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(72);
                            ImGui.InputFloat("##Weight", ref _group.Weight);
                        }
                        // string Type
                        ImGui.Text("Type:");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(72);
                        int comparisonIndex = Array.IndexOf(GroupTypes, _group.GroupType);
                        if (ImGui.Combo("##GroupType", ref comparisonIndex, GroupTypes, GroupTypes.Length))
                            _group.GroupType = GroupTypes[comparisonIndex];

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

                    ImGui.Text($"Are you ready to insert the new {objectTypes[clayType]}?");
                    if (ImGui.Button("Yes", new Vector2(120, 0)))
                    {
                        // Add the item
                        if (_clay is Group _group)
                        {
                            var result = _group.Clone();
                            group.Add(result);
                        }
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
                bool isTreeNodeOpen = ImGui.TreeNodeEx(str, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.AllowItemOverlap);

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
                    if (CanDrop(ImGui.GetIO().KeyShift) && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                    {
                        _shiftHeld = ImGui.GetIO().KeyShift;
                        _dragFinalize = true;
                        App.Log($"{_dragSource} was dragged to {_dragTarget}" + (_shiftHeld ? " and it was duplicated." : "."), 0);
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

                    ImGui.SameLine();
                    // Draw Import/Export and Up/Down Buttons at the end of the row
                    //Set up some locat variables related to the right margin
                    bool isScrollable = ImGui.GetScrollMaxY() > 0;
                    var padding = ImGui.GetStyle().FramePadding.X;
                    var scrollAdjust = (isScrollable ? -ImGui.GetStyle().ScrollbarSize : 0);
                    var letterSpace = ImGui.CalcTextSize("Import ").X;

                    // We want these to be fully opaque
                    // Get the original style colors for the button, button hover state, and button active state
                    Vector4* buttonColor = ImGui.GetStyleColorVec4(ImGuiCol.Button);
                    Vector4* buttonHoverColor = ImGui.GetStyleColorVec4(ImGuiCol.ButtonHovered);
                    Vector4* buttonActiveColor = ImGui.GetStyleColorVec4(ImGuiCol.ButtonActive);
                    // Set the alpha channel to 1f for the opaque style
                    buttonColor->W = 1f;
                    buttonHoverColor->W = 1f;
                    buttonHoverColor->X = 0.1f;
                    buttonHoverColor->Y = 0.7f;
                    buttonHoverColor->Z = 0.8f;
                    buttonActiveColor->W = 1f;
                    // Set the opaque style for the button, button hover state, and button active state
                    ImGui.PushStyleColor(ImGuiCol.Button, *buttonColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, *buttonHoverColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, *buttonActiveColor);

                    if (group is not TopGroup)
                    {
                        // Up/Down Button Section
                        bool filled = false;
                        string up = filled ? "▲" + "↑" : "^";
                        string down = filled ? "▼" + "↓" : "v";
                        var directionSpace = ImGui.CalcTextSize("  ").X;
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 7 - letterSpace * 2 - directionSpace * 2 - padding * 4 + scrollAdjust);
                        if (ImGui.Button(up))
                        {
                            verticalMovement = "up";
                            verticalIndexer = indexer;
                            verticalType = typeof(Group);
                        }
                        ImGui.SameLine();
                        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 5 - letterSpace * 2 - directionSpace - padding * 4 + scrollAdjust);
                        if (ImGui.Button(down))
                        {
                            verticalMovement = "down";
                            verticalIndexer = indexer;
                            verticalType = typeof(Group);
                        }
                    }

                    // Import/Export Button Section
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowWidth() - letterSpace * 2 - padding * 4 + scrollAdjust);
                    if (ImGui.Button("Import"))
                    {
                        var import = ImGui.GetClipboardText();
                        if (JSON.Validate(import))
                        {
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
                        else
                        {
                            App.Log("The clipboard did not contain a valid JSON");
                        }
                    }
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(ImGui.GetWindowWidth() - letterSpace - padding * 2 + scrollAdjust);
                    if (ImGui.Button("Export"))
                    {
                        string json = group.ToJson();
                        ImGui.SetClipboardText(json);
                        App.Log(json,0);
                    }
                    // Remove the opacity increase on the buttons
                    ImGui.PopStyleColor(3);
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
            #endregion

            #region Draw Element
            // Element Section
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
                    if (CanDrop(ImGui.GetIO().KeyShift) && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
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


                    // We want these to be fully opaque
                    // Get the original style colors for the button, button hover state, and button active state
                    Vector4* buttonColor = ImGui.GetStyleColorVec4(ImGuiCol.Button);
                    Vector4* buttonHoverColor = ImGui.GetStyleColorVec4(ImGuiCol.ButtonHovered);
                    Vector4* buttonActiveColor = ImGui.GetStyleColorVec4(ImGuiCol.ButtonActive);
                    // Set the alpha channel to 1f for the opaque style
                    buttonColor->W = 1f;
                    buttonHoverColor->W = 1f;
                    buttonHoverColor->X = 0.1f;
                    buttonHoverColor->Y = 0.7f;
                    buttonHoverColor->Z = 0.8f;
                    buttonActiveColor->W = 1f;
                    // Set the opaque style for the button, button hover state, and button active state
                    ImGui.PushStyleColor(ImGuiCol.Button, *buttonColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, *buttonHoverColor);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, *buttonActiveColor);


                    //Set up some locat variables related to the right margin
                    bool isScrollable = ImGui.GetScrollMaxY() > 0;
                    var padding = ImGui.GetStyle().FramePadding.X;
                    var scrollAdjust = (isScrollable ? -ImGui.GetStyle().ScrollbarSize : 0);
                    var letterSpace = ImGui.CalcTextSize("Export ").X;
                    var winWidth = ImGui.GetWindowWidth();

                    // Up/Down Button Section
                    bool filled = false;
                    string   up = filled ? "▲" + "↑" : "^";
                    string down = filled ? "▼" + "↓" : "v";
                    var directionSpace = ImGui.CalcTextSize("  ").X;
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(winWidth - 7 - letterSpace - directionSpace * 2 - padding * 2 + scrollAdjust);
                    if (ImGui.Button(up))
                    {
                        verticalMovement = "up";
                        verticalIndexer = indexer;
                        verticalType = typeof(Element);
                    }
                    ImGui.SameLine();
                    ImGui.SetCursorPosX(winWidth - 5 - letterSpace - directionSpace - padding * 2 + scrollAdjust);
                    if (ImGui.Button(down))
                    {
                        verticalMovement = "down";
                        verticalIndexer = indexer;
                        verticalType = typeof(Element);
                    }


                    ImGui.SameLine();
                    ImGui.SetCursorPosX(winWidth - letterSpace - padding * 2 + scrollAdjust);
                    if (ImGui.Button("Export"))
                    {
                        string json = leaf.ToJson();
                        ImGui.SetClipboardText(json);
                        App.Log(json,0);
                    }
                    EditElement(parentType);
                    // remove the color adjustments
                    ImGui.PopStyleColor(3);
                }
                #endregion

                // Only PopID after all logic is complete
                    ImGui.PopID();
            }
            #endregion
            
            else
                App.Log("This should not display", 4);
        }

        /// <summary>
        /// Gives us a method for importing and exporting top level objects.
        /// </summary>
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
                            // create an instance of AHK to select a path
                            var ahk = new AHK();
                            string returnPath = ahk.SelectFile("1",Path.Combine(AppContext.BaseDirectory,"save"),"Select a file to load.");
                            string dir = Path.GetDirectoryName(_selectedFile);
                            // We need to check if the path is null, blank, or invalid.
                            if (returnPath == null || returnPath == "")
                                App.Log($"File selection was canceled.");
                            else if (!Directory.Exists(dir))
                                App.Log($"File selection of {returnPath} returned an invalid directory: {dir}", 4);
                            else
                            {
                                App.Log($"Loaded file path has been changed to {returnPath}", 0);
                                _selectedFile = returnPath;
                                UpdateStashSorterFile();
                            }
                            // We end by unlocking file operations
                            _fileOperation = false;
                        });
                    }
                    NewSection(1);
                    if (ImGui.MenuItem("Save"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            // We make our default path
                            string defaultPath = Path.Combine(AppContext.BaseDirectory, "save", "StashSorter.json");
                            // We validate the _selectedFile
                            string dir = Path.GetDirectoryName(_selectedFile);
                            bool validSelection = _selectedFile != null && _selectedFile != "" && Directory.Exists(dir);
                            // determine the path we will use to save
                            // if we have loaded a valid location then we will want to continue to use it
                            string path = validSelection ? _selectedFile : defaultPath;
                            File.WriteAllText(path, JSON.Str(App.StashSorterList));
                            App.Log($"Saved file to {path}", 0);
                            // We end by unlocking file operations
                            _fileOperation = false;
                        });
                    }
                    if (ImGui.MenuItem("Save As"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            bool validSelection;
                            // create an instance of AHK to select a path
                            var ahk = new AHK();
                            string result = ahk.SelectFile("S0", Path.Combine(AppContext.BaseDirectory, "save"), "Select a new save target.");
                            // Validate the resulting selection
                            string dir = Path.GetDirectoryName(result);
                            validSelection = result != null && result != "" && Directory.Exists(dir);
                            if (validSelection)
                            {
                                _selectedFile = result;
                                App.Log($"Saving file to selected path at {_selectedFile}", 0);
                                File.WriteAllText(_selectedFile, JSON.Str(App.StashSorterList));
                            }
                            else
                                App.Log($"File selection was canceled.");
                            // We end by unlocking file operations
                            _fileOperation = false;
                        });
                    }
                    NewSection(1);
                    if (ImGui.MenuItem("Reload"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            // determine the target file for the log message
                            bool isSelected = _selectedFile != null && _selectedFile != "" && File.Exists(_selectedFile);
                            string path = isSelected ? _selectedFile : Path.Combine(AppContext.BaseDirectory, "save", "StashSorter.json");
                            App.Log($"Reloading from {path}", 0);
                            // Load the file without saving
                            UpdateStashSorterFile();
                            // We end by unlocking file operations
                            _fileOperation = false;
                        });
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Import"))
                {
                    if (ImGui.MenuItem("Group or Element"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            // Prepare local variables
                            var key = Opts.GetKey<int>("SelectedGroup");
                            var import = ImGui.GetClipboardText();
                            if (JSON.Validate(import))
                            {
                                var Jobj = JSON.AGroupElement(import);
                                // Validate the resulting object type
                                if (Jobj is Group _group)
                                {
                                    App.Log($"Importing group from clipboard:\n" +
                                        $"{import}", 0);
                                    ((TopGroup)App.StashSorterList[key]).Add(_group);
                                }
                                else if (Jobj is Element _element)
                                {
                                    App.Log($"Importing element from clipboard:\n" +
                                        $"{import}", 0);
                                    ((TopGroup)App.StashSorterList[key]).Add(_element);
                                }
                                else
                                    App.Log($"The clipboard contents were not valid Group or Element:\n" +
                                        $"{import}", 0);
                            }
                            else
                            {
                                App.Log("The clipboard did not contain a valid JSON");
                            }
                            // We end by unlocking file operations
                            _fileOperation = false;
                        });
                    }
                    NewSection(1);
                    if (ImGui.BeginMenu("Overwrite"))
                    {
                        if (ImGui.MenuItem("TopGroup"))
                        {
                            if (IsFileOperating())
                                return;
                            Task.Run(() =>
                            {
                                App.Log($"", 0);
                                // We end by unlocking file operations
                                _fileOperation = false;
                            });
                        }
                        NewSection(1);
                        if (ImGui.MenuItem("Full List"))
                        {
                            if (IsFileOperating())
                                return;
                            Task.Run(() =>
                            {
                                App.Log($"", 0);
                                // We end by unlocking file operations
                                _fileOperation = false;
                            });
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Export"))
                {
                    if (ImGui.MenuItem("TopGroup"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            App.Log($"", 0);
                            // We end by unlocking file operations
                            _fileOperation = false;
                        });
                    }
                    NewSection(1);
                    if (ImGui.MenuItem("Full List"))
                    {
                        if (IsFileOperating())
                            return;
                        Task.Run(() =>
                        {
                            App.Log($"", 0);
                            // We end by unlocking file operations
                            _fileOperation = false;
                        });
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
        }

        #region Utility Helpers
        static bool IsFileOperating()
        {
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
        /// <summary>
        /// Fetch the object at the specified indexer.
        /// Type determines the returned index is from Group or Element list.
        /// Pop will remove the object instead of just giving a reference.
        /// </summary>
        /// <param name="indexer"></param>
        /// <param name="type"></param>
        /// <param name="pop"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        static object GetObjectByIndexer(string indexer, Type type, bool pop = false)
        {
            if (indexer == "0" && verticalMovement == null)
                throw new ArgumentException("TopGroup should never be a drag source.");

            string[] indices = indexer.Split('_');
            int length = indices.Length;
            object target = App.StashSorterList[Opts.GetKey<int>("SelectedGroup")];
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
            int selectedGroup = Opts.GetKey<int>("SelectedGroup");
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
        static string AdjustTargetKey(string sourceKey, string destKey, bool isSourceGroup, bool isDuplicating)
        {
            // First we take care of obvious returns
            if (!isSourceGroup || isDuplicating || destKey == "0")
                return destKey;
            // Split the string by the _ symbol in order to get our groups
            string[] sourceParts = sourceKey.Split('_');
            string[] destParts = destKey.Split('_');
            // If the source is deeper than the target 
            if (sourceParts.Length > destParts.Length)
                return destKey;

            for (int i = 1; i < sourceParts.Length - 1; i++)
            {
                if (sourceParts[i] != destParts[i])
                    return destKey;
            }

            var key = sourceParts.Length - 1;

            var sourceInt = int.Parse(sourceParts[key]);
            var destInt = int.Parse(destParts[key]);

            if (sourceInt < destInt)
            {
                destParts[key] = (destInt - 1).ToString();
            }

            return string.Join("_", destParts);
        }
        static bool CanDrop(bool isShiftHeld)
        {
            if (_dragSourceType == typeof(Element))
                return _dragStarted;
            if (isShiftHeld)
                return _dragStarted;
            string source = StripIndexerElement(_dragSourceType, _dragSource);
            string target = StripIndexerElement(_dragTargetType, _dragTarget);

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
                    // float Weight
                    ImGui.Text("Weight:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(72);
                    ImGui.InputFloat("##Weight", ref _rightClickedElement.Weight);
                    ImGui.SameLine();
                }
                // string Key
                ImGui.Text("Key:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(availableSpace);
                AffixFilter.DrawTextBox(ref _rightClickedElement.Key);

                // string Eval
                ImGui.Text("Eval:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(72);
                int comparisonIndex = Array.IndexOf(EvalOptions, _rightClickedElement.Eval);
                if (ImGui.Combo("##Eval", ref comparisonIndex, EvalOptions, EvalOptions.Length))
                    _rightClickedElement.Eval = EvalOptions[comparisonIndex];

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
                int comparisonIndex = Array.IndexOf(GroupTypes, _rightClickedGroup.GroupType);
                if (ImGui.Combo("##GroupType", ref comparisonIndex, GroupTypes, GroupTypes.Length))
                    _rightClickedGroup.GroupType = GroupTypes[comparisonIndex];
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
                    // float Weight
                    ImGui.SameLine();
                    ImGui.Text("Weight:");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(72);
                    ImGui.InputFloat("##Weight", ref _rightClickedGroup.Weight);
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
