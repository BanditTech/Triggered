﻿namespace Triggered.modules.struct_game
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
        public float Life { get; set; } = 1f;

        /// <summary>
        /// The currently determined Mana.
        /// </summary>
        public float Mana { get; set; } = 1f;

        /// <summary>
        /// The currently determined Energy Shield.
        /// </summary>
        public float Shield { get; set; } = 1f;

        /// <summary>
        /// The currently determined Ward.
        /// </summary>
        public float Ward { get; set; } = 1f;

        /// <summary>
        /// The currently determined Rage.
        /// </summary>
        public float Rage { get; set; } = 1f;

        /// <summary>
        /// String array of all found buff samples.
        /// </summary>
        public string[] Buffs { get; set; } = new string[0];

        /// <summary>
        /// String array of all found debuff samples.
        /// </summary>
        public string[] Debuffs { get; set; } = new string[0];

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

        /// <summary>
        /// The current Ability states
        /// </summary>
        public Ability[] Abilities { get; set; } = {
            new Ability(1),
            new Ability(2),
            new Ability(3),
            new Ability(4),
            new Ability(5),
            new Ability(6),
            new Ability(7),
            new Ability(8)
        };

        /// <summary>
        /// The current Bonus Bar states
        /// </summary>
        public Ability[] BonusBar { get; set; } = {
            new Ability(1),
            new Ability(2),
            new Ability(3),
            new Ability(4),
            new Ability(5)
        };
    }
}
