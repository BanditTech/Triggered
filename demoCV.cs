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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography;

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
        public static void DemoBlackWhite()
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
        public static void DemoColor()
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

        /// <summary>
        /// Displays a window of the Primary Monitor in color.
        /// </summary>
        public static void DemoIndColor()
        {
            string win1 = "Primary Screen Capture Individual Color";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            var options = App.Options.DemoCV;
            
            // We create our named window
            CvInvoke.NamedWindow(win1);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Set up our local variables
                float r = options.GetKey<float>("filterR");
                float g = options.GetKey<float>("filterG");
                float b = options.GetKey<float>("filterB");
                Vector3 filterColor = options.GetKey<Vector3>("filterColorIndRGB");
                ScalarArray rMin, rMax, gMin, gMax, bMin, bMax;
                
                // Take color points and produce ranges from them
                (rMin, rMax) = ProduceRange(r, filterColor.X);
                (gMin, gMax) = ProduceRange(g, filterColor.Y);
                (bMin, bMax) = ProduceRange(b, filterColor.Z);
                // We use this style of `using` structure to visualize the disposables
                using (Mat screenMat = new Mat())
                {
                    // Produce bounds to capture the primary screen
                    using (Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
                    {
                        // Prepare our graphics context
                        using (Graphics graphicAdjust = Graphics.FromImage(screenBitmap))
                        {
                            // Copy the image from the location into the top left corner of screenBitmap
                            graphicAdjust.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size); // ==> screenBitmap
                            // Release Memory
                            graphicAdjust.Dispose();
                        }
                        // Convert the graphics context into a bitmap
                        BitmapExtension.ToMat(screenBitmap, screenMat); // ==> screenMat
                        // Release Memory
                        screenBitmap.Dispose();
                    }
                    // We now have a bitmap matrix of our screen contained in `screenMat`
                    // Split the input Mat into B G R channels
                    Mat[] channels = screenMat.Split(); // ==> channels
                    // Release Memory
                    screenMat.Dispose();
                    // Apply the specified color range to each channel
                    CvInvoke.InRange(channels[2], rMin, rMax, channels[2]);
                    CvInvoke.InRange(channels[1], gMin, gMax, channels[1]);
                    CvInvoke.InRange(channels[0], bMin, bMax, channels[0]);

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
                            // Release Memory
                            filteredChannelsInput.Dispose();

                            // Resize the captured screen image
                            using (Mat frame = new Mat(screenBounds.Height / 4, screenBounds.Width / 4, DepthType.Cv8U, 3))
                            {
                                CvInvoke.Resize(filteredMat, frame, frame.Size); // ==> frame
                                // Release Memory
                                filteredMat.Dispose();
                                // Display the filtered image in the named window
                                CvInvoke.Imshow(win1, frame);
                                // Release Memory
                                frame.Dispose();
                            }
                        }
                    }
                }
            }
            CvInvoke.DestroyWindow(win1);
            options.SetKey("Display_AdjustIndColor", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderIndColor()
        {
            ImGui.Begin("DemoCVIndColor");

            // This sets up an options for the DemoCV methods.
            var options = App.Options.DemoCV;
            // Get the current values from options
            var r = options.GetKey<float>("filterR");
            var g = options.GetKey<float>("filterG");
            var b = options.GetKey<float>("filterB");
            var color = options.GetKey<Vector3>("filterColorIndRGB");

            // Adjustable range sliders from the base color
            if (ImGui.SliderFloat("Range for R", ref r, 0f, 1f))
                options.SetKey("filterR", r);
            if (ImGui.SliderFloat("Range for G", ref g, 0f, 1f))
                options.SetKey("filterG", g);
            if (ImGui.SliderFloat("Range for B", ref b, 0f, 1f))
                options.SetKey("filterB", b);
            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Color", ref color))
                options.SetKey("filterColorIndRGB", color);

            ImGui.End();
        }

        /// <summary>
        /// Displays a window of the Primary Monitor in HSV.
        /// </summary>
        public static void DemoHSVColor()
        {
            string win1 = "Primary Screen Capture HSV Color";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            var options = App.Options.DemoCV;

            // We create our named window
            CvInvoke.NamedWindow(win1);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Set up our local variables
                float h = options.GetKey<float>("filterH");
                float s = options.GetKey<float>("filterS");
                float v = options.GetKey<float>("filterV");
                Vector3 filterColor = options.GetKey<Vector3>("filterColorHSV");
                ScalarArray hMin, hMax, sMin, sMax, vMin, vMax;

                // Take color points and produce ranges from them
                (hMin, hMax) = ProduceRange(h, filterColor.X, true);
                (sMin, sMax) = ProduceRange(s, filterColor.Y);
                (vMin, vMax) = ProduceRange(v, filterColor.Z);
                // We use this style of `using` structure to visualize the disposables
                using (Mat screenMat = new Mat())
                {
                    // Produce bounds to capture the primary screen
                    using (Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
                    {
                        // Prepare our graphics context
                        using (Graphics graphicAdjust = Graphics.FromImage(screenBitmap))
                        {
                            // Copy the image from the location into the top left corner of screenBitmap
                            graphicAdjust.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size); // ==> screenBitmap
                            // Release Memory
                            graphicAdjust.Dispose();
                        }
                        // Convert the graphics context into a bitmap
                        BitmapExtension.ToMat(screenBitmap, screenMat); // ==> screenMat
                        // Release Memory
                        screenBitmap.Dispose();
                        // Convert into HSV color space
                        using (Mat hsvMat = new Mat())
                        {
                            CvInvoke.CvtColor(screenMat, hsvMat, ColorConversion.Bgr2Hsv);
                            // Release Memory
                            screenMat.Dispose();
                            // Split the input Mat into H S V channels
                            Mat[] channels = hsvMat.Split(); // ==> channels
                            // Release Memory
                            hsvMat.Dispose();
                            // Apply the specified color range to each channel
                            CvInvoke.InRange(channels[0], hMin, hMax, channels[0]);
                            CvInvoke.InRange(channels[1], sMin, sMax, channels[1]);
                            CvInvoke.InRange(channels[2], vMin, vMax, channels[2]);
                            // Combine them again
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
                                    // Release Memory
                                    filteredChannelsInput.Dispose();

                                    // Resize the captured screen image
                                    using (Mat frame = new Mat(screenBounds.Height / 4, screenBounds.Width / 4, DepthType.Cv8U, 3))
                                    {
                                        CvInvoke.Resize(filteredMat, frame, frame.Size); // ==> frame
                                        // Release Memory
                                        filteredMat.Dispose();
                                        // Display the filtered image in the named window
                                        CvInvoke.Imshow(win1, frame);
                                        // Release Memory
                                        frame.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            CvInvoke.DestroyWindow(win1);
            options.SetKey("Display_AdjustHSVColor", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderHSVColor()
        {
            ImGui.Begin("DemoCVHSVColor");

            // This sets up an options for the DemoCV methods.
            var options = App.Options.DemoCV;
            // Get the current values from options
            var h = options.GetKey<float>("filterH");
            var s = options.GetKey<float>("filterS");
            var v = options.GetKey<float>("filterV");
            var color = options.GetKey<Vector3>("filterColorHSV");

            // Adjustable range sliders from the base color
            if (ImGui.SliderFloat("Hue", ref h, 0f, 1f))
                options.SetKey("filterH", h);
            if (ImGui.SliderFloat("Saturation", ref s, 0f, 1f))
                options.SetKey("filterS", s);
            if (ImGui.SliderFloat("Vibrance", ref v, 0f, 1f))
                options.SetKey("filterV", v);
            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Color", ref color, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                options.SetKey("filterColorHSV", color);

            ImGui.End();
        }

        /// <summary>
        /// Displays a window of the Primary Monitor in HSV.
        /// </summary>
        public static void DemoHSVColorDual()
        {
            string win1 = "Primary Screen Capture HSV Dual Color";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            var options = App.Options.DemoCV;

            // We create our named window
            CvInvoke.NamedWindow(win1, WindowFlags.FreeRatio);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Set up our local variables
                Vector3 min = options.GetKey<Vector3>("filterColorHSVMin");
                Vector3 max = options.GetKey<Vector3>("filterColorHSVMax");
                ScalarArray hMin, hMax, sMin, sMax, vMin, vMax;

                // Take color points and produce ranges from them
                (hMin, hMax) = ProduceMinMax(min.X, max.X, true);
                (sMin, sMax) = ProduceMinMax(min.Y, max.Y);
                (vMin, vMax) = ProduceMinMax(min.Z, max.Z);

                Mat screenMat = GetScreenMat(screenBounds);
                // Convert into HSV color space
                Mat hsvMat = new();
                CvInvoke.CvtColor(screenMat, hsvMat, ColorConversion.Bgr2Hsv);
                // Split the input Mat into H S V channels
                Mat[] channels = hsvMat.Split(); // ==> channels
                // Release Memory
                hsvMat.Dispose();
                // Apply the specified color range to each channel
                CvInvoke.InRange(channels[0], hMin, hMax, channels[0]);
                CvInvoke.InRange(channels[1], sMin, sMax, channels[1]);
                CvInvoke.InRange(channels[2], vMin, vMax, channels[2]);
                // Produce the target VectorOfMat to merge
                var filteredChannelsInput = new VectorOfMat();
                // Ensure the memory is not occupied (memory leak)
                filteredChannelsInput.Clear();
                // Recombine the channels
                foreach (var channel in channels)
                    filteredChannelsInput.Push(channel);
                // Release Memory
                foreach (var channel in channels)
                    channel.Dispose();
                // Create the destination for the VectorOfMat
                Mat filteredMat = new();
                // Merge the VectorOfMat into a Mat
                CvInvoke.Merge(filteredChannelsInput, filteredMat); // ==> filteredMat
                // Release Memory
                filteredChannelsInput.Dispose();
                // Produce the target mask Mat
                Mat hsvMask = GetBlackWhiteMaskMat(filteredMat);
                // Release Memory
                filteredMat.Dispose();
                // Create a Mat for the result
                Mat copied = new();
                // Copy the image from within the masked area
                screenMat.CopyTo(copied, hsvMask);
                // Release Memory
                screenMat.Dispose();
                // Create a destination for the resized image
                Mat frame = new(screenBounds.Height / 4, screenBounds.Width / 4, DepthType.Cv8U, 3);
                // Move the image data into another shape
                CvInvoke.Resize(copied, frame, frame.Size); // ==> frame
                // Release Memory
                hsvMask.Dispose();
                copied.Dispose();
                // Display the image
                CvInvoke.Imshow(win1, frame);
                // Release Memory
                frame.Dispose();
            }
            CvInvoke.DestroyWindow(win1);
            options.SetKey("Display_AdjustHSVColorDual", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderHSVColorDual()
        {
            ImGui.Begin("DemoCVHSVColorDual");

            // This sets up an options for the DemoCV methods.
            var options = App.Options.DemoCV;
            // Get the current values from options
            var min = options.GetKey<Vector3>("filterColorHSVMin");
            var max = options.GetKey<Vector3>("filterColorHSVMax");

            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Min", ref min, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                options.SetKey("filterColorHSVMin", min);

            if (ImGui.ColorPicker3("Filter Max", ref max, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                options.SetKey("filterColorHSVMax", max);

            ImGui.End();
        }

        public static void DisplayImage(string name,Mat mat, int height = 0, int width = 0, int channels = 3, DepthType type = DepthType.Cv8U)
        {
            // First we establish some defaults for the display size
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            if (width <= 0 || height <= 0)
            {
                height = screenBounds.Height / 4;
                width = screenBounds.Width / 4;
            }
            else if (width < 10 && height < 10)
            {
                height = screenBounds.Height / height;
                width = screenBounds.Width / width;
            }

            // Produce the target frame for the image
            Mat frame = new(height, width, type, channels);
            CvInvoke.Resize(mat, frame, frame.Size); // ==> frame
            CvInvoke.Imshow(name, frame);
            frame.Dispose();
        }

        public static Mat GetFilteredMat(Mat mat, Vector3 min, Vector3 max, bool hsv = false, bool flip = false)
        {
            // produce target variables
            ScalarArray hMin, hMax, sMin, sMax, vMin, vMax;
            // Take color points and produce ranges from them
            (hMin, hMax) = ProduceMinMax(min.X, max.X, hsv);
            (sMin, sMax) = ProduceMinMax(min.Y, max.Y);
            (vMin, vMax) = ProduceMinMax(min.Z, max.Z);
            // Split the input Mat into H S V channels
            Mat[] channels = mat.Split(); // ==> channels
            // Apply the specified color range to each channel
            CvInvoke.InRange(channels[0], hMin, hMax, channels[0]);
            CvInvoke.InRange(channels[1], sMin, sMax, channels[1]);
            CvInvoke.InRange(channels[2], vMin, vMax, channels[2]);
            // Merge the channels back into a Mat
            return GetMergedMat(channels);
        }

        public static Mat GetScreenMat(Rectangle screenBounds)
        {
            // Produce bounds to capture the primary screen
            Bitmap screenBitmap = new(screenBounds.Width, screenBounds.Height);
            // Prepare our graphics context
            Graphics graphicAdjust = Graphics.FromImage(screenBitmap);
            // Copy the image from the location into the top left corner of screenBitmap
            graphicAdjust.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size); // ==> screenBitmap
                                                                                                 // Release Memory
            graphicAdjust.Dispose();
            // Convert the graphics context into a bitmap
            Mat screenMat = new();
            BitmapExtension.ToMat(screenBitmap, screenMat); // ==> screenMat
                                                            // Release Memory
            screenBitmap.Dispose();
            return screenMat;
        }

        public static Mat GetMergedMat(Mat[] channels)
        {
            // Produce the target VectorOfMat to merge
            var channelsInput = new VectorOfMat();
            // Ensure the memory is not occupied (memory leak)
            channelsInput.Clear();
            // Recombine the channels
            foreach (var channel in channels)
                channelsInput.Push(channel);
            // Release Memory
            foreach (var channel in channels)
                channel.Dispose();
            // Create the destination for the VectorOfMat
            Mat mat = new();
            // Merge the VectorOfMat into a Mat
            CvInvoke.Merge(channelsInput, mat); // ==> filteredMat
            // Release Memory
            channelsInput.Dispose();
            // We have a Mat produced from our channels
            return mat;
        }

        public static Mat GetBlackWhiteMaskMat(Mat filteredMat)
        {
            // Produce the target mask Mat
            Mat mask = new();
            // Convert to gray
            CvInvoke.CvtColor(filteredMat, mask, ColorConversion.Bgr2Gray);
            // Release Memory
            filteredMat.Dispose();
            // Strip non-white pixels
            CvInvoke.InRange(mask, new ScalarArray(255), new ScalarArray(255), mask);
            return mask;
        }

        public static (ScalarArray, ScalarArray) ProduceMinMax(float min, float max, bool hue = false)
        {
            if (hue)
            {
                min = Math.Clamp(min * 180, 0, 180);
                max = Math.Clamp(max * 180, 0, 180);
            }
            else
            {
                min = Math.Clamp(min * 255, 0, 255);
                max = Math.Clamp(max * 255, 0, 255);
            }
            return (new ScalarArray(min), new ScalarArray(max));
        }

        /// <summary>
        /// Takes a point and splits the range onto either side.
        /// When below 0f or above 1f it moves that value to the opposite side.
        /// Always returns set of points with a sum equivalent to provided range.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static (ScalarArray, ScalarArray) ProduceRange(float range, float point, bool hue = false)
        {
            float halfRange = range / 2f;
            float min = point - halfRange;
            float max = point + halfRange;

            if (min < 0f)
            {
                max -= min;
                min = 0f;
            }

            if (max > 1f)
            {
                min -= (max - 1f);
                max = 1f;
            }

            if (min < 0f)
                min = 0f;
            
            if (hue)
            {
                min = Math.Clamp(min * 180, 0, 180);
                max = Math.Clamp(max * 180, 0, 180);
            }
            else
            {
                min = Math.Clamp(min * 255, 0, 255);
                max = Math.Clamp(max * 255, 0, 255);
            }
            return (new ScalarArray(min), new ScalarArray(max));
        }
    }
}
