using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Windows.Forms;
using ImGuiNET;
using System.Numerics;
using Emgu.CV.Reg;
using System.Threading.Channels;

namespace Triggered
{
    /// <summary>
    /// This class is a demonstration of what methods we can use from OpenCV using Emgu.CV.
    /// Simple sections without controls only need to be called once.
    /// For sections using ImGui controls, you will need to seperate them into another thread.
    /// Follow the same formula as the other menus, do not try to open ImGui inside a thread with Forms.
    /// </summary>
    public static class demoCV
    {
        /// <summary>
        /// Shows a blue screen with Hello, World
        /// </summary>
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
        /// <summary>
        /// Displays the primary monitor with a wacky rectangle.
        /// </summary>
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
        /// <summary>
        /// Displays a window of the Primary Monitor in black and white.
        /// </summary>
        public static void AdjustBlackWhite()
        {
            // Construct our variables with `using` when possible
            string win1 = "Primary Screen Capture BW";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            using Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using Mat frame = new Mat(screenBounds.Height / 4, screenBounds.Width / 4, DepthType.Cv8U, 3);
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
        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
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
        /// <summary>
        /// Displays a window of the Primary Monitor in color.
        /// </summary>
        public static void AdjustColor()
        {
            // Construct our variables with `using` when possible
            string win1 = "Primary Screen Capture Color";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            var options = App.Options.DemoCV;

            // We create our named window
            CvInvoke.NamedWindow(win1);

            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                float up = options.GetKey<float>("filterup");
                float down = options.GetKey<float>("filterdown");
                Vector3 filterColor = options.GetKey<Vector3>("filterColorRGB");

                using (Mat screenMat = new Mat())
                {
                    using (Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
                    {
                        using (Graphics graphicAdjust = Graphics.FromImage(screenBitmap))
                        {
                            graphicAdjust.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
                            BitmapExtension.ToMat(screenBitmap, screenMat); // ==> screenMat
                            graphicAdjust.Dispose();
                        }
                        screenBitmap.Dispose();
                    }

                    // Split the input Mat into its three channels
                    Mat[] channels = screenMat.Split(); // ==> channels

                    // Apply the specified color range to each channel
                    CvInvoke.InRange(channels[2], new ScalarArray(Math.Clamp((filterColor.X - down)*255, 0, 255)), new ScalarArray(Math.Clamp((filterColor.X + up)*255, 0, 255)), channels[2]);
                    CvInvoke.InRange(channels[1], new ScalarArray(Math.Clamp((filterColor.Y - down)*255, 0, 255)), new ScalarArray(Math.Clamp((filterColor.Y + up)*255, 0, 255)), channels[1]);
                    CvInvoke.InRange(channels[0], new ScalarArray(Math.Clamp((filterColor.Z - down)*255, 0, 255)), new ScalarArray(Math.Clamp((filterColor.Z + up)*255, 0, 255)), channels[0]);

                    using (var filteredChannelsInput = new VectorOfMat())
                    {
                        // Set the channels to the VectorOfMat object
                        filteredChannelsInput.Clear();
                        foreach (var channel in channels)
                            filteredChannelsInput.Push(channel);
                        // Release the memory used by the channels and clear the VectorOfMat object
                        foreach (var channel in channels)
                            channel.Dispose();
                        // Merge the channels back into a single Mat
                        using (Mat filteredMat = new Mat())
                        {
                            CvInvoke.Merge(filteredChannelsInput, filteredMat); // ==> filteredMat

                            // Resize the captured screen image
                            using (Mat frame = new Mat(screenBounds.Height / 4, screenBounds.Width / 4, DepthType.Cv8U, 3))
                            {
                                CvInvoke.Resize(filteredMat, frame, frame.Size);
                                // Display the filtered image in the named window
                                CvInvoke.Imshow(win1, frame);
                                frame.Dispose();
                            }
                            filteredMat.Dispose();
                        }
                        filteredChannelsInput.Dispose();
                    }
                    screenMat.Dispose();
                }
            }
            CvInvoke.DestroyWindow(win1);
            options.SetKey("Display_AdjustColor", false);
        }
        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderColor()
        {
            ImGui.Begin("DemoCVColor");

            // This sets up an options for the DemoCV methods.
            var options = App.Options.DemoCV;
            // Get the current values from options
            var up = options.GetKey<float>("filterup");
            var down = options.GetKey<float>("filterdown");
            var color = options.GetKey<Vector3>("filterColorRGB");

            // Adjustable range sliders from the base color
            if (ImGui.SliderFloat("Included Below", ref down, 0f, 1f))
                options.SetKey("filterdown", down);
            if (ImGui.SliderFloat("Included Above", ref up, 0f, 1f))
                options.SetKey("filterup", up);
            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Color", ref color))
                options.SetKey("filterColorRGB", color);

            ImGui.End();
        }
    }
}
