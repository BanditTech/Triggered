namespace Triggered.modules.struct_game
{
    using System;

    /// <summary>
    /// Represents each individual flask.
    /// </summary>
    public class Flask : Triggerable
    {
        /// <summary>
        /// The slot of the Flask
        /// </summary>
        protected int Slot;

        /// <summary>
        /// Determines the active state of a flask slot.
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Determines the ending timestamp of a flask.
        /// </summary>
        public DateTime EndsAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Determines the flask availability.
        /// </summary>
        public bool HasCharges { get; set; } = true;

        /// <summary>
        /// Sets the duration of a flask in seconds.
        /// </summary>
        public float Duration { get; set; } = 6.0f;

        /// <summary>
        /// Assign the bound key for this slot.
        /// </summary>
        public string boundKey { get; set; } = string.Empty;

        /// <summary>
        /// Assign a slot to the Flask to produce it.
        /// </summary>
        /// <param name="slot"></param>
        public Flask(int slot)
        {
            Slot = slot;
        }

        /// <summary>
        /// Checks if is not active, and has charges.
        /// </summary>
        /// <returns>If the flask is ready to use.</returns>
        public bool IsReady()
        {
            return !IsActive && HasCharges;
        }

        /// <summary>
        /// Determine if the flasks
        /// </summary>
        public void Fire()
        {
            if (!IsReady())
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
        }
    }
}
