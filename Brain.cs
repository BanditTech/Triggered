using Emgu.CV;
using Emgu.CV.OCR;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using static Triggered.modules.wrapper.OpenCV;
using static Triggered.modules.wrapper.PointScaler;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Linq;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Logical processing organ.
    /// </summary>
    public static class Brain
    {
        /// <summary>
        /// With digital lenses, they compose visual tales,
        /// Where innovation thrives and wisdom never fails.
        /// </summary>
        public static Mat Vision { get; set; }
        private static IntPtr targetProcess = IntPtr.Zero;


        /// <summary>
        /// Finalize the components of the Brain.
        /// Add Sensations to the Senses list for each sense.
        /// Initiate the Tesseract engine with its language model.
        /// </summary>
        static Brain()
        {
            foreach (Sense sense in Enum.GetValuesAsUnderlyingType(typeof(Sense)))
            {
                Senses.Add(new(sense, new(Analysis)));
                if (sense == Sense.Location)
                    continue;
                // Construct our Kalman filters
                Recollection[sense] = new();
                Recollection[sense]["current"] = new();
                Recollection[sense]["maximum"] = new();

                Translate[sense] = new Tesseract();
                Translate[sense].Init(Path.Join(AppContext.BaseDirectory, "lib", "Tesseract", "tessdata"), "fast", OcrEngineMode.LstmOnly);
            }
        }

        #region Logical Processes
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
        #endregion

        #region Senses
        /// <summary>
        /// Local reference to Life
        /// </summary>
        public static float Life
        {
            get { return App.Player.Life; }
            set { App.Player.Life = value; }
        }

        /// <summary>
        /// Local reference to Mana
        /// </summary>
        public static float Mana
        {
            get { return App.Player.Mana; }
            set { App.Player.Mana = value; }
        }

        /// <summary>
        /// Local reference to Shield
        /// </summary>
        public static float Shield
        {
            get { return App.Player.Shield; }
            set { App.Player.Shield = value; }
        }

        /// <summary>
        /// Local reference to Ward
        /// </summary>
        public static float Ward
        {
            get { return App.Player.Ward; }
            set { App.Player.Ward = value; }
        }

        /// <summary>
        /// Local reference to Rage
        /// </summary>
        public static float Rage
        {
            get { return App.Player.Rage; }
            set { App.Player.Rage = value; }
        }

        /// <summary>
        /// Local reference to Location
        /// </summary>
        public static string Location
        {
            get { return App.Player.Location; }
            set { App.Player.Location = value; }
        }

        private static List<Sensation> Senses = new();
        private static readonly Dictionary<Sense, Dictionary<string, KalmanFilter>> Recollection = new();
        private static readonly Dictionary<Sense, Tesseract> Translate = new();

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

        /// <summary>
        /// Determine the type of Sense and how to comprehend it
        /// </summary>
        /// <param name="data"></param>
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

        /// <summary>
        /// Comprehension of a Sense using our Vision.
        /// </summary>
        /// <param name="sense"></param>
        private static void Cognition(Sense sense)
        {
            // Gather internal resources
            KalmanFilter kalmanCurrent = Recollection[sense]["current"];
            KalmanFilter kalmanMaximum = Recollection[sense]["maximum"];
            Vector3 lower = App.Options.Colors.GetKey<Vector3>("Resource.Min");
            Vector3 upper = App.Options.Colors.GetKey<Vector3>("Resource.Max");
            bool show = App.Options.Colors.GetKey<bool>("Resource.ShowTest");
            Tesseract translator = Translate[sense];

            // Get the name of the property based on the Sensation type
            string propertyName = sense.ToString();
            PropertyInfo property = typeof(Brain).GetProperty(propertyName);
            ScaledRectangle origin = App.Options.Locations.GetKey<ScaledRectangle>($"Resource.{propertyName}");
            Rectangle targetRect = User32.GetWindowRectangle(targetProcess);
            Rectangle area = origin.Relative(targetRect);

            using Mat regionOfInterest = new(Vision, area);
            using Mat filtered = GetFilteredMat(regionOfInterest, lower, upper, false, true);
            using Mat masked = GetBlackWhiteMaskMat(filtered);
            using Mat invert = new();
            // We now have a masked image which represents the matching pixels
            if (show && sense == Sense.Life)
            {
                using Mat copied = new();
                regionOfInterest.CopyTo(copied, masked);
                DisplayImage("Resource Test", copied);
            }
            // Invert to black text on white background
            CvInvoke.BitwiseNot(masked, invert);
            translator.SetImage(invert);
            string result = translator.GetUTF8Text().Trim();

            if (result == "")
                return;

            // Remove small discrepencies
            result = Regex.Replace(result,   "  ", " ");
            result = Regex.Replace(result, "[,.]",  "");
            result = Regex.Replace(result,    "{", "/");
            result = Regex.Replace(result,   "//", "/");

            string[] split = result.Split("/",StringSplitOptions.TrimEntries);
            // Without a divisor we do not have our two parts, we have no way to continue
            if (split.Length != 2)
                return;
            // We need both sides of the fraction to contain only digits
            if (!split[0].All(char.IsDigit) || !split[1].All(char.IsDigit)) 
                return;

            float current = float.Parse(split[0]);
            float maximum = float.Parse(split[1]);

            // Something is wrong and we have incorrect data
            if (current > maximum)
                return;

            float filteredCurrent = kalmanCurrent.Filter(current);
            float filteredMaximum = kalmanMaximum.Filter(maximum);

            float conclusion = filteredCurrent / filteredMaximum;
            App.Log($"Cognition reached a conclusion of {conclusion} for {propertyName}",0);
            property.SetValue(null, conclusion);
        }

        /// <summary>
        /// Gain perception of our location without sight.
        /// </summary>
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
