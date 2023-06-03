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
            Default();
        }

        internal override void Default()
        {
            SetKey("MinimumLogLevel", 1);
            SetKey("ScrollToBottom", true);
            SetKey("WindowMaxLines", 1000);
            SetKey("Transparency", 0f);

            TrimNullValues(keyList);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
