using Emgu.CV;
using Emgu.CV.OCR;
using System;
using System.Collections.Generic;
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
        private static float Life
        {
            get { return App.Player.Life; }
            set { App.Player.Life = value; }
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
            // We have passed our threshold of input
            NeuralCascade();
        }
        
        /// <summary>
        /// Make observation of our target.
        /// </summary>
        /// <returns></returns>
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
        
        /// <summary>
        /// Initiate neural activity on threshold activation.
        /// </summary>
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
        
        #region Senses
        private static List<Sensation> Senses = new List<Sensation>();
        public enum Sense
        {
            Life,
            Mana,
            Shield,
            Ward,
            Rage
        }
        public class Sensation
        {
            public Sense Type { get; set; }
            public Action<object> Handler { get; set; }
            public Sensation(Sense type, Action<object> handler)
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
                ScaledRectangle origin = App.Options.Locations.GetKey<ScaledRectangle>($"Resource.{propertyName}");
                var targetRect = User32.GetWindowRectangle(targetProcess);
                var area = origin.Relative(targetRect);



                float conclusion = default;
                property.SetValue(null, conclusion);
            }
        }
        #endregion
    }
}
