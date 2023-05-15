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
using static Triggered.modules.wrapper.OpenCV;
using System.Diagnostics;
using System.Collections.Generic;

namespace Triggered.modules.demo
{
    /// <summary>
    /// This class is a demonstration of what methods we can use from OpenCV using Emgu.CV.
    /// Simple sections without controls only need to be called once.
    /// For sections using ImGui controls, you will need to seperate them into another thread.
    /// Follow the same formula as the other menus, do not try to open ImGui inside a thread with Forms.
    /// </summary>
    public static class DemoCV
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
                   new Point(10, 80),
                   FontFace.HersheyComplex,
                   3,
                   new Bgr(0, 255, 0).MCvScalar,
                   2);
                CvInvoke.PutText(
                   img,
                   "world",
                   new Point(10, 160),
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
                    screenBitmap.ToMat(screenMat);
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
                    screenBitmap.ToMat(screenMat);
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

            if (ImGui.SliderInt("Min", ref min, 0, 255))
            {
                if (min < max)
                    options.SetKey("minFilterBlackWhite", min);
                else
                    min = max - 1;
            }

            if (ImGui.SliderInt("Max", ref max, 0, 255))
            {
                if (max > min)
                    options.SetKey("maxFilterBlackWhite", max);
                else
                    max = min + 1;
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
                            screenBitmap.ToMat(screenMat); // ==> screenMat
                            graphicAdjust.Dispose();
                        }
                        screenBitmap.Dispose();
                    }

                    // Split the input Mat into its three channels
                    Mat[] channels = screenMat.Split(); // ==> channels

                    // Apply the specified color range to each channel
                    CvInvoke.InRange(channels[2], new ScalarArray(Math.Clamp((filterColor.X - down) * 255, 0, 255)), new ScalarArray(Math.Clamp((filterColor.X + up) * 255, 0, 255)), channels[2]);
                    CvInvoke.InRange(channels[1], new ScalarArray(Math.Clamp((filterColor.Y - down) * 255, 0, 255)), new ScalarArray(Math.Clamp((filterColor.Y + up) * 255, 0, 255)), channels[1]);
                    CvInvoke.InRange(channels[0], new ScalarArray(Math.Clamp((filterColor.Z - down) * 255, 0, 255)), new ScalarArray(Math.Clamp((filterColor.Z + up) * 255, 0, 255)), channels[0]);

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
                        screenBitmap.ToMat(screenMat); // ==> screenMat
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
                        screenBitmap.ToMat(screenMat); // ==> screenMat
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
                // Capture the screen
                Mat screenMat = GetScreenMat(screenBounds);
                // Convert into HSV color space
                Mat hsvMat = new();
                CvInvoke.CvtColor(screenMat, hsvMat, ColorConversion.Bgr2Hsv);
                // Filter the image according to the range values.
                Mat filteredMat = GetFilteredMat(hsvMat, min, max, true);
                // Release Memory
                hsvMat.Dispose();
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
                hsvMask.Dispose();
                // Display the Masked image
                DisplayImage(win1, copied, 2, 2);
                // Release Memory
                copied.Dispose();
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

        /// <summary>
        /// Display a window of Primary Monitor for shape detection
        /// </summary>
        public static void DemoShapeDetection()
        {
            string win1 = "Shape Detection";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            var options = App.Options.DemoCV;


            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Capture the screen
                Mat screenMat = GetScreenMat(screenBounds);
                // Resize the image
                Mat frame = new(screenMat.Rows / 4, screenMat.Cols / 4, DepthType.Cv8U, 3);
                CvInvoke.Resize(screenMat, frame, frame.Size); // ==> frame
                Mat result = ProcessImage(frame);
                screenMat.Dispose();
                frame.Dispose();

                DisplayImage(win1, result);
            }

            CvInvoke.DestroyWindow(win1);
            options.SetKey("Display_AdjustShape", false);
        }

        public static Mat ProcessImage(Mat img)
        {
            using (UMat gray = new UMat())
            using (UMat cannyEdges = new UMat())
            using (Mat triangleRectangleImage = new Mat(img.Size, DepthType.Cv8U, 3)) //image to draw triangles and rectangles on
            using (Mat circleImage = new Mat(img.Size, DepthType.Cv8U, 3)) //image to draw circles on
            using (Mat lineImage = new Mat(img.Size, DepthType.Cv8U, 3)) //image to drtaw lines on
            {
                //Convert the image to grayscale and filter out the noise
                CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);

                //Remove noise
                CvInvoke.GaussianBlur(gray, gray, new Size(3, 3), 1);

                #region circle detection
                double cannyThreshold = 180.0;
                double circleAccumulatorThreshold = 120;
                CircleF[] circles = CvInvoke.HoughCircles(gray, HoughModes.Gradient, 2.0, 20.0, cannyThreshold,
                    circleAccumulatorThreshold, 5);
                #endregion

                #region Canny and edge detection
                double cannyThresholdLinking = 120.0;
                CvInvoke.Canny(gray, cannyEdges, cannyThreshold, cannyThresholdLinking);
                LineSegment2D[] lines = CvInvoke.HoughLinesP(
                    cannyEdges,
                    1, //Distance resolution in pixel-related units
                    Math.PI / 45.0, //Angle resolution measured in radians.
                    20, //threshold
                    30, //min Line width
                    10); //gap between lines
                #endregion

                #region Find triangles and rectangles
                List<Triangle2DF> triangleList = new List<Triangle2DF>();
                List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List,
                        ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        using (VectorOfPoint approxContour = new VectorOfPoint())
                        {
                            CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05,
                                true);
                            if (CvInvoke.ContourArea(approxContour, false) > 250
                            ) //only consider contours with area greater than 250
                            {
                                if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
                                {
                                    Point[] pts = approxContour.ToArray();
                                    triangleList.Add(new Triangle2DF(
                                        pts[0],
                                        pts[1],
                                        pts[2]
                                    ));
                                }
                                else if (approxContour.Size == 4) //The contour has 4 vertices.
                                {
                                    #region determine if all the angles in the contour are within [80, 100] degree
                                    bool isRectangle = true;
                                    Point[] pts = approxContour.ToArray();
                                    LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                    for (int j = 0; j < edges.Length; j++)
                                    {
                                        double angle = Math.Abs(
                                            edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                        if (angle < 80 || angle > 100)
                                        {
                                            isRectangle = false;
                                            break;
                                        }
                                    }

                                    #endregion

                                    if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
                                }
                            }
                        }
                    }
                }
                #endregion

                #region draw triangles and rectangles
                triangleRectangleImage.SetTo(new MCvScalar(0));
                foreach (Triangle2DF triangle in triangleList)
                {
                    CvInvoke.Polylines(triangleRectangleImage, Array.ConvertAll(triangle.GetVertices(), Point.Round),
                        true, new Bgr(Color.DarkBlue).MCvScalar, 2);
                }

                foreach (RotatedRect box in boxList)
                {
                    CvInvoke.Polylines(triangleRectangleImage, Array.ConvertAll(box.GetVertices(), Point.Round), true,
                        new Bgr(Color.DarkOrange).MCvScalar, 2);
                }

                //Drawing a light gray frame around the image
                CvInvoke.Rectangle(triangleRectangleImage,
                    new Rectangle(Point.Empty,
                        new Size(triangleRectangleImage.Width - 1, triangleRectangleImage.Height - 1)),
                    new MCvScalar(120, 120, 120));
                //Draw the labels
                CvInvoke.PutText(triangleRectangleImage, "Triangles and Rectangles", new Point(20, 20),
                    FontFace.HersheyDuplex, 0.5, new MCvScalar(120, 120, 120));
                #endregion

                #region draw circles
                circleImage.SetTo(new MCvScalar(0));
                foreach (CircleF circle in circles)
                    CvInvoke.Circle(circleImage, Point.Round(circle.Center), (int)circle.Radius,
                        new Bgr(Color.Brown).MCvScalar, 2);

                //Drawing a light gray frame around the image
                CvInvoke.Rectangle(circleImage,
                    new Rectangle(Point.Empty, new Size(circleImage.Width - 1, circleImage.Height - 1)),
                    new MCvScalar(120, 120, 120));
                //Draw the labels
                CvInvoke.PutText(circleImage, "Circles", new Point(20, 20), FontFace.HersheyDuplex, 0.5,
                    new MCvScalar(120, 120, 120));
                #endregion

                #region draw lines
                lineImage.SetTo(new MCvScalar(0));
                foreach (LineSegment2D line in lines)
                    CvInvoke.Line(lineImage, line.P1, line.P2, new Bgr(Color.Green).MCvScalar, 2);
                //Drawing a light gray frame around the image
                CvInvoke.Rectangle(lineImage,
                    new Rectangle(Point.Empty, new Size(lineImage.Width - 1, lineImage.Height - 1)),
                    new MCvScalar(120, 120, 120));
                //Draw the labels
                CvInvoke.PutText(lineImage, "Lines", new Point(20, 20), FontFace.HersheyDuplex, 0.5,
                    new MCvScalar(120, 120, 120));
                #endregion

                Mat result = new Mat();
                CvInvoke.VConcat(new Mat[] { triangleRectangleImage, circleImage, lineImage }, result);
                return result;
            }
        }

        public static void DemoShapeRectangle()
        {
            var options = App.Options.DemoCV;
            string win1 = "Rectangle Detection";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Capture the screen
                Mat screenMat = GetScreenMat(screenBounds);
                // Resize the image
                Mat frame = new(screenMat.Rows / 4, screenMat.Cols / 4, DepthType.Cv8U, 3);
                CvInvoke.Resize(screenMat, frame, frame.Size); // ==> frame
                Mat result = ProcessRectangles(frame);
                screenMat.Dispose();
                frame.Dispose();

                DisplayImage(win1, result);
                result.Dispose();
            }

            CvInvoke.DestroyWindow(win1);
            options.SetKey("Display_AdjustRectangle", false);
        }

        public static Mat ProcessRectangles(Mat img)
        {
            var options = App.Options.DemoCV;
            // Get the current values from options
            var area = options.GetKey<int>("rectangleArea");
            double cannyThreshold = options.GetKey<float>("cannyThreshold");
            double cannyThresholdLinking = options.GetKey<float>("cannyThresholdLinking");
            Stopwatch watch = Stopwatch.StartNew();

            using (UMat gray = new UMat())
            using (UMat cannyEdges = new UMat())
            {
                //Convert the image to grayscale and filter out the noise
                CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);

                //Remove noise
                CvInvoke.GaussianBlur(gray, gray, new Size(3, 3), 1);

                // Produce Edges
                CvInvoke.Canny(gray, cannyEdges, cannyThreshold, cannyThresholdLinking);

                #region Find and rectangles
                List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle
                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List,
                        ChainApproxMethod.ChainApproxSimple);
                    int count = contours.Size;
                    for (int i = 0; i < count; i++)
                    {
                        using (VectorOfPoint contour = contours[i])
                        using (VectorOfPoint approxContour = new VectorOfPoint())
                        {
                            CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05,
                                true);
                            if (CvInvoke.ContourArea(approxContour, false) > area && approxContour.Size == 4 )
                            {
                                #region determine if all the angles in the contour are within [80, 100] degree
                                bool isRectangle = true;
                                Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double angle = Math.Abs(
                                        edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }

                                #endregion

                                if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
                            }
                        }
                    }
                }
                #endregion

                #region Draw rectangles
                Mat triangleRectangleImage = new Mat(img.Size, DepthType.Cv8U, 3);
                triangleRectangleImage.SetTo(new MCvScalar(0));

                foreach (RotatedRect box in boxList)
                {
                    CvInvoke.Polylines(triangleRectangleImage, Array.ConvertAll(box.GetVertices(), Point.Round), true,
                        new Bgr(Color.DarkOrange).MCvScalar, 2);
                }
                //Drawing a light gray frame around the image
                CvInvoke.Rectangle(triangleRectangleImage,
                    new Rectangle(Point.Empty,
                        new Size(triangleRectangleImage.Width - 1, triangleRectangleImage.Height - 1)),
                    new MCvScalar(120, 120, 120));
                //Draw the labels
                CvInvoke.PutText(triangleRectangleImage, "Rectangles", new Point(20, 20),
                    FontFace.HersheyDuplex, 0.5, new MCvScalar(120, 120, 120));
                #endregion

                watch.Stop();
                App.Log($"Rectangle Detection took: {watch.ElapsedMilliseconds}ms",0);
                return triangleRectangleImage;
            }
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderShapeDetection()
        {

        }
        public static void RenderShapeRectangle()
        {
            // This sets up an options for the DemoCV methods.
            var options = App.Options.DemoCV;
            // Get the current values from options
            var area = options.GetKey<int>("rectangleArea");
            var cannyThreshold = options.GetKey<float>("cannyThreshold");
            var cannyThresholdLinking = options.GetKey<float>("cannyThresholdLinking");

            if (ImGui.SliderInt("Area", ref area, 10, 1000))
                options.SetKey("rectangleArea", area);
            if (ImGui.SliderFloat("cannyThreshold", ref cannyThreshold, 0f, 1000f))
                options.SetKey("cannyThreshold", cannyThreshold);
            if (ImGui.SliderFloat("cannyThresholdLinking", ref cannyThresholdLinking, 0f, 1000f))
                options.SetKey("cannyThresholdLinking", cannyThresholdLinking);
        }
    }
}
