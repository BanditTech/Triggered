using System;
using System.Drawing;
using static Triggered.modules.wrapper.User32;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Identifies a position in the game space.
    /// Uses a scale defined by the destination height / origin height.
    /// </summary>
    public static class PointScaler
    {
        /// <summary>
        /// Represents a measurement value with associated height.
        /// </summary>
        public struct Measurement
        {
            /// <summary>
            /// Gets or sets the value of the measurement.
            /// </summary>
            public int Value { get; set; }

            /// <summary>
            /// Gets or sets the height associated with the measurement.
            /// </summary>
            public int Height { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Measurement"/> struct.
            /// </summary>
            /// <param name="value">The value of the measurement.</param>
            /// <param name="height">The height associated with the measurement.</param>
            public Measurement(int value, int height)
            {
                Value = value;
                Height = height;
            }
        }

        /// <summary>
        /// Defines a sampled location to be used in other resolutions.
        /// </summary>
        public struct Coordinate
        {
            /// <summary>
            /// Defines the origin Height to use for scaling the position.
            /// </summary>
            public int Height { get; set; }

            /// <summary>
            /// Designates the X and Y position to use in relation to the anchor.
            /// </summary>
            public Point Point { get; set; }

            /// <summary>
            /// Determine which position to set as (x 0, y 0).
            /// </summary>
            public AnchorPosition Anchor { get; set; }

            /// <summary>
            /// Constructs the Coordinate.
            /// </summary>
            /// <param name="point">
            /// The X, Y position
            /// </param>
            /// <param name="height">
            /// The origin position's window height
            /// </param>
            /// <param name="anchor">
            /// Determine which side to begin from.
            /// </param>
            public Coordinate(Point point, int height, AnchorPosition anchor)
            {
                Point = point;
                Height = height;
                Anchor = anchor;
            }
        }

        /// <summary>
        /// Represents a rectangle with scaled coordinates.
        /// </summary>
        public struct ScaledRectangle
        {
            /// <summary>
            /// The starting coordinate of the rectangle.
            /// </summary>
            public Coordinate Start { get; set; }

            /// <summary>
            /// The ending coordinate of the rectangle.
            /// </summary>
            public Coordinate End { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ScaledRectangle"/> struct.
            /// </summary>
            /// <param name="start">The starting coordinate of the rectangle.</param>
            /// <param name="end">The ending coordinate of the rectangle.</param>
            public ScaledRectangle(Coordinate start, Coordinate end)
            {
                Start = start;
                End = end;
            }

            /// <summary>
            /// Define the calculated Width
            /// </summary>
            public readonly int Width { get => Math.Abs(Start.Point.X - End.Point.X) + 1; }

            /// <summary>
            /// Define the calculated Height
            /// </summary>
            public readonly int Height { get => Math.Abs(Start.Point.Y - End.Point.Y) + 1; }

            public Rectangle Relative(Rectangle target)
            {
                return CalculateRectangle(this,target);
            }
        }

        /// <summary>
        /// Determine the starting location for X, Y
        /// </summary>
        public enum AnchorPosition
        {
            /// <summary>
            /// Begins from the center of the rect.
            /// </summary>
            Center,
            /// <summary>
            /// Begin in the left at the center.
            /// </summary>
            Left,
            /// <summary>
            /// Begin in the right at the center.
            /// </summary>
            Right,
            /// <summary>
            /// Begin in the top at the center.
            /// </summary>
            Top,
            /// <summary>
            /// Begin in the bottom at the center.
            /// </summary>
            Bottom,
            /// <summary>
            /// Begin in the top left corner (standard).
            /// </summary>
            TopLeft,
            /// <summary>
            /// Begin in the top right corner.
            /// </summary>
            TopRight,
            /// <summary>
            /// Begin in the bottom left corner.
            /// </summary>
            BottomLeft,
            /// <summary>
            /// Begin in the bottom right corner.
            /// </summary>
            BottomRight
        }

        /// <summary>
        /// Retrieves the mouse coordinate relative to the specified target window and anchor position.
        /// </summary>
        /// <param name="targetWindow">The handle of the target window.</param>
        /// <param name="anchorPosition">The anchor position relative to the window.</param>
        /// <returns>The mouse coordinate as a Coordinate object.</returns>
        public static Coordinate GetMouseCoordinate(IntPtr targetWindow, AnchorPosition anchorPosition)
        {
            Point point = GetRelativeMousePosition(targetWindow);
            Rectangle rectangle = GetWindowRectangle(targetWindow);
            return CalculateCoordinate(point,rectangle,anchorPosition);
        }

        /// <summary>
        /// Calculates the coordinate based on the relative point, rectangle, and anchor position.
        /// </summary>
        /// <param name="relativePoint">The relative point to calculate the coordinate from.</param>
        /// <param name="rectangle">The rectangle to calculate the coordinate within.</param>
        /// <param name="anchorPosition">The anchor position relative to the rectangle.</param>
        /// <returns>The calculated coordinate as a Coordinate object.</returns>
        public static Coordinate CalculateCoordinate(Point relativePoint, Rectangle rectangle, AnchorPosition anchorPosition)
        {
            int x = 0;
            int y = 0;
            switch (anchorPosition)
            {
                case AnchorPosition.TopLeft:
                    x = relativePoint.X;
                    y = relativePoint.Y;
                    break;
                case AnchorPosition.Center:
                    x = relativePoint.X - (rectangle.Width / 2);
                    y = relativePoint.Y - (rectangle.Height / 2);
                    break;
                case AnchorPosition.Left:
                    x = relativePoint.X;
                    y = relativePoint.Y - (rectangle.Height / 2);
                    break;
                case AnchorPosition.Right:
                    x = rectangle.Width - relativePoint.X;
                    y = relativePoint.Y - (rectangle.Height / 2);
                    break;
                case AnchorPosition.Top:
                    x = relativePoint.X - (rectangle.Width / 2);
                    y = relativePoint.Y;
                    break;
                case AnchorPosition.Bottom:
                    x = relativePoint.X - (rectangle.Width / 2);
                    y = rectangle.Height - relativePoint.Y;
                    break;
                case AnchorPosition.BottomLeft:
                    x = relativePoint.X;
                    y = rectangle.Height - relativePoint.Y;
                    break;
                case AnchorPosition.TopRight:
                    x = rectangle.Width - relativePoint.X;
                    y = relativePoint.Y;
                    break;
                case AnchorPosition.BottomRight:
                    x = rectangle.Width - relativePoint.X;
                    y = rectangle.Height - relativePoint.Y;
                    break;
                default:
                    break;
            }

            return new Coordinate(new Point(x, y), rectangle.Height, anchorPosition);
        }

        /// <summary>
        /// Calculates the anchor position based on the specified rectangle and anchor position.
        /// </summary>
        /// <param name="rectangle">The rectangle to calculate the anchor position from.</param>
        /// <param name="anchorPosition">The anchor position to calculate.</param>
        /// <returns>The anchor position as a Point.</returns>
        public static Point GetAnchorPositionFromRectangle(Rectangle rectangle, AnchorPosition anchorPosition)
        {
            int x = 0;
            int y = 0;

            switch (anchorPosition)
            {
                case AnchorPosition.Center:
                    x = rectangle.Width / 2;
                    y = rectangle.Height / 2;
                    break;
                case AnchorPosition.Left:
                    x = 0;
                    y = rectangle.Height / 2;
                    break;
                case AnchorPosition.Right:
                    x = rectangle.Width;
                    y = rectangle.Height / 2;
                    break;
                case AnchorPosition.Top:
                    x = rectangle.Width / 2;
                    y = 0;
                    break;
                case AnchorPosition.Bottom:
                    x = rectangle.Width / 2;
                    y = rectangle.Height;
                    break;
                case AnchorPosition.TopLeft:
                    x = 0;
                    y = 0;
                    break;
                case AnchorPosition.TopRight:
                    x = rectangle.Width;
                    y = 0;
                    break;
                case AnchorPosition.BottomLeft:
                    x = 0;
                    y = rectangle.Height;
                    break;
                case AnchorPosition.BottomRight:
                    x = rectangle.Width;
                    y = rectangle.Height;
                    break;
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Produce a scaled position from an origin Coordinate and target Rectangle.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Point CalculatePoint(Coordinate origin, Rectangle rectangle)
        {
            Point anchorPoint = GetAnchorPositionFromRectangle(rectangle,origin.Anchor);
            var position = ScaledPoint(origin, rectangle.Height);

            // Determine which edge to use as initial X, Y
            switch (origin.Anchor)
            {
                case AnchorPosition.Center:
                case AnchorPosition.Left:
                case AnchorPosition.Top:
                case AnchorPosition.TopLeft:
                    anchorPoint.X += position.X;
                    anchorPoint.Y += position.Y;
                    break;
                case AnchorPosition.Right:
                case AnchorPosition.TopRight:
                    anchorPoint.X -= position.X;
                    anchorPoint.Y += position.Y;
                    break;
                case AnchorPosition.Bottom:
                case AnchorPosition.BottomLeft:
                    anchorPoint.X += position.X;
                    anchorPoint.Y -= position.Y;
                    break;
                case AnchorPosition.BottomRight:
                    anchorPoint.X -= position.X;
                    anchorPoint.Y -= position.Y;
                    break;
                default:
                    throw new ArgumentException("Invalid anchor position");
            }

            // Ensure we return within the bounds
            ValidatePoint(ref anchorPoint,rectangle);
            return anchorPoint;
        }

        /// <summary>
        /// Calculates a scaled measurement value based on a given measurement and reference rectangle.
        /// </summary>
        /// <param name="measurement">The measurement value and associated height.</param>
        /// <param name="rectangle">The reference rectangle.</param>
        /// <returns>The calculated scaled measurement value.</returns>
        public static int CalculateMeasurement(Measurement measurement, Rectangle rectangle)
        {
            var scale = rectangle.Height / measurement.Height;
            return ScaledValue(measurement.Value,scale);
        }

        /// <summary>
        /// Calculates a new rectangle based on a scaled origin rectangle and a reference rectangle.
        /// </summary>
        /// <param name="origin">The scaled origin rectangle.</param>
        /// <param name="rectangle">The reference rectangle.</param>
        /// <returns>A new calculated rectangle.</returns>
        public static Rectangle CalculateRectangle(ScaledRectangle origin, Rectangle rectangle)
        {
            // Begin with validated points
            var start = CalculatePoint(origin.Start,rectangle);
            var end = CalculatePoint(origin.End, rectangle);
            // Ensure that we begin from the top left corner
            var X = start.X < end.X ? start.X : end.X;
            var Y = start.Y < end.Y ? start.Y : end.Y;
            return new(X, Y, origin.Width, origin.Height);
        }

        /// <summary>
        /// Scales the origin position relative to target height.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Point ScaledPoint(Coordinate origin, int height)
        {
            float scale = height / (float)origin.Height;
            int scaledX = ScaledValue(origin.Point.X, scale);
            int scaledY = ScaledValue(origin.Point.Y, scale);
            return new Point(scaledX, scaledY);
        }

        /// <summary>
        /// Multiply a value by a scale.
        /// </summary>
        /// <param name="value">
        /// A value representing a position
        /// </param>
        /// <param name="scale">
        /// Typically this would be the newHeight / origin.Height
        /// </param>
        /// <returns></returns>
        public static int ScaledValue(int value, float scale)
        {
            float scaledValue = scale * value;
            return (int)scaledValue;
        }

        /// <summary>
        /// Ensures a point remains within the bounds of the rectangle
        /// </summary>
        /// <param name="point"></param>
        /// <param name="rectangle"></param>
        public static bool ValidatePoint(ref Point point, Rectangle rectangle)
        {
            bool smallX = point.X < 0;
            bool smallY = point.Y < 0;
            bool bigX = point.X > rectangle.Width;
            bool bigY = point.Y > rectangle.Height;

            bool valid = !smallX && !smallY && !bigX && !bigY;

            if (!valid)
            {
                App.Log($"{point.X}, {point.Y} is outside of bounds ({rectangle.Width},{rectangle.Height})");
                // Adjust X coordinate
                if (smallX)
                    point.X = 0;
                else if (bigX)
                    point.X = rectangle.Width;
                // Adjust Y coordinate
                if (smallY)
                    point.Y = rectangle.Top;
                else if (bigY)
                    point.Y = rectangle.Height;
            }

            return valid;
        }
    }
}
