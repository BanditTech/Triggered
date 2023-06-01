using static Triggered.modules.wrapper.PointScaler;
using System.Drawing;

namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Demo CV.
    /// </summary>
    public class Options_Locations : Options
    {
        /// <summary>
        /// Construct a default DemoCV options.
        /// </summary>
        public Options_Locations()
        {
            // Assign the name we will use to save the file
            Name = "Locations";
            Default();
        }
        internal override void Default()
        {
            // Configure Default values

            SetKey("Resource.Left", new ScaledRectangle(
                new Coordinate(
                    new Point(40, 250),
                    1080,
                    AnchorPosition.Left
                ),
                new Coordinate(
                    new Point(250, 310),
                    1080,
                    AnchorPosition.Left
                )
            ));
            SetKey("Resource.Right", new ScaledRectangle(
                new Coordinate(
                    new Point(250, 250),
                    1080,
                    AnchorPosition.Right
                ),
                new Coordinate(
                    new Point(40, 310),
                    1080,
                    AnchorPosition.Right
                )
            ));
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
