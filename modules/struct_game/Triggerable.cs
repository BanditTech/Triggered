using System;

namespace Triggered.modules.struct_game
{
    /// <summary>
    /// Provides a uniform system for Flask and Ability slots.
    /// I realized they often require the exact same properties.
    /// </summary>
    public class Triggerable
    {
        /// <summary>
        /// The slot of the triggerable
        /// </summary>
        protected int Slot;

        /// <summary>
        /// Assign the bound key for this triggerable slot.
        /// </summary>
        public string boundKey { get; set; } = string.Empty;

        /// <summary>
        /// Provide a public method for determining ready state.
        /// </summary>
        public bool IsReady { get { return !IsActive && IsAvailable; } }
        internal bool IsActive { get; set; } = false;
        internal bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Sets the duration of a triggerable in seconds.
        /// </summary>
        public float Duration { get; set; } = 6.0f;

        /// <summary>
        /// Determines the ending timestamp of a triggerable.
        /// </summary>
        public DateTime EndsAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Determine if the flasks
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
        /// Check if any slot has expired
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
