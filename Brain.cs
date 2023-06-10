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
        private static float Shield => App.Player.Shield;
        private static float Ward => App.Player.Ward;
        private static float Rage => App.Player.Rage;
        private static string Location => App.Player.Location;

        /// <summary>
        /// In the realm of machine vision, algorithms unveil hidden truths,
        /// Perceiving details through unblinking eyes, where beauty and knowledge fuse.
        /// With digital sight, they paint visual tales,
        /// Where innovation thrives and wisdom prevails.
        /// In this realm of code and light, a symphony unfolds,
        /// Pixels dance, revealing insights untold.
        /// </summary>
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
