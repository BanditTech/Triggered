using Emgu.CV;
using Emgu.CV.OCR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using static Triggered.modules.wrapper.OpenCV;
using static Triggered.modules.wrapper.PointScaler;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Logical processing organ.
    /// </summary>
    public static class Brain
    {
        private static float Health
        {
            get { return App.Player.Health; }
            set { App.Player.Health = value; }
        }

        private static float Mana
        {
            get { return App.Player.Mana; }
            set { App.Player.Mana = value; }
        }

        private static float Shield
        {
            get { return App.Player.Shield; }
            set { App.Player.Shield = value; }
        }

        private static float Ward
        {
            get { return App.Player.Ward; }
            set { App.Player.Ward = value; }
        }

        private static float Rage
        {
            get { return App.Player.Rage; }
            set { App.Player.Rage = value; }
        }

        private static string Location
        {
            get { return App.Player.Location; }
            set { App.Player.Location = value; }
        }

        private static readonly Tesseract OCR = new();
        private static IntPtr targetProcess = IntPtr.Zero;
        private static List<Feeling> Senses = new List<Feeling>();

        /// <summary>
        /// Finish initiating our Tesseract engine
        /// </summary>
        static Brain()
        {
            OCR.Init(Path.Join(AppContext.BaseDirectory, "lib", "Tesseract", "tessdata"), "fast", OcrEngineMode.LstmOnly);
            foreach (Sense sense in Enum.GetValuesAsUnderlyingType(typeof(Sense)))
                Senses.Add(new(sense, new(InterpretSense)));
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
            if (!Observe())
                return;
            NeuralCascade();
        }
        private static bool Observe()
        {
            if (targetProcess == IntPtr.Zero)
                targetProcess = Windows.GetProcessHandle();
            if (targetProcess != IntPtr.Zero)
            {
                if (Vision != null)
                    Vision.Dispose();
                Vision = GetWindowMat(targetProcess);
                return true;
            }
            return false;
        }
        private static void NeuralCascade()
        {
            List<Task> tasks = new List<Task>();

            foreach (var sense in Senses)
            {
                Task task = Task.Run(() =>
                {
                    sense.Handler.Invoke(sense.Type);
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }
        public enum Sense
        {
            Life,
            Mana,
            Shield,
            Ward,
            Rage
        }
        public class Feeling
        {
            public Sense Type { get; set; }
            public Action<object> Handler { get; set; }
            public Feeling(Sense type, Action<object> handler)
            {
                Type = type;
                Handler = handler;
            }
        }
        private static void InterpretSense(object data)
        {
            if (data is Sense type)
            {
                // Get the name of the property based on the FeelingType
                string propertyName = type.ToString();
                PropertyInfo property = typeof(Brain).GetProperty(propertyName);
                var newValue = property.GetValue(null);
                ScaledRectangle area = App.Options.Locations.GetKey<ScaledRectangle>($"Resource.{propertyName}");
                property.SetValue(null,newValue);
            }
        }
    }
}
