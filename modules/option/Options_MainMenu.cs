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
            // Logging
            SetKey("SelectedLogLevelIndex", 1);
            SetKey("LogAutoScroll", true);
            SetKey("LogMaxLines", 1000);
            // Panel Visibility
            SetKey("Display_StashSorter", true);
            SetKey("Display_Main", true);
            SetKey("Display_Log", true);
            // Viewport options
            SetKey("Fullscreen", true);
            SetKey("Padding", false);
            // Logic
            SetKey("LogicTickDelayInMilliseconds", 100);
            SetKey("This.0.Name", false);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
