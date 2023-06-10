using Emgu.CV;
using System;
using System.Diagnostics;
using static Triggered.modules.wrapper.OpenCV;

namespace Triggered.modules.wrapper
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
        public static Mat Vision { get; set; }
        /// <summary>
        /// Logical processing.
        /// </summary>
        public static void Processing()
        {
            var hwnd = Windows.GetProcessHandle();
            if (hwnd != IntPtr.Zero)
            {
                if (Vision != null)
                    Vision.Dispose();
                Vision = GetWindowMat(hwnd);
            }
        }
    }
}
