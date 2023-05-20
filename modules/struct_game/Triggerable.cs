using System;

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
