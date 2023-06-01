﻿using System;
using System.Drawing;

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
        /// Produce a scaled position from an origin Coordinate and target Rectangle.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Point CalculatePoint(Coordinate origin, Rectangle rectangle)
        {
            Point anchorPoint = new();
            int centerX;
            int centerY;
            var position = ScaledPoint(origin, rectangle.Height);

            // Determine which edge to use as initial X, Y
            switch (origin.Anchor)
            {
                case AnchorPosition.Center:
                    centerX = rectangle.X + rectangle.Width / 2;
                    centerY = rectangle.Y + rectangle.Height / 2;
                    anchorPoint.X = centerX + position.X;
                    anchorPoint.Y = centerY + position.Y;
                    break;
                case AnchorPosition.Left:
                    centerY = rectangle.Y + rectangle.Height / 2;
                    anchorPoint.X = rectangle.X + position.X;
                    anchorPoint.Y = centerY + position.Y;
                    break;
                case AnchorPosition.Right:
                    centerY = rectangle.Y + rectangle.Height / 2;
                    anchorPoint.X = rectangle.X + rectangle.Width - position.X;
                    anchorPoint.Y = centerY + position.Y;
                    break;
                case AnchorPosition.Top:
                    centerX = rectangle.X + rectangle.Width / 2;
                    anchorPoint.X = centerX + position.X;
                    anchorPoint.Y = rectangle.Y + position.Y;
                    break;
                case AnchorPosition.TopLeft:
                    anchorPoint.X = rectangle.X + position.X;
                    anchorPoint.Y = rectangle.Y + position.Y;
                    break;
                case AnchorPosition.TopRight:
                    anchorPoint.X = rectangle.X + rectangle.Width - position.X;
                    anchorPoint.Y = rectangle.Y + position.Y;
                    break;
                case AnchorPosition.Bottom:
                    centerX = rectangle.X + rectangle.Width / 2;
                    anchorPoint.X = centerX + position.X;
                    anchorPoint.Y = rectangle.Y + rectangle.Height - position.Y;
                    break;
                case AnchorPosition.BottomLeft:
                    anchorPoint.X = rectangle.X + position.X;
                    anchorPoint.Y = rectangle.Y + rectangle.Height - position.Y;
                    break;
                case AnchorPosition.BottomRight:
                    anchorPoint.X = rectangle.X + rectangle.Width - position.X;
                    anchorPoint.Y = rectangle.Y + rectangle.Height - position.Y;
                    break;
                default:
                    throw new ArgumentException("Invalid anchor position");
            }
            
            // Ensure we return within the bounds
            ValidatePoint(ref anchorPoint,rectangle);
            return anchorPoint;
        }

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
            var end = CalculatePoint(origin.End,rectangle);
            Rectangle output = new();
            // Ensure that we begin from the top left corner
            output.X = start.X < end.X ? start.X : end.X;
            output.Y = start.Y < end.Y ? start.Y : end.Y;
            // Ensure we always calculate the correct value
            output.Width = Math.Abs(end.X - start.X) + 1;
            output.Height = Math.Abs(end.Y - start.Y) + 1;
            return output;
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
            if (!rectangle.Contains(point))
            {
                // Log discrepancy
                App.Log($"{point.X}, {point.Y} is outside of bounds ({rectangle.Left},{rectangle.Top}) ({rectangle.Right},{rectangle.Bottom})");
                // Adjust X coordinate
                if (point.X < rectangle.Left)
                    point.X = rectangle.Left;
                else if (point.X > rectangle.Right)
                    point.X = rectangle.Right;
                // Adjust Y coordinate
                if (point.Y < rectangle.Top)
                    point.Y = rectangle.Top;
                else if (point.Y > rectangle.Bottom)
                    point.Y = rectangle.Bottom;
                return false;
            }
            return true;
        }
    }
}
