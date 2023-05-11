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
            // Black/White values
            SetKey("minFilterBlackWhite", 67);
            SetKey("maxFilterBlackWhite", 187);
            // Color Values
            SetKey("filterColorRGB", new Vector3(0f, 0f, 1f));
            SetKey("filterup", 0.5f);
            SetKey("filterdown", 0.5f);
            // visibility
            SetKey("Display_AdjustBW",false);
            SetKey("Display_AdjustColor",false);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
