﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.OCR;
using System;
using System.Drawing;
using System.Windows.Forms;
using ImGuiNET;
using System.Numerics;
using static Triggered.modules.wrapper.OpenCV;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Triggered.modules.options;

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
        private static Options_Panel Panel => App.Options.Panel;
        private static Options_DemoCV Opts => App.Options.DemoCV;
        public static void Render()
        {
            if (Panel.GetKey<bool>("CV.BlackWhite"))
                RenderBW();
            if (Panel.GetKey<bool>("CV.Color"))
                RenderColor();
            if (Panel.GetKey<bool>("CV.IndividualColor"))
                RenderIndColor();
            if (Panel.GetKey<bool>("CV.HSV"))
                RenderHSVColor();
            if (Panel.GetKey<bool>("CV.DualHSV"))
                RenderHSVColorDual();
            if (Panel.GetKey<bool>("CV.Shape"))
                RenderShapeDetection();
            if (Panel.GetKey<bool>("CV.Rectangle"))
                RenderShapeRectangle();
            if (Panel.GetKey<bool>("CV.SubsetHSV"))
                RenderHSVSubset();
            if (Panel.GetKey<bool>("CV.OCR"))
                RenderOCR();
            if (Panel.GetKey<bool>("CV.WindowHandle"))
                RenderHWND();
        }
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

            // We create our named window
            CvInvoke.NamedWindow(win1);

            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                var minSlide = Opts.GetKey<int>("minFilterBlackWhite");
                var maxSlide = Opts.GetKey<int>("maxFilterBlackWhite");
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
            Panel.SetKey("CV.BlackWhite", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderBW()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("DemoCVBlackWhite");
            var min = Opts.GetKey<int>("minFilterBlackWhite");
            var max = Opts.GetKey<int>("maxFilterBlackWhite");

            if (ImGui.SliderInt("Min", ref min, 0, 255))
            {
                if (min < max)
                    Opts.SetKey("minFilterBlackWhite", min);
                else
                    min = max - 1;
            }

            if (ImGui.SliderInt("Max", ref max, 0, 255))
            {
                if (max > min)
                    Opts.SetKey("maxFilterBlackWhite", max);
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

            // We create our named window
            CvInvoke.NamedWindow(win1);

            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                float up = Opts.GetKey<float>("filterup");
                float down = Opts.GetKey<float>("filterdown");
                Vector3 filterColor = Opts.GetKey<Vector3>("filterColorRGB");

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
            Panel.SetKey("CV.Color", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderColor()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("DemoCVColor");

            // Get the current values from options
            var up = Opts.GetKey<float>("filterup");
            var down = Opts.GetKey<float>("filterdown");
            var color = Opts.GetKey<Vector3>("filterColorRGB");

            // Adjustable range sliders from the base color
            if (ImGui.SliderFloat("Included Below", ref down, 0f, 1f))
                Opts.SetKey("filterdown", down);
            if (ImGui.SliderFloat("Included Above", ref up, 0f, 1f))
                Opts.SetKey("filterup", up);
            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Color", ref color))
                Opts.SetKey("filterColorRGB", color);

            ImGui.End();
        }

        /// <summary>
        /// Displays a window of the Primary Monitor in color.
        /// </summary>
        public static void DemoIndColor()
        {
            string win1 = "Primary Screen Capture Individual Color";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            // We create our named window
            CvInvoke.NamedWindow(win1);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Set up our local variables
                float r = Opts.GetKey<float>("filterR");
                float g = Opts.GetKey<float>("filterG");
                float b = Opts.GetKey<float>("filterB");
                Vector3 filterColor = Opts.GetKey<Vector3>("filterColorIndRGB");
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
            Panel.SetKey("CV.IndividualColor", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderIndColor()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("DemoCVIndColor");

            // Get the current values from options
            var r = Opts.GetKey<float>("filterR");
            var g = Opts.GetKey<float>("filterG");
            var b = Opts.GetKey<float>("filterB");
            var color = Opts.GetKey<Vector3>("filterColorIndRGB");

            // Adjustable range sliders from the base color
            if (ImGui.SliderFloat("Range for R", ref r, 0f, 1f))
                Opts.SetKey("filterR", r);
            if (ImGui.SliderFloat("Range for G", ref g, 0f, 1f))
                Opts.SetKey("filterG", g);
            if (ImGui.SliderFloat("Range for B", ref b, 0f, 1f))
                Opts.SetKey("filterB", b);
            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Color", ref color))
                Opts.SetKey("filterColorIndRGB", color);

            ImGui.End();
        }

        /// <summary>
        /// Displays a window of the Primary Monitor in HSV.
        /// </summary>
        public static void DemoHSVColor()
        {
            string win1 = "Primary Screen Capture HSV Color";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            // We create our named window
            CvInvoke.NamedWindow(win1);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Set up our local variables
                float h = Opts.GetKey<float>("filterH");
                float s = Opts.GetKey<float>("filterS");
                float v = Opts.GetKey<float>("filterV");
                Vector3 filterColor = Opts.GetKey<Vector3>("filterColorHSV");
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
            Panel.SetKey("CV.HSV", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderHSVColor()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("DemoCVHSVColor");

            // Get the current values from options
            var h = Opts.GetKey<float>("filterH");
            var s = Opts.GetKey<float>("filterS");
            var v = Opts.GetKey<float>("filterV");
            var color = Opts.GetKey<Vector3>("filterColorHSV");

            // Adjustable range sliders from the base color
            if (ImGui.SliderFloat("Hue", ref h, 0f, 1f))
                Opts.SetKey("filterH", h);
            if (ImGui.SliderFloat("Saturation", ref s, 0f, 1f))
                Opts.SetKey("filterS", s);
            if (ImGui.SliderFloat("Vibrance", ref v, 0f, 1f))
                Opts.SetKey("filterV", v);
            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Color", ref color, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                Opts.SetKey("filterColorHSV", color);

            ImGui.End();
        }

        /// <summary>
        /// Displays a window of the Primary Monitor in HSV.
        /// </summary>
        public static void DemoHSVColorDual()
        {
            string win1 = "Primary Screen Capture HSV Dual Color";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            // We create our named window
            CvInvoke.NamedWindow(win1, WindowFlags.FreeRatio);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Set up our local variables
                Vector3 min = Opts.GetKey<Vector3>("filterColorHSVMin");
                Vector3 max = Opts.GetKey<Vector3>("filterColorHSVMax");
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
            Panel.SetKey("CV.DualHSV", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderHSVColorDual()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("DemoCVHSVColorDual");

            // Get the current values from options
            var min = Opts.GetKey<Vector3>("filterColorHSVMin");
            var max = Opts.GetKey<Vector3>("filterColorHSVMax");

            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Min", ref min, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                Opts.SetKey("filterColorHSVMin", min);

            if (ImGui.ColorPicker3("Filter Max", ref max, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                Opts.SetKey("filterColorHSVMax", max);

            ImGui.End();
        }

        /// <summary>
        /// Display a window of Primary Monitor for shape detection
        /// </summary>
        public static void DemoShapeDetection()
        {
            string win1 = "Shape Detection";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

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
            Panel.SetKey("CV.Shape", false);
        }

        /// <summary>
        /// Method from Emgu.CV Wiki that demonstrates Edge, circle and rectangle detection.
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
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

        /// <summary>
        /// See how fast we can make rectangle detection alone.
        /// </summary>
        public static void DemoShapeRectangle()
        {
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
            Panel.SetKey("CV.Rectangle", false);
        }

        /// <summary>
        /// Rectangle Processing and benchmark.
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Mat ProcessRectangles(Mat img)
        {
            // Get the current values from options
            var area = Opts.GetKey<int>("rectangleArea");
            double cannyThreshold = Opts.GetKey<float>("cannyThreshold");
            double cannyThresholdLinking = Opts.GetKey<float>("cannyThresholdLinking");
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
            // There are no settings for this
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderShapeRectangle()
        {
            // Get the current values from options
            var area = Opts.GetKey<int>("rectangleArea");
            var cannyThreshold = Opts.GetKey<float>("cannyThreshold");
            var cannyThresholdLinking = Opts.GetKey<float>("cannyThresholdLinking");

            if (ImGui.SliderInt("Area", ref area, 10, 1000))
                Opts.SetKey("rectangleArea", area);
            if (ImGui.SliderFloat("cannyThreshold", ref cannyThreshold, 0f, 1000f))
                Opts.SetKey("cannyThreshold", cannyThreshold);
            if (ImGui.SliderFloat("cannyThresholdLinking", ref cannyThresholdLinking, 0f, 1000f))
                Opts.SetKey("cannyThresholdLinking", cannyThresholdLinking);
        }

        private static float _percentage = 1f;
        /// <summary>
        /// Displays a window of a subset of the Primary Monitor in HSV.
        /// </summary>
        public static void DemoHSVSubset()
        {
            string win1 = "HSV Subset Matching";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            // We create our named window
            CvInvoke.NamedWindow(win1, WindowFlags.FreeRatio);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Set up our local variables
                Vector3 min = Opts.GetKey<Vector3>("filterSubsetHSVMin");
                Vector3 max = Opts.GetKey<Vector3>("filterSubsetHSVMax");
                int x = Opts.GetKey<int>("filterSubsetX");
                int y = Opts.GetKey<int>("filterSubsetY");
                int w = Opts.GetKey<int>("filterSubsetW");
                int h = Opts.GetKey<int>("filterSubsetH");

                Rectangle subset = new Rectangle(x, y, w, h);
                // Capture the screen
                Mat capture = GetScreenMat(screenBounds);
                Mat screenMat = new Mat(capture,subset);
                // Convert into HSV color space
                Mat hsvMat = new();
                CvInvoke.CvtColor(screenMat, hsvMat, ColorConversion.Bgr2Hsv);
                // Filter the image according to the range values.
                Mat filteredMat = GetFilteredMat(hsvMat, min, max, true);
                // Release Memory
                hsvMat.Dispose();
                // Produce the target mask Mat
                Mat hsvMask = GetBlackWhiteMaskMat(filteredMat);
                Stopwatch watch = Stopwatch.StartNew();
                var percentage = GetMaskPercentage(filteredMat);
                watch.Stop();
                App.Log($"Mask Percentage Detection took: {watch.ElapsedMilliseconds}ms", 0);
                if (_percentage != percentage)
                {
                    _percentage = percentage;
                    Opts.SetKey("filterSubsetPercentage", percentage);
                }
                // Release Memory
                filteredMat.Dispose();
                // Create a Mat for the result
                Mat copied = new();
                // Copy the image from within the masked area
                screenMat.CopyTo(copied, hsvMask);
                // Release Memory
                screenMat.Dispose();
                hsvMask.Dispose();
                capture.Dispose();
                // Display the Masked image
                DisplayImage(win1, copied);
                // Release Memory
                copied.Dispose();
            }
            CvInvoke.DestroyWindow(win1);
            Panel.SetKey("CV.SubsetHSV", false);
        }

        /// <summary>
        /// ImGui menu for adjusting the Min/Max match values.
        /// </summary>
        public static void RenderHSVSubset()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("DemoCVHSVSubset");

            // This sets up an options for the DemoCV methods.
            var screen = Screen.PrimaryScreen.Bounds;
            // Get the current values from options
            var min = Opts.GetKey<Vector3>("filterSubsetHSVMin");
            var max = Opts.GetKey<Vector3>("filterSubsetHSVMax");
            var x = Opts.GetKey<int>("filterSubsetX");
            var y = Opts.GetKey<int>("filterSubsetY");
            var w = Opts.GetKey<int>("filterSubsetW");
            var h = Opts.GetKey<int>("filterSubsetH");
            var percentage = Opts.GetKey<float>("filterSubsetPercentage");

            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Min", ref min, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                Opts.SetKey("filterSubsetHSVMin", min);

            if (ImGui.ColorPicker3("Filter Max", ref max, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                Opts.SetKey("filterSubsetHSVMax", max);

            if (ImGui.SliderInt("X", ref x, 0, screen.Width - 1))
            {
                if (x + w > screen.Width)
                {
                    w = screen.Width - x;
                    Opts.SetKey("filterSubsetW", w);
                }
                Opts.SetKey("filterSubsetX", x);
            }

            if (ImGui.SliderInt("Y", ref y, 0, screen.Height - 1))
            {
                if (y + h > screen.Height)
                {
                    h = screen.Height - y;
                    Opts.SetKey("filterSubsetH", h);
                }
                Opts.SetKey("filterSubsetY", y);
            }

            if (ImGui.SliderInt("W", ref w, 1, screen.Width - x))
            {
                Opts.SetKey("filterSubsetW", w);
            }

            if (ImGui.SliderInt("H", ref h, 1, screen.Height - y))
            {
                Opts.SetKey("filterSubsetH", h);
            }

            ImGui.LabelText("% ",$"{percentage}");

            ImGui.End();
        }

        private static wrapper.KalmanFilter currFilter = new();
        private static wrapper.KalmanFilter maxFilter = new();
        public static void DemoOCR()
        {
            string win1 = "OCR Matching";
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            Tesseract OCR = new();
            OCR.Init(Path.Join(AppContext.BaseDirectory, "lib", "Tesseract", "tessdata"),"fast",OcrEngineMode.LstmOnly);
            // We create our named window
            CvInvoke.NamedWindow(win1, WindowFlags.FreeRatio);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Set up our local variables
                Vector3 min = Opts.GetKey<Vector3>("OCR.Min");
                Vector3 max = Opts.GetKey<Vector3>("OCR.Max");
                int x = Opts.GetKey<int>("OCR.X");
                int y = Opts.GetKey<int>("OCR.Y");
                int w = Opts.GetKey<int>("OCR.W");
                int h = Opts.GetKey<int>("OCR.H");

                Rectangle subset = new Rectangle(x, y, w, h);
                // Capture the screen
                Mat capture = GetScreenMat(screenBounds);
                Mat screenMat = new Mat(capture, subset);
                // Convert into HSV color space
                Mat hsvMat = new();
                CvInvoke.CvtColor(screenMat, hsvMat, ColorConversion.Bgr2Hsv);
                // Filter the image according to the range values.
                Mat filteredMat = GetFilteredMat(hsvMat, min, max, true);
                // Release Memory
                hsvMat.Dispose();
                // Produce the target mask Mat
                Mat hsvMask = GetBlackWhiteMaskMat(filteredMat);
                // Invert the mask to improve Tesseract matching
                Mat invertMask = new();
                CvInvoke.BitwiseNot(hsvMask, invertMask);
                OCR.SetImage(invertMask);
                string result = OCR.GetUTF8Text().Trim();
                if (result != "")
                {
                    var chars = OCR.GetCharacters();
                    var bound = FindTextBounds(chars,"Life");
                    if (!bound.IsEmpty)
                        App.Log($"We have a match for Life @{bound.X},{bound.Y} - W{bound.Width} H{bound.Height}");
                    // brute force a few scenarios to conform to shape
                    result = Regex.Replace(result, "  ", " ");
                    result = Regex.Replace(result, "[,.]", "");
                    result = Regex.Replace(result, "{", "/");
                    result = Regex.Replace(result, "//", "/");
                    string[] split = result.Split(" ");
                    if (split.Length != 2)
                        App.Log($"Not exactly one space: {result}",4);
                    else
                    {
                        string name = split[0];
                        string[] parts = split[1].Split("/",StringSplitOptions.TrimEntries);
                        if (parts.Length != 2)
                            App.Log($"No divisor: {result}", 4);
                        else
                        {
                            if (!parts[0].All(char.IsDigit) || !parts[1].All(char.IsDigit)) 
                                App.Log($"Not all digits: {result}", 4);
                            else
                            {
                                float current = float.Parse(parts[0]);
                                float maximum = float.Parse(parts[1]);
                                if (current > maximum)
                                    App.Log($"Min above max: {current} / {maximum}",4);
                                else
                                {
                                    float kCurrent = currFilter.Filter(current);
                                    float kMaximum = maxFilter.Filter(maximum);
                                    // We have our non-error condition
                                    double kPercentage = Math.Ceiling( (double)kCurrent / kMaximum * 100);
                                    float percentage = current / maximum;

                                    App.Log($"{name} : {(int)current} / {(int)maximum} = {(int)(percentage * 100)}%\n=> Kalman {(int)kCurrent} / {(int)kMaximum} = {(int)kPercentage}% ",0);
                                }
                            }
                        }
                    }
                }
                // Release Memory
                filteredMat.Dispose();
                invertMask.Dispose();
                // Create a Mat for the result
                Mat copied = new();
                // Copy the image from within the masked area
                screenMat.CopyTo(copied, hsvMask);
                // Release Memory
                screenMat.Dispose();
                hsvMask.Dispose();
                capture.Dispose();
                // Display the Masked image
                DisplayImage(win1, copied);
                // Release Memory
                copied.Dispose();
            }
            CvInvoke.DestroyWindow(win1);
            Panel.SetKey("CV.OCR", false);
            OCR.Dispose();
        }

        public static void RenderOCR()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("Demo OCR");

            var screen = Screen.PrimaryScreen.Bounds;
            // Get the current values from options
            var min = Opts.GetKey<Vector3>("OCR.Min");
            var max = Opts.GetKey<Vector3>("OCR.Max");
            var x = Opts.GetKey<int>("OCR.X");
            var y = Opts.GetKey<int>("OCR.Y");
            var w = Opts.GetKey<int>("OCR.W");
            var h = Opts.GetKey<int>("OCR.H");

            // Render colorpicker widget
            if (ImGui.ColorPicker3("Filter Min", ref min, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                Opts.SetKey("OCR.Min", min);

            if (ImGui.ColorPicker3("Filter Max", ref max, ImGuiColorEditFlags.InputHSV | ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.PickerHueWheel))
                Opts.SetKey("OCR.Max", max);

            if (ImGui.SliderInt("X", ref x, 0, screen.Width - 1))
            {
                if (x + w > screen.Width)
                {
                    w = screen.Width - x;
                    Opts.SetKey("OCR.W", w);
                }
                Opts.SetKey("OCR.X", x);
            }

            if (ImGui.SliderInt("Y", ref y, 0, screen.Height - 1))
            {
                if (y + h > screen.Height)
                {
                    h = screen.Height - y;
                    Opts.SetKey("OCR.H", h);
                }
                Opts.SetKey("OCR.Y", y);
            }

            if (ImGui.SliderInt("W", ref w, 1, screen.Width - x))
            {
                Opts.SetKey("OCR.W", w);
            }

            if (ImGui.SliderInt("H", ref h, 1, screen.Height - y))
            {
                Opts.SetKey("OCR.H", h);
            }

            ImGui.End();
        }

        public static void DemoHWND()
        {
            string win1 = "Capture Window";
            // We create our named window
            CvInvoke.NamedWindow(win1, WindowFlags.FreeRatio);
            // Exit the loop when you press the Escape Key
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                string name = Opts.GetKey<string>("HWND.Name");
                // Capture the Window
                Mat screenMat = GetWindowMat(name);
                if (screenMat == null)
                    continue;
                // Display the Masked image
                DisplayImage(win1, screenMat);
                // Release Memory
                screenMat.Dispose();
            }
            CvInvoke.DestroyWindow(win1);
            Panel.SetKey("CV.WindowHandle", false);
        }

        public static void RenderHWND()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("Demo HWND");

            // This sets up an options for the DemoCV methods.
            // Get the current values from options
            string name = Opts.GetKey<string>("HWND.Name");
            if (ImGui.InputText("Window",ref name, 256))
                Opts.SetKey("HWND.Name", name);

            ImGui.End();
        }

    }
}
