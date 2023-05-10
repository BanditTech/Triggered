using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Drawing;
using System.Windows.Forms;
using ImGuiNET;
using System.Linq;
using System.Collections.Immutable;
using Emgu.CV.Util;

namespace Triggered
{
    public static class demoCV
    {
        public static void ShowBlue()
        {
            //Create a 3 channel image of 400x200
            using (Mat img = new Mat(200, 400, DepthType.Cv8U, 3))
            {
                img.SetTo(new Bgr(255, 0, 0).MCvScalar); // set it to Blue color

                //Draw "Hello, world." on the image using the specific font
                CvInvoke.PutText(
                   img,
                   "Hello,",
                   new System.Drawing.Point(10, 80),
                   FontFace.HersheyComplex,
                   3,
                   new Bgr(0, 255, 0).MCvScalar,
                   2);
                CvInvoke.PutText(
                   img,
                   "world",
                   new System.Drawing.Point(10, 160),
                   FontFace.HersheyComplex,
                   3,
                   new Bgr(0, 255, 0).MCvScalar,
                   2);
                var style = false;
                if (style)
                {
                    CvInvoke.Imshow("Test Window", img);
                    CvInvoke.WaitKey();
                }
                else
                {
                    //Show the image using ImageViewer from Emgu.CV.UI
                    ImageViewer.Show(img, "Test Window");
                }
            }
        }
        public static void Capture()
        {
            // Construct our variables with `using` when possible
            int direction;
            var rnd = new Random();
            string win1 = "Primary Screen Capture";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            Rectangle drawBox = new Rectangle(100, 100, 400, 400);
            using Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using Mat frame = new Mat(screenBounds.Height / 2, screenBounds.Width / 2, DepthType.Cv8U, 3);
            using Mat screenMat = new Mat();
            // We create our named window
            CvInvoke.NamedWindow(win1);

            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Capture the primary screen and convert it to a System.Drawing.Bitmap object
                using (Graphics graphicsCapture = Graphics.FromImage(screenBitmap))
                {
                    // Copy the pixels of the screen into the bitmap
                    // screenBounds.Location is the upper left corner of the screen rectangle
                    // Point.Empty is the offset within the bitmap to start copying the pixels from
                    // screenBounds.Size is the size of the rectangle to copy
                    graphicsCapture.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
                    // Convert the bitmap to an Emgu.CV Mat object
                    // screenMat is the output Mat object
                    BitmapExtension.ToMat(screenBitmap, screenMat);
                }

                // The Graphics object has been released
                // At this point the screen data is contained within screenMat

                #region Random Direction Fun
                direction = rnd.Next(1, 5);
                // Move the drawBox rectangle in a random direction
                switch (direction)
                {
                    case 1: // Up
                        drawBox.Y -= rnd.Next(11, 111);
                        break;
                    case 2: // Down
                        drawBox.Y += rnd.Next(11, 111);
                        break;
                    case 3: // Left
                        drawBox.X -= rnd.Next(11, 111);
                        break;
                    case 4: // Right
                        drawBox.X += rnd.Next(11, 111);
                        break;
                }
                // Check if the drawBox rectangle is within the screen bounds
                if (drawBox.X < 0)
                    drawBox.X = 0;
                else if (drawBox.X + drawBox.Width > screenBounds.Width)
                    drawBox.X = screenBounds.Width - drawBox.Width;
                if (drawBox.Y < 0)
                    drawBox.Y = 0;
                else if (drawBox.Y + drawBox.Height > screenBounds.Height)
                    drawBox.Y = screenBounds.Height - drawBox.Height;
                #endregion

                // Draw a rectangle on the image
                CvInvoke.Rectangle(screenMat, drawBox, new MCvScalar(0, 255, 0), default, LineType.AntiAlias);
                // Resize the captured screen image and display it in the named window
                CvInvoke.Resize(screenMat, frame, frame.Size);
                CvInvoke.Imshow(win1, frame);
            }
            CvInvoke.DestroyWindow(win1);
        }
        public static void AdjustBlackWhite()
        {
            // Construct our variables with `using` when possible
            string win1 = "Primary Screen Capture BW";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            using Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using Mat frame = new Mat(screenBounds.Height / 2, screenBounds.Width / 2, DepthType.Cv8U, 3);
            using Mat screenMat = new Mat();
            var options = App.Options.DemoCV;

            // We create our named window
            CvInvoke.NamedWindow(win1);

            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                var minSlide = options.GetKey<int>("minFilterBlackWhite");
                var maxSlide = options.GetKey<int>("maxFilterBlackWhite");
                using (Graphics graphicAdjust = Graphics.FromImage(screenBitmap))
                {
                    graphicAdjust.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
                    // Load Bitmap into Mat
                    BitmapExtension.ToMat(screenBitmap, screenMat);
                }

                // Convert to grayscale
                using Mat grayImage = new Mat();
                CvInvoke.CvtColor(screenMat, grayImage, ColorConversion.Bgr2Gray);

                MCvScalar min = new MCvScalar(minSlide);
                MCvScalar max = new MCvScalar(maxSlide);

                // Convert to binary image
                using Mat binaryImage = new Mat();
                CvInvoke.InRange(grayImage, new ScalarArray(min), new ScalarArray(max), binaryImage);

                // Resize the captured screen image and display it in the named window
                CvInvoke.Resize(binaryImage, frame, frame.Size);
                // Display the binary image in the named window
                CvInvoke.Imshow(win1, frame);
            }
            CvInvoke.DestroyWindow(win1);
            options.SetKey("Display_AdjustBW", false);
        }

        public static void RenderBW()
        {
            ImGui.Begin("DemoCVBlackWhite");
            // This sets up an options for the DemoCV methods.
            var options = App.Options.DemoCV;
            var min = options.GetKey<int>("minFilterBlackWhite");
            var max = options.GetKey<int>("maxFilterBlackWhite");

            if (ImGui.SliderInt("Min",ref min, 0, 255))
            {
                if (min < max)
                    options.SetKey("minFilterBlackWhite",min);
                else
                    min = max-1;
            }

            if (ImGui.SliderInt("Max", ref max, 0, 255))
            {
                if (max > min)
                    options.SetKey("maxFilterBlackWhite", max);
                else
                    max = min+1;
            }
            ImGui.Separator();

            ImGui.End();
        }
        public static void AdjustColor()
        {
            // Construct our variables with `using` when possible
            string win1 = "Primary Screen Capture Color";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            using Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using Mat frame = new Mat(screenBounds.Height / 2, screenBounds.Width / 2, DepthType.Cv8U, 3);
            using Mat screenMat = new Mat();
            using Mat filteredMat = new Mat();
            var options = App.Options.DemoCV;

            // We create our named window
            CvInvoke.NamedWindow(win1);

            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                var min1 = options.GetKey<int>("minFilterColorR");
                var max1 = options.GetKey<int>("maxFilterColorR");
                var min2 = options.GetKey<int>("minFilterColorG");
                var max2 = options.GetKey<int>("maxFilterColorG");
                var min3 = options.GetKey<int>("minFilterColorB");
                var max3 = options.GetKey<int>("maxFilterColorB");

                using (Graphics graphicAdjust = Graphics.FromImage(screenBitmap))
                {
                    graphicAdjust.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
                    BitmapExtension.ToMat(screenBitmap, screenMat); // ==> screenMat
                }

                // Split the input Mat into its three channels
                Mat[] channels = screenMat.Split(); // ==> channels

                // Apply the specified color range to each channel
                ApplyColorFilter(channels[2], min1, max1);
                ApplyColorFilter(channels[1], min2, max2);
                ApplyColorFilter(channels[0], min3, max3);

                // Create an input array of arrays from the filtered color channels
                IInputArrayOfArrays filteredChannelsInput = new VectorOfMat(channels); // ==> filteredChannelsInput
                channels = null;
                // Merge the channels back into a single Mat
                CvInvoke.Merge(filteredChannelsInput, filteredMat); // ==> filteredMat

                // Resize the captured screen image and display it in the named window
                CvInvoke.Resize(filteredMat, frame, frame.Size);

                // Display the filtered image in the named window
                CvInvoke.Imshow(win1, frame);
            }

            CvInvoke.DestroyWindow(win1);
            options.SetKey("Display_AdjustColor", false);
        }

        private static void ApplyColorFilter(Mat channel, int min, int max)
        {
            // Threshold the channel based on the specified color range
            CvInvoke.InRange(channel, new ScalarArray(min), new ScalarArray(max), channel);
        }
        public static void RenderColor()
        {
            ImGui.Begin("DemoCVColor");

            // This sets up an options for the DemoCV methods.
            var options = App.Options.DemoCV;

            // Get the current R, G, B min/max values from options
            var minR = options.GetKey<int>("minFilterColorR");
            var maxR = options.GetKey<int>("maxFilterColorR");
            var minG = options.GetKey<int>("minFilterColorG");
            var maxG = options.GetKey<int>("maxFilterColorG");
            var minB = options.GetKey<int>("minFilterColorB");
            var maxB = options.GetKey<int>("maxFilterColorB");

            // Render R channel sliders
            if (ImGui.SliderInt("Min R", ref minR, 0, 255))
            {
                if (minR < maxR)
                    options.SetKey("minFilterColorR", minR);
                else
                    minR = maxR - 1;
            }
            if (ImGui.SliderInt("Max R", ref maxR, 0, 255))
            {
                if (maxR > minR)
                    options.SetKey("maxFilterColorR", maxR);
                else
                    maxR = minR + 1;
            }
            ImGui.Separator();

            // Render G channel sliders
            if (ImGui.SliderInt("Min G", ref minG, 0, 255))
            {
                if (minG < maxG)
                    options.SetKey("minFilterColorG", minG);
                else
                    minG = maxG - 1;
            }
            if (ImGui.SliderInt("Max G", ref maxG, 0, 255))
            {
                if (maxG > minG)
                    options.SetKey("maxFilterColorG", maxG);
                else
                    maxG = minG + 1;
            }
            ImGui.Separator();

            // Render B channel sliders
            if (ImGui.SliderInt("Min B", ref minB, 0, 255))
            {
                if (minB < maxB)
                    options.SetKey("minFilterColorB", minB);
                else
                    minB = maxB - 1;
            }
            if (ImGui.SliderInt("Max B", ref maxB, 0, 255))
            {
                if (maxB > minB)
                    options.SetKey("maxFilterColorB", maxB);
                else
                    maxB = minB + 1;
            }

            ImGui.End();
        }

    }
}
