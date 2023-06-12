using System.Numerics;

namespace Triggered.modules.options
{
    /// <summary>
    /// Options class for the Demo CV.
    /// </summary>
    public class Options_DemoCV : Options
    {
        /// <summary>
        /// Construct a default DemoCV options.
        /// </summary>
        public Options_DemoCV()
        {
            // Assign the name we will use to save the file
            Name = "DemoCV";
            RunDefault();
        }
        internal override void Default()
        {
            // OCR Values
            SetKey("OCR.Min", new Vector3(0.4f, 0f, 0f));
            SetKey("OCR.Max", new Vector3(0.5f, 1f, 1f));
            SetKey("OCR.X", 100);
            SetKey("OCR.Y", 100);
            SetKey("OCR.W", 100);
            SetKey("OCR.H", 100);
            // HSV Subset Values
            SetKey("filterSubsetHSVMin", new Vector3(0.4f, 0f, 0f));
            SetKey("filterSubsetHSVMax", new Vector3(0.5f, 1f, 1f));
            SetKey("filterSubsetX", 100);
            SetKey("filterSubsetY", 100);
            SetKey("filterSubsetW", 100);
            SetKey("filterSubsetH", 100);
            SetKey("filterSubsetPercentage", 1f);
            // Shape Detection
            SetKey("rectangleArea", 250);
            SetKey("cannyThreshold", 180.0f);
            SetKey("cannyThresholdLinking", 120.0f);
        }
    }
}
