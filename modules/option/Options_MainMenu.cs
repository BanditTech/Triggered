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
            SetKey("SelectedLogLevelIndex", 1);
            SetKey("LogAutoScroll", true);
            SetKey("LogMaxLines", 1000);
            // Font
            SetKey("Font.Index", 13);
            SetKey("Font.Size", 18);
            SetKey("Font.Range", 0);
            // Panel Visibility
            SetKey("Display_StashSorter", true);
            SetKey("Display_Main", true);
            SetKey("Display_Log", true);
            // Viewport options
            SetKey("Fullscreen", true);
            SetKey("Padding", false);
            SetKey("VSync", true);
            // Logic
            SetKey("LogicTickDelayInMilliseconds", 100);
            // Test filtered text
            SetKey("TestText", "");
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
