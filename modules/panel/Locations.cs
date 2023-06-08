using Triggered.modules.options;
using static Triggered.modules.wrapper.PointScaler;
using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using Triggered.modules.wrapper;
using System;
using System.Linq;
using System.Numerics;

namespace Triggered.modules.panel
{
    internal static class Locations
    {
        private static string _selected;
        private static Options_Locations Opts => App.Options.Locations;
        private static Options_Panel Panel => App.Options.Panel;
        private static readonly Type anchorPosType = typeof(AnchorPosition);
        private static readonly string[] anchorNames = Enum.GetNames(anchorPosType);
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Required for dynamic menu creation")]
        private static readonly Array anchorValues = Enum.GetValues(anchorPosType);
        private static string currentSection;

        [RequiresDynamicCode("Calls Triggered.modules.options.Options.IterateObjects()")]
        internal static void Render()
        {
            if (!Panel.GetKey<bool>("Locations"))
                return;
            ImGui.Begin("Locations");
            foreach (var (key, obj) in Opts.IterateObjects())
            {
                var keySplit = key.Split('.');
                var label = Opts.keyLabels[key];
                var displayedKey = string.IsNullOrEmpty(label) ? string.Join(" ",keySplit) : label;
                if (keySplit.Length > 1 && keySplit[0] != currentSection)
                {
                    ImGui.Spacing();
                    currentSection = keySplit[0];
                    ImGui.Text(currentSection);
                    ImGui.Separator();
                    ImGui.Spacing();
                }
                else if (currentSection != null && keySplit.Length <= 1)
                    currentSection = null;

                if (obj is ScaledRectangle scaledRectangle)
                {
                    ImGui.PushID($"{key} Treenode");
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.6f, 1f, .5f, 1f)); // Set text color to red
                    var treeOpen = ImGui.TreeNode(displayedKey);
                    ImGui.PopStyleColor();
                    ImGui.PopID();
                    if (treeOpen)
                    {
                        ImGui.PushID($"{key} Button");
                        if (ImGui.Button("Select Rectangle"))
                            _selected = key;
                        ImGui.PopID();
                        ImGui.SameLine();
                        ImGui.Text("Anchor:");
                        ImGui.SameLine();
                        var anchorIndex = Array.IndexOf(anchorValues, scaledRectangle.Start.Anchor);
                        ImGui.PushID($"{key} Combo");
                        ImGui.SetNextItemWidth(120);
                        if (ImGui.Combo("##Anchor{key}", ref anchorIndex, anchorNames, anchorNames.Length))
                        {
                            string anchorPositionName = Enum.GetName(anchorPosType, anchorIndex);
                            AnchorPosition newAnchorPosition = (AnchorPosition)Enum.Parse(anchorPosType, anchorPositionName);
                            scaledRectangle.Start = new(scaledRectangle.Start.Point, scaledRectangle.Start.Height, newAnchorPosition);
                            scaledRectangle.End = new(scaledRectangle.End.Point, scaledRectangle.End.Height, newAnchorPosition);
                            Opts.SetKey(key, scaledRectangle);
                        }
                        ImGui.PopID();
                        ImGui.TextColored(new Vector4(.6f, 1f, .8f, 1f), $"Area: ({scaledRectangle.Start.Point.X}, {scaledRectangle.Start.Point.Y}) - ({scaledRectangle.End.Point.X}, {scaledRectangle.End.Point.Y})");
                        ImGui.TextColored(new Vector4(.5f, .5f, 1f, 1f), $"Size: W{scaledRectangle.Width} H{scaledRectangle.Height}");
                        ImGui.SameLine();
                        ImGui.Spacing();
                        ImGui.SameLine();
                        ImGui.Spacing();
                        ImGui.SameLine();
                        ImGui.TextColored(new Vector4(1f, .5f, 1f, 1f), $"ScaleH: {scaledRectangle.Start.Height}");
                        ImGui.TreePop();
                    }

                    if (_selected == key && Selector.ScaledRectangle(ref scaledRectangle,scaledRectangle.Start.Anchor))
                    { 
                        Opts.SetKey(key,scaledRectangle);
                        App.Log($"New \"{keySplit.Last()}\" scaled rectangle taken\n{JSON.Min(scaledRectangle)}");
                        _selected = null;
                    }

                    ImGui.Spacing();
                }
                else if (obj is Coordinate coordinate)
                {
                    ImGui.Text($"Coordinate: {displayedKey}");
                    ImGui.PushID($"{key} Button");
                    ImGui.Indent(40);
                    if (ImGui.Button("Select Point"))
                        _selected = key;
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.Text("Anchor:");
                    ImGui.SameLine();
                    ImGui.PushID($"{key} Combo");
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    var anchorIndex = Array.IndexOf(anchorValues, coordinate.Anchor);
                    if (ImGui.Combo("##Anchor{key}", ref anchorIndex, anchorNames, anchorNames.Length))
                    {
                        string anchorPositionName = Enum.GetName(anchorPosType, anchorIndex);
                        AnchorPosition newAnchorPosition = (AnchorPosition)Enum.Parse(anchorPosType, anchorPositionName);
                        coordinate = new(coordinate.Point, coordinate.Height, newAnchorPosition);
                        Opts.SetKey(key, coordinate);
                    }
                    ImGui.PopID();
                    if (_selected == key && Selector.Coordinate(ref coordinate, coordinate.Anchor))
                    {
                        Opts.SetKey(key, coordinate);
                        App.Log($"New \"{keySplit.Last()}\" coordinate taken\n{JSON.Min(coordinate)}");
                        _selected = null;
                    }
                    ImGui.Text($"Point:{JSON.Min(coordinate.Point)}, " +
                        $"ScaleH {coordinate.Height}");
                    ImGui.SameLine();
                    ImGui.Text($"");

                    ImGui.Spacing();
                    ImGui.Unindent(40);
                }
                else if (obj is Measurement measurement)
                {
                    ImGui.Text($"Coordinate: {displayedKey}");
                    ImGui.PushID($"{key} Button");
                    ImGui.Indent(40);
                    if (ImGui.Button("Select Line"))
                        _selected = key;
                    ImGui.PopID();
                    if (_selected == key && Selector.Measurement(ref measurement))
                    {
                        Opts.SetKey(key, measurement);
                        App.Log($"New \"{keySplit.Last()}\" measurement taken\n{JSON.Min(measurement)}");
                        _selected = null;
                    }
                    ImGui.SameLine();
                    ImGui.Text($"Value:{measurement.Value}, " +
                        $"ScaleH {measurement.Height}");
                    ImGui.SameLine();
                    ImGui.Text($"");

                    ImGui.Spacing();
                    ImGui.Unindent(40);
                }
            }
            ImGui.End();
        }
        private static Vector4 colorText = new(0.5f, .5f, 0.5f, 1f);
        public static void CenteredColorText(string text)
        {
            CenteredColorText(colorText, text);
        }
        public static void CenteredColorText(Vector4 color, string text)
        {
            // Calculate the width of the text
            Vector2 textSize = ImGui.CalcTextSize(text);

            // Calculate the position to center the text
            var windowWidth = ImGui.GetContentRegionAvail().X;
            var calcMiddle = (windowWidth - textSize.X) * 0.5f;
            ImGui.Indent(calcMiddle);
            // Display the text
            ImGui.TextColored(color,text);
            ImGui.Unindent(calcMiddle);
        }
    }
}
