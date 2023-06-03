namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Main Menu.
    /// </summary>
    public class Options_MainMenu : Options
    {
        /// <summary>
        /// Construct a default Main Menu options.
        /// </summary>
        public Options_MainMenu()
        {
            // Assign the name we will use to save the file
            Name = "MainMenu";
            Default();
        }

        internal override void Default()
        {
            // Logging
            SetKey("Log.SelectedIndex", 1);
            SetKey("Log.AutoScroll", true);
            SetKey("Log.MaxLines", 1000);
            // Font
            SetKey("Font.Index", 13);
            SetKey("Font.Size", 18);
            SetKey("Font.Range", 0);
            // Viewport options
            SetKey("Fullscreen", true);
            SetKey("Padding", false);
            SetKey("VSync", true);
            // Test filtered text
            SetKey("TestText", "");

            TrimNullValues(keyList);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
