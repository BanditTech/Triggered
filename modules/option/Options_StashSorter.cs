namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Stash Sorter.
    /// </summary>
    public class Options_StashSorter : Options
    {
        /// <summary>
        /// Construct a default Stash Sorter options.
        /// </summary>
        public Options_StashSorter()
        {
            // Assign the name we will use to save the file
            Name = "StashSorter";
            RunDefault();
        }
        internal override void Default()
        {
            // Determines which group is being edited inside the Stash Sorter list
            SetKey("SelectedGroup", 0);

            TrimNullValues(keyList);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
