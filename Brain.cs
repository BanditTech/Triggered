using Emgu.CV;
using Emgu.CV.OCR;
using System;
using System.Diagnostics;
using System.IO;
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

        private static readonly Tesseract OCR = new();
        private static IntPtr targetProcess = IntPtr.Zero;

        /// <summary>
        /// Finish initiating our Tesseract engine
        /// </summary>
        static Brain()
        {
            OCR.Init(Path.Join(AppContext.BaseDirectory, "lib", "Tesseract", "tessdata"), "fast", OcrEngineMode.LstmOnly);
        }

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
            if (targetProcess == IntPtr.Zero)
                targetProcess = Windows.GetProcessHandle();
            if (targetProcess != IntPtr.Zero)
            {
                if (Vision != null)
                    Vision.Dispose();
                Vision = GetWindowMat(targetProcess);
            }
        }
    }
}
