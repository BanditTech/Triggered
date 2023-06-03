using Triggered.modules.options;

namespace Triggered.modules.panel
{
    internal static class Locations
    {
        private static Options_Locations Opts => App.Options.Locations;
        internal static void Render()
        {
            if (Opts == null)
            {
                // We got a problem cowboy.
            }
        }
    }
}
