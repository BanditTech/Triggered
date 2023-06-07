using Triggered.modules.options;
using static Triggered.modules.wrapper.PointScaler;
using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using Triggered.modules.wrapper;
using System;

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

        [RequiresDynamicCode("Calls Triggered.modules.options.Options.IterateObjects()")]
        internal static void Render()
        {
            if (!Panel.GetKey<bool>("Locations"))
                return;
            ImGui.Begin("Locations");
            foreach (var (key, obj) in Opts.IterateObjects())
            {
                if (obj is ScaledRectangle scaledRectangle)
                {
                    ImGui.Text($"Scaled Rectangle: {key}");
                    ImGui.PushID($"{key} Button");
                    ImGui.Indent(40);
                    if (ImGui.Button("Select Area"))
                        _selected = key;
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.Text("Anchor:");
                    ImGui.SameLine();
                    ImGui.PushID($"{key} Combo");
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    var anchorIndex = Array.IndexOf(anchorValues, scaledRectangle.Start.Anchor);
                    if (ImGui.Combo("##Anchor{key}",ref anchorIndex, anchorNames, anchorNames.Length))
                    {
                        string anchorPositionName = Enum.GetName(anchorPosType, anchorIndex);
                        AnchorPosition newAnchorPosition = (AnchorPosition)Enum.Parse(anchorPosType, anchorPositionName);
                        scaledRectangle.Start = new(scaledRectangle.Start.Point, scaledRectangle.Start.Height, newAnchorPosition);
                        scaledRectangle.End = new(scaledRectangle.End.Point, scaledRectangle.End.Height, newAnchorPosition);
                        Opts.SetKey(key,scaledRectangle);
                    }
                    ImGui.PopID();
                    if (_selected == key && Selector.ScaledRectangle(ref scaledRectangle,scaledRectangle.Start.Anchor))
                    { 
                        Opts.SetKey(key,scaledRectangle);
                        _selected = null;
                    }
                    ImGui.Spacing();
                    ImGui.Unindent(40);
                }
                else if (obj is Coordinate coordinate)
                {
                    // do
                }
                else if (obj is Measurement measurement)
                {
                    // do 
                }
            }
            ImGui.End();
        }
    }
}
