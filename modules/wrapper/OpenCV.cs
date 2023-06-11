using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// Wrapper for OpenCV wrapper Emgu.CV.
    /// Double the wrapper for a better wrap.
    /// </summary>
    public static class OpenCV
    {
        /// <summary>
        /// Take the Mat and prepare to display.
        /// Attempts to decide a shape if not provided.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mat"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="channels"></param>
        /// <param name="type"></param>
        public static void DisplayImage(string name, Mat mat, int height = 0, int width = 0, int channels = 3, DepthType type = DepthType.Cv8U)
        {
            if (width <= 0 || height <= 0)
            {
                height = mat.Rows;
                width = mat.Cols;
            }
            else if (width < 10 && height < 10)
            {
                height = mat.Rows / height;
                width = mat.Cols / width;
            }

            // Produce the target frame for the image
            Mat frame = new(height, width, type, channels);
            CvInvoke.Resize(mat, frame, frame.Size); // ==> frame
            CvInvoke.Imshow(name, frame);
            frame.Dispose();
        }

        /// <summary>
        /// Converts the image to grayscale then return the mask.
        /// </summary>
        /// <param name="filteredMat"></param>
        /// <returns></returns>
        public static Mat GetBlackWhiteMaskMat(Mat filteredMat)
        {
            // Produce the target mask Mat
            Mat mask = new();
            // produce gray mask
            Mat grayImage = new Mat();
            // Convert to gray
            CvInvoke.CvtColor(filteredMat, grayImage, ColorConversion.Bgr2Gray);
            // Strip non-white pixels
            CvInvoke.InRange(grayImage, new ScalarArray(255), new ScalarArray(255), mask);
            grayImage.Dispose();
            return mask;
        }

        /// <summary>
        /// Take an image and filter it in a 3 channel range.
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="hsv"></param>
        /// <param name="flipXZ"></param>
        /// <returns></returns>
        public static Mat GetFilteredMat(Mat mat, Vector3 min, Vector3 max, bool hsv = false, bool flipXZ = false)
        {
            // produce target variables
            ScalarArray xMin, xMax, xMin2, xMax2, yMin, yMax, zMin, zMax;
            int x = flipXZ ? 2 : 0;
            int y = 1;
            int z = flipXZ ? 0 : 2;

            // Split the input Mat into H S V channels
            Mat[] channels = mat.Split(); // ==> channels

            // If we have an HSV its possible to wrap around Hue
            // We want to allow a large min and low max
            if (hsv && min.X > max.X)
            {
                // Apply two filters for X channel when hsv is true and min > max
                (xMin, xMax) = ProduceMinMax(min.X, 1f, true);
                (xMin2, xMax2) = ProduceMinMax(0f, max.X, true);

                // Apply InRange to the first copy of the X channel
                Mat xMask1 = new Mat();
                CvInvoke.InRange(channels[x], xMin, xMax, xMask1);

                // Apply InRange to the second copy of the X channel
                Mat xMask2 = new Mat();
                CvInvoke.InRange(channels[x], xMin2, xMax2, xMask2);

                // Combine the two X masks
                CvInvoke.BitwiseOr(xMask1, xMask2, channels[x]);
                xMask1.Dispose();
                xMask2.Dispose();
            }
            else
            {
                (xMin, xMax) = ProduceMinMax(min.X, max.X, hsv);
                CvInvoke.InRange(channels[x], xMin, xMax, channels[x]);
            }

            // Take color points and produce ranges from them
            (yMin, yMax) = ProduceMinMax(min.Y, max.Y);
            (zMin, zMax) = ProduceMinMax(min.Z, max.Z);

            CvInvoke.InRange(channels[y], yMin, yMax, channels[y]);
            CvInvoke.InRange(channels[z], zMin, zMax, channels[z]);

            // Merge the channels back into a Mat
            return GetMergedMat(channels);
        }

        /// <summary>
        /// Produce a standard Mat from a Mat[].
        /// </summary>
        /// <param name="channels"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Fetches a capture of the graphics context.
        /// </summary>
        /// <param name="screenBounds"></param>
        /// <returns>The Mat of the screen image</returns>
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
            // Convert into a Matrix
            Mat screenMat = new();
            screenBitmap.ToMat(screenMat); // ==> screenMat
            // Release Memory
            screenBitmap.Dispose();
            return screenMat;
        }

        /// <summary>
        /// Capture a specific window
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static Mat GetWindowMat(IntPtr hwnd)
        {
            // Get the window bounds
            if (!User32.GetWindowRect(hwnd, out User32.RECT windowRect))
                return null;
            Rectangle screenBounds = new Rectangle(windowRect.Left, windowRect.Top, windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);

            // Produce bounds to capture the specified window
            Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);

            // Prepare our graphics context
            using (Graphics graphicAdjust = Graphics.FromImage(screenBitmap))
            {
                // Copy the image from the location into the top left corner of screenBitmap
                IntPtr hdcSrc = User32.GetWindowDC(hwnd);
                IntPtr hdcDest = graphicAdjust.GetHdc();
                GDI32.BitBlt(hdcDest, 0, 0, screenBounds.Width, screenBounds.Height, hdcSrc, 0, 0, GDI32.SRCCOPY);
                graphicAdjust.ReleaseHdc(hdcDest);
                User32.ReleaseDC(hwnd, hdcSrc);
            }

            // Convert into a Matrix
            Mat screenMat = new Mat();
            screenBitmap.ToMat(screenMat);
            screenBitmap.Dispose();
            return screenMat;
        }

        /// <summary>
        /// Capture a named window
        /// </summary>
        /// <param name="windowName"></param>
        /// <returns></returns>
        public static Mat GetWindowMat(string windowName)
        {
            IntPtr hWnd = User32.GetWindowHandle(windowName);
            if (hWnd != IntPtr.Zero)
                return GetWindowMat(hWnd);
            return null;
        }

        /// <summary>
        /// Determine how many rows before a non-zero pixel.
        /// </summary>
        /// <returns>Number of Empty Rows</returns>
        public static int GetEmptyRowsFromTop(Mat bwMask)
        {
            // Convert grayMask to Image
            using (Bitmap bitmap = bwMask.ToBitmap())
            {
                // Find the first white pixel
                int totalRows = bitmap.Height;
                int emptyRows = 0;

                for (int row = 0; row < totalRows; row++)
                {
                    for (int col = 0; col < bitmap.Width; col++)
                    {
                        Color pixelColor = bitmap.GetPixel(col, row);
                        if (pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255)
                            return emptyRows;
                    }
                    emptyRows++;
                }
                return emptyRows;
            }
        }

        /// <summary>
        /// Retrieves the number of rows from the bottom of the image where the first white pixel is encountered.
        /// </summary>
        /// <param name="bwMask">The binary mask image (Mat) to process.</param>
        /// <returns>The number of matching rows from the bottom of the image.</returns>
        public static int GetMatchingRowsFromBottom(Mat bwMask)
        {
            // Convert grayMask to Image
            using (Bitmap bitmap = bwMask.ToBitmap())
            {
                // Find the first white pixel
                int totalRows = bitmap.Height;
                int matchRows = 0;

                for (int row = 0; row < totalRows; row++)
                {
                    bool found = false;
                    for (int col = 0; col < bitmap.Width; col++)
                    {
                        Color pixelColor = bitmap.GetPixel(col, row);
                        if (pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        matchRows++;
                    else
                        break;
                }
                return matchRows;
            }
        }

        /// <summary>
        /// Finds the positions (bounding rectangles) of multiple target strings within an array of Tesseract.Character objects.
        /// </summary>
        /// <param name="characters">An array of Tesseract.Character objects representing the individual characters.</param>
        /// <param name="targetStrings">A list of target strings to search for.</param>
        /// <returns>A dictionary containing the target strings as keys and their corresponding bounding rectangles as values.</returns>
        public static Dictionary<string, Rectangle> FindTextPositions(Tesseract.Character[] characters, List<string> targetStrings)
        {
            Dictionary<string, Rectangle> textPositions = new();

            foreach (string targetString in targetStrings)
            {
                var result = FindTextBounds(characters, targetString);
                if (!result.IsEmpty)
                {
                    textPositions.Add(targetString, result);
                }
            }

            return textPositions;
        }

        /// <summary>
        /// Finds the bounding rectangle encompassing the characters that form the specified target text.
        /// </summary>
        /// <param name="characters">An array of Tesseract.Character objects representing the individual characters.</param>
        /// <param name="target">The target text to search for.</param>
        /// <returns>The bounding rectangle that encompasses the characters forming the target text, or the default rectangle if not found.</returns>
        public static Rectangle FindTextBounds(Tesseract.Character[] characters, string target)
        {
            List<Rectangle> boundingBoxes = new();
            string putty = string.Empty;
            for (int i = 0; i < characters.Length; i++)
            {
                var nullPutty = string.IsNullOrEmpty(putty);
                var containPutty = nullPutty || (!nullPutty && target.Contains(putty));
                if (target.Contains(characters[i].Text) && containPutty)
                {
                    putty = putty == string.Empty ? characters[i].Text : putty + characters[i].Text;
                    boundingBoxes.Add(characters[i].Region);
                    if (putty == target)
                    {
                        Rectangle bounds = boundingBoxes[0];
                        foreach (Rectangle box in boundingBoxes)
                        {
                            bounds = Rectangle.Union(box, bounds);
                        }
                        return bounds;
                    }
                }
                else
                {
                    // reset the comparison
                    putty = string.Empty;
                    boundingBoxes.Clear();
                }
            }
            return default;
        }

        /// <summary>
        /// Validates the proximity of characters in a list based on maximum vertical and horizontal distances.
        /// </summary>
        /// <param name="characters">A list of Tesseract.Character objects representing the characters to validate.</param>
        /// <param name="maxVerticalDistance">The maximum allowed vertical distance between consecutive characters.</param>
        /// <param name="maxHorizontalDistance">The maximum allowed horizontal distance between consecutive characters.</param>
        /// <returns>True if the characters meet the proximity criteria, False otherwise.</returns>
        public static bool ValidateProximity(List<Tesseract.Character> characters, int maxVerticalDistance = 10, int maxHorizontalDistance = 50)
        {
            if (characters.Count < 2)
                return true;

            // Iterate over the characters to check the distance between consecutive characters
            for (int i = 1; i < characters.Count; i++)
            {
                int verticalDistance = Math.Abs(characters[i].Region.Y - characters[i - 1].Region.Y);
                int horizontalDistance = Math.Abs(characters[i].Region.X - characters[i - 1].Region.X);
                if (verticalDistance > maxVerticalDistance)
                    return false;
                if (horizontalDistance > maxHorizontalDistance)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Return a float representing the percentage from the top until non zero.
        /// </summary>
        /// <param name="bwMask"></param>
        /// <returns>Float value between 0f and 1f</returns>
        public static float GetMaskPercentage(Mat bwMask)
        {
            float totalRows = bwMask.Rows;
            float emptyRows = GetEmptyRowsFromTop(bwMask);
            //float matchRows = GetMatchingRowsFromBottom(bwMask);
            float result = (totalRows - emptyRows) / totalRows;
            //float result = matchRows / totalRows;
            return result;
        }

        /// <summary>
        /// Take two floating point values and create ScalarArray.
        /// Formats the values according to OpenCV style.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="hue"></param>
        /// <returns></returns>
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
        /// <param name="hue"></param>
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
                min -= max - 1f;
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