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
            RunDefault();
        }
        internal override void Default()
        {
            // Resource Area
            SetKey("Resource.Life", new ScaledRectangle(
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
            ), "Life Min/Max text area");
            SetKey("Resource.Mana", new ScaledRectangle(
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
            ), "Mana Min/Max text area");
            SetKey("Resource.Shield", new ScaledRectangle(
                new Coordinate(
                    new Point(250, 250),
                    1080,
                    AnchorPosition.Left
                ),
                new Coordinate(
                    new Point(40, 310),
                    1080,
                    AnchorPosition.Left
                )
            ), "Energy Shield Min/Max text area");
            SetKey("Resource.Ward", new ScaledRectangle(
                new Coordinate(
                    new Point(250, 250),
                    1080,
                    AnchorPosition.Left
                ),
                new Coordinate(
                    new Point(40, 310),
                    1080,
                    AnchorPosition.Left
                )
            ), "Ward Min/Max text area");

            // Panel Coordinate
            SetKey("Panel.Main", new Coordinate(
                new Point(250, 250),
                1080,
                AnchorPosition.BottomLeft
            ), "Main game interface");
            SetKey("Panel.Inventory.0", new Coordinate(
                new Point(250, 250),
                1080,
                AnchorPosition.Right
            ), "Inventory Coord 1");
            SetKey("Panel.Inventory.1", new Coordinate(
                new Point(250, 250),
                1080,
                AnchorPosition.Right
            ), "Inventory Coord 2");
            SetKey("Panel.Stash.0", new Coordinate(
                new Point(250, 250),
                1080,
                AnchorPosition.Left
            ), "Stash Coord 1");
            SetKey("Panel.Stash.1", new Coordinate(
                new Point(250, 250),
                1080,
                AnchorPosition.Left
            ), "Stash Coord 2");

            // Measurements
            SetKey("Measure.SlotSize",
                new Measurement(20, 1080),
                "Slot Size");

            TrimNullValues(keyList);
            // Reset the changed flag to avoid saving again
            _changed = false;
        }
    }
}
