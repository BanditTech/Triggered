using Triggered.modules.options;
using static Triggered.modules.wrapper.PointScaler;
using ImGuiNET;

namespace Triggered.modules.panel
{
    internal static class Locations
    {
        private static Options_Locations Opts => App.Options.Locations;
        internal static void Render()
        {
            foreach (var location in Opts.Iterate())
            {
                switch (location)
                {
                    case Coordinate coordinate:
                        ImGui.Button("Here");
                        // Code for handling Coordinate type
                        break;
                    case ScaledRectangle scaledRectangle:
                        // Code for handling ScaledRectangle type
                        break;
                    case Measurement measurement:
                        // Code for handling Measurement type
                        break;
                    default:
                        // Default case if the type doesn't match any of the expected types
                        break;
                }
            }
        }
    }
}
