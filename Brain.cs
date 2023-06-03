namespace Triggered
{
    public static class Brain
    {
        private static float Health => App.Player.Health;
        private static float Mana => App.Player.Mana;
        private static float EnergyShield => App.Player.EnergyShield;
        private static string Location => App.Player.Location;


        static Brain()
        {
            // Give some thought
        }

        /// <summary>
        /// Logical processing.
        /// </summary>
        public static void Process()
        {
            // Ponder our logic
        }
    }
}
