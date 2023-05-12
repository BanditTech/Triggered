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
            // Individual Color Values
            SetKey("filterColorIndRGB", new Vector3(0f, 1f, 0f));
            SetKey("filterR", 0.5f);
            SetKey("filterG", 0.5f);
            SetKey("filterB", 0.5f);
            // HSV Color Values
            SetKey("filterColorHSV", new Vector3(0.5f, 0.5f, 0.5f));
            SetKey("filterH", 0.5f);
            SetKey("filterS", 0.5f);
            SetKey("filterV", 0.5f);
            // HSV Dual Color Values
            SetKey("filterColorHSVMin", new Vector3(0.4f, 0f, 0f));
            SetKey("filterColorHSVMax", new Vector3(0.5f, 1f, 1f));
            // visibility
            SetKey("Display_AdjustBW",false);
            SetKey("Display_AdjustColor",false);
            SetKey("Display_AdjustIndColor", false);
            SetKey("Display_AdjustHSVColor", false);
            SetKey("Display_AdjustHSVColorDual", false);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
