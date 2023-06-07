using Triggered.modules.options;
using static Triggered.modules.wrapper.PointScaler;
using ImGuiNET;
using System.Diagnostics.CodeAnalysis;
using Triggered.modules.wrapper;

namespace Triggered.modules.panel
{
    internal static class Locations
    {
        private static string _selected;
        private static Options_Locations Opts => App.Options.Locations;
        private static Options_Panel Panel => App.Options.Panel;

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
                    if (ImGui.Button(key))
                        _selected = key;
                    if (_selected == key)
                    {
                        if (Selector.ScaledRectangle(ref scaledRectangle,scaledRectangle.Start.Anchor))
                        {
                            Opts.SetKey(key,scaledRectangle);
                            _selected = null;
                        }
                    }
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
