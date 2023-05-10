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
            // visibility
            SetKey("Display_AdjustBW",false);
            // Slider values
            SetKey("minFilterColorR", 67);
            SetKey("maxFilterColorR", 187);
            SetKey("minFilterColorG", 67);
            SetKey("maxFilterColorG", 187);
            SetKey("minFilterColorB", 67);
            SetKey("maxFilterColorB", 187);
            // visibility
            SetKey("Display_AdjustColor",false);

            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
