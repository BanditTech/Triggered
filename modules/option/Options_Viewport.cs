namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Viewport.
    /// </summary>
    public class Options_Viewport : Options
    {
        /// <summary>
        /// Construct a default Viewport options.
        /// </summary>
        public Options_Viewport()
        {
            // Assign the name we will use to save the file
            Name = "Viewport";
            RunDefault();
        }

        internal override void Default()
        {
            SetKey("Fullscreen", true);
            SetKey("Padding", false);
            SetKey("VSync", true);

            TrimNullValues(keyList);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
