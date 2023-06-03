namespace Triggered
{
    /// <summary>
    /// Logical processing organ.
    /// </summary>
    public static class Brain
    {
        private static float Health => App.Player.Health;
        private static float Mana => App.Player.Mana;
        private static float EnergyShield => App.Player.EnergyShield;
        private static string Location => App.Player.Location;

        /// <summary>
        /// Logical processing.
        /// </summary>
        public static void Process()
        {
            // Ponder our logic
        }
    }
}
