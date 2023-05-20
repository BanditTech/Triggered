using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Windows.Forms;
using Triggered.modules.panels;
using Vortice;

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
            // Convert the graphics context into a bitmap
            Mat screenMat = new();
            screenBitmap.ToMat(screenMat); // ==> screenMat
            // Release Memory
            screenBitmap.Dispose();
            return screenMat;
        }

        /// <summary>
        /// Determine how many rows before a non-zero pixel.
        /// </summary>
        /// <returns>Number of Empty Rows</returns>
        public static int GetEmptyRows(Mat bwMask)
        {
            // Ensure bwMask is single-channel
            Mat grayMask = new Mat();
            if (bwMask.NumberOfChannels > 1)
                CvInvoke.CvtColor(bwMask, grayMask, ColorConversion.Bgr2Gray);
            else
                grayMask = bwMask.Clone();

            // Convert grayMask to Image
            Bitmap bitmap = grayMask.ToBitmap();
            grayMask.Dispose();
            // Find the first white pixel
            int totalRows = bitmap.Height;
            int emptyRows = 0;

            for (int row = 0; row < totalRows; row++)
            {
                bool isRowEmpty = true;
                for (int col = 0; col < bitmap.Width; col++)
                {
                    Color pixelColor = bitmap.GetPixel(col, row);
                    if (pixelColor.R == 255 && pixelColor.G == 255 && pixelColor.B == 255)
                    {
                        isRowEmpty = false;
                        break;
                    }
                }

                if (isRowEmpty)
                    emptyRows++;
                else
                    break;
            }
            bitmap.Dispose();

            return emptyRows;
        }

        /// <summary>
        /// Return a float representing the percentage from the top until non zero.
        /// </summary>
        /// <param name="bwMask"></param>
        /// <returns>Float value between 0f and 1f</returns>
        public static float GetMaskPercentage(Mat bwMask)
        {
            float totalRows = bwMask.Rows;
            float emptyRows = GetEmptyRows(bwMask);
            float result = (totalRows - emptyRows) / totalRows;
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