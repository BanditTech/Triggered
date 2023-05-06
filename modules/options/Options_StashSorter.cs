namespace Triggered.modules.options
{
    public class Options_StashSorter : Options
    {
        public Options_StashSorter()
        {
            // Assign the name we will use to save the file
            Name = "StashSorter";
            // Determines which group is being edited inside the Stash Sorter list
            SetKey("SelectedGroup", 0);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
