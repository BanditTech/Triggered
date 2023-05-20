using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Triggered.modules.struct_game
{
    /// <summary>
    /// Provides a uniform system for Flask and Ability slots.
    /// I realized they often require the exact same properties.
    /// </summary>
    public abstract class Triggerable
    {
        /// <summary>
        /// The slot of the triggerable
        /// </summary>
        public int Slot { get; set; }

        /// <summary>
        /// Assign the bound key for this triggerable slot.
        /// </summary>
        public string BoundKey { get; set; }

        /// <summary>
        /// Determine the position to check for availability.
        /// Available options: top, bottom, left, right, center
        /// Available ending math: +#x / -#x or +#y / -#y
        /// </summary>
        public string Anchor { get; set; } = "center";
        internal Point DetermineCenterOffset()
        {
            Point offset = new(0,0);
            var parts = Anchor.Split(" ");

            if (parts[0].ToLower() == "top" )
                offset.Y -= 20;
            else if (parts[0].ToLower() == "bottom" )
                offset.Y += 20;
            else if (parts[0].ToLower() == "left" )
                offset.X -= 20;
            else if (parts[0].ToLower() == "right" )
                offset.X += 20;
            else if (parts[0].ToLower() == "center" && parts[1].ToLower() == "left")
                offset.X -= 10;
            else if (parts[0].ToLower() == "center" && parts[1].ToLower() == "right")
                offset.X += 10;
            else if (parts[0].ToLower() == "center" && parts[1].ToLower() == "top")
                offset.Y -= 10;
            else if (parts[0].ToLower() == "center" && parts[1].ToLower() == "bottom")
                offset.Y += 10;

            if (parts[0].ToLower() != "center")
            {
                if (parts[1].ToLower() == "left")
                    offset.X -= 20;
                else if (parts[1].ToLower() == "right")
                    offset.X += 20;
                else if (parts[1].ToLower() == "top")
                    offset.Y -= 20;
                else if (parts[1].ToLower() == "bottom")
                    offset.Y += 20;
                else if (parts[1].ToLower() == "center" && parts[2].ToLower() == "left")
                    offset.X -= 10;
                else if (parts[1].ToLower() == "center" && parts[2].ToLower() == "right")
                    offset.X += 10;
                else if (parts[1].ToLower() == "center" && parts[2].ToLower() == "top")
                    offset.Y -= 10;
                else if (parts[1].ToLower() == "center" && parts[2].ToLower() == "bottom")
                    offset.Y += 10;
            }

            var offsetGroups = parts.SelectMany(part => Regex.Matches(part, @"([+-])(\d+)([xXyY])").Cast<Match>().Select(match => match.Groups));

            foreach (var groups in offsetGroups)
            {
                char operation = groups[1].Value[0];
                int value = int.Parse(groups[2].Value);
                char dimension = char.ToLower(groups[3].Value[0]);

                AddSubtractFromPoint(operation, value, dimension, ref offset);
            }

            return offset;
        }

        private void AddSubtractFromPoint(char operation, int value, char dimension, ref Point offset)
        {
            if (operation == '-')
                value = value * -1;
            if (dimension == 'x')
                offset.X += value;
            else
                offset.Y += value;
        }

        /// <summary>
        /// Sets the duration of a triggerable in seconds.
        /// </summary>
        public float Duration { get; set; } = 1f;

        /// <summary>
        /// Set the expiration timestamp.
        /// Uses the Now timestamp + Duration.
        /// </summary>
        public DateTime EndsAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Provide a public property for checking ready state.
        /// </summary>
        public bool IsReady { get { return !IsActive && IsAvailable; } }
        internal bool IsActive { get; set; } = false;
        internal bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Determine if the triggerable can fire
        /// </summary>
        public void Fire()
        {
            if (!IsReady)
                return;
            IsActive = true;
            EndsAt = EndsAt.AddSeconds(Duration);
            // Fire boundKey or add it to keystroke manager
        }

        /// <summary>
        /// Check if this Slot has expired
        /// </summary>
        public void Check()
        {
            if (IsActive && DateTime.Now >= EndsAt)
            {
                IsActive = false;
            }
            // Check for charges
            if (!IsAvailable && !IsActive)
            {
                // Add logic to verify if the slot is available
                IsAvailable = true;
            }
        }
    }
}
