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
                Senses.Add(new(sense, new(Analysis)));
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

        /// <summary>
        /// Represents different in-game resources (senses) in our Brain.
        /// </summary>
        public enum Sense
        {
            /// <summary>
            /// Life resource.
            /// </summary>
            Life,

            /// <summary>
            /// Mana resource.
            /// </summary>
            Mana,

            /// <summary>
            /// Shield resource.
            /// </summary>
            Shield,

            /// <summary>
            /// Ward resource.
            /// </summary>
            Ward,

            /// <summary>
            /// Rage resource.
            /// </summary>
            Rage,
            /// <summary>
            /// Location Tracking.
            /// </summary>
            Location
        }

        /// <summary>
        /// Represents a sensation associated with a specific sense in a brain system.
        /// </summary>
        public class Sensation
        {
            /// <summary>
            /// Gets or sets the type of sense associated with the sensation.
            /// </summary>
            public Sense Type { get; set; }

            /// <summary>
            /// Gets or sets the handler action that processes the sensation.
            /// </summary>
            public Action<object> Handler { get; set; }

            /// <summary>
            /// Initializes a new instance of the Sensation class with the specified sense type and handler action.
            /// </summary>
            /// <param name="type">The type of sense associated with the sensation.</param>
            /// <param name="handler">The handler action that processes the sensation.</param>
            public Sensation(Sense type, Action<object> handler)
            {
                Type = type;
                Handler = handler;
            }
        }

        private static void Analysis(object data)
        {
            if (data is Sense sense)
            {
                if (sense == Sense.Location)
                    Proprioception();
                else
                    Cognition(sense);
            }
        }

        private static void Cognition(Sense sense)
        {
            // Get the name of the property based on the Sensation type
            string propertyName = sense.ToString();
            PropertyInfo property = typeof(Brain).GetProperty(propertyName);
            ScaledRectangle origin = App.Options.Locations.GetKey<ScaledRectangle>($"Resource.{propertyName}");
            var targetRect = User32.GetWindowRectangle(targetProcess);
            var area = origin.Relative(targetRect);
            Mat regionOfInterest = new Mat(Vision, area);
            // come up with the conclusion from the region of interest
            regionOfInterest.Dispose();
            float conclusion = default;
            property.SetValue(null, conclusion);
        }

        private static void Proprioception()
        {
            // load the file object if not loaded
            // read to the end of file on init
            // Store the file pos if required by C#
            // Each call reads from Pos to EOF
            // Split the content by /r/n
            // Regex match in series to determine location change
            // Same for level up text
            // See if there is any other useful data from the client.txt
        }
        #endregion
    }
}
