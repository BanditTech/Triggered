namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Log.
    /// </summary>
    public class Options_Log : Options
    {
        /// <summary>
        /// Construct a default Log options.
        /// </summary>
        public Options_Log()
        {
            // Assign the name we will use to save the file
            Name = "Log";
            RunDefault();
        }

        internal override void Default()
        {
            string[] logLevelNames = { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" };

            SetKey("MinimumLogLevel",
                1,
                "Minimum Log Level",
                logLevelNames);
            SetKey("ScrollToBottom",
                true,
                "Scroll to Bottom");
            SetKey("WindowMaxLines",
                1000,
                "Maximum Log Lines Displayed",
                500,
                10000);
            SetKey("Transparency",
                1f,
                "Background Transparency",
                0.01f,
                1f);

            TrimNullValues(keyList);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
