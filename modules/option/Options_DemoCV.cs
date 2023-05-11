using System.Numerics;

namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Demo CV.
    /// </summary>
    public class Options_DemoCV : Options
    {
        /// <summary>
        /// Construct a default DemoCV options.
        /// </summary>
        public Options_DemoCV()
        {
            // Assign the name we will use to save the file
            Name = "DemoCV";
            // Slider values
            SetKey("minFilterBlackWhite", 67);
            SetKey("maxFilterBlackWhite", 187);
            SetKey("This.Key.Name", false);
            SetKey("filterColorRGB", new Vector3(0.5f, 0.5f, 0.5f));
            SetKey("filterup", 60);
            SetKey("filterdown", 60);
            // visibility
            SetKey("Display_AdjustBW",false);
            SetKey("Display_AdjustColor",false);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
