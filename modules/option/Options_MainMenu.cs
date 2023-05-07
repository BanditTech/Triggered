namespace Triggered.modules.options
{
    public class Options_MainMenu : Options
    {
        public Options_MainMenu()
        {
            // Assign the name we will use to save the file
            Name = "MainMenu";
            // Panel Visibility
            SetKey("Display_StashSorter", true);
            SetKey("Display_Main", true);
            SetKey("Display_Log", true);
            // Viewport options
            SetKey("Fullscreen", true);
            SetKey("Padding", false);
            // Logic
            SetKey("LogicTickDelayInMilliseconds", 100);
            // Logging
            SetKey("SelectedLogLevelIndex", 1);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
