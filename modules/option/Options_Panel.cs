using System.Numerics;

namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for Panel Visibility.
    /// </summary>
    public class Options_Panel : Options
    {
        /// <summary>
        /// Construct a default Panel options.
        /// </summary>
        public Options_Panel()
        {
            // Assign the name we will use to save the file
            Name = "Panel";
            RunDefault();
        }
        internal override void Default()
        {
            // Core Panels
            SetKey("RenderChildren", true);
            SetKey("StashSorter", true);
            SetKey("MainMenu", true);
            SetKey("LogWindow", true);
            SetKey("Log", false);
            SetKey("Locations", false);
            SetKey("Font", false);
            SetKey("Viewport", false);
            SetKey("Colors", false);

            // CV Demo panels
            SetKey("CV.SubsetHSV", false);
            SetKey("CV.Shape", false);
            SetKey("CV.Rectangle", false);
            SetKey("CV.OCR", false);
        }
    }
}
