using Triggered.modules.wrapper;

namespace Triggered.modules.options
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
            RunDefault();
        }

        internal override void Default()
        {
            SetKey(
                "Selection",
                13,
                "Font",
                App.fonts,
                Callbacks.FontIndexEdit
            );
            SetKey(
                "Name",
                "Ubuntu",
                true
            );
            SetKey(
                "Size",
                18,
                "Size",
                6,                
                30,
                Callbacks.SetFont
            );
            SetKey(
                "Range",
                0,
                "Glyph Range",
                App.glyphs,
                Callbacks.SetFont
            );

            TrimNullValues(keyList);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
