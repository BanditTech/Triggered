﻿namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Viewport.
    /// </summary>
    public class Options_Font : Options
    {
        /// <summary>
        /// Construct a default Viewport options.
        /// </summary>
        public Options_Font()
        {
            // Assign the name we will use to save the file
            Name = "Font";
            Default();
        }

        internal override void Default()
        {
            SetKey("Selection", 13);
            SetKey("Name", "Ubuntu");
            SetKey("Size", 18);
            SetKey("Range", 0);

            TrimNullValues(keyList);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}