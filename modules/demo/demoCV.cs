using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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
        /// <summary>
        /// Renders any active OpenCV demonstration window.
        /// </summary>
        public static void Render()
        {
            if (Panel.GetKey<bool>("CV.Rectangle"))
                RenderShapeRectangle();
            if (Panel.GetKey<bool>("CV.SubsetHSV"))
                RenderHSVSubset();
            if (Panel.GetKey<bool>("CV.OCR"))
                RenderOCR();
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
    }
}
