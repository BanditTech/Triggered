namespace Triggered.modules.struct_game
{
    /// <summary>
    /// Represents the Player status
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The player's current location.
        /// </summary>
        public string Location { get; set; } = "";

        /// <summary>
        /// The currently determined health.
        /// </summary>
        public float Health { get; set; } = 1f;

        /// <summary>
        /// The currently determined Mana.
        /// </summary>
        public float Mana { get; set; } = 1f;

        /// <summary>
        /// The currently determined Energy Shield.
        /// </summary>
        public float EnergyShield { get; set; } = 1f;

        /// <summary>
        /// The current Flask states
        /// </summary>
        public Flask[] Flasks { get; set; } = {
            new Flask(1),
            new Flask(2),
            new Flask(3),
            new Flask(4),
            new Flask(5),
        };
    }
}
