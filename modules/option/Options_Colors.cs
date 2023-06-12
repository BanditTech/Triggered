using System.Numerics;

namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Colors.
    /// </summary>
    public class Options_Colors : Options
    {
        /// <summary>
        /// Construct a default Colors options.
        /// </summary>
        public Options_Colors()
        {
            // Assign the name we will use to save the file
            Name = "Colors";
            RunDefault();
        }

        internal override void Default()
        {
            SetKey("Resource.Min",
                new Vector4(0f,0f,0f,1f),
                "Filter Minimum");
            SetKey("Resource.Max",
                new Vector4(1f,1f,1f,1f),
                "Filter Maximum");
            SetKey("Resource.Display",
                false,
                "Display Window");
        }
    }
}
