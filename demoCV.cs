using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

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
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            Rectangle drawBox = new Rectangle(100, 100, 400, 400);
            using Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using Mat frame = new Mat(screenBounds.Height / 2, screenBounds.Width / 2, DepthType.Cv8U, 3);
            using Mat screenMat = new Mat();
            string win1 = "Primary Screen Capture";
            CvInvoke.NamedWindow(win1);
            int direction;
            var rnd = new Random();
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Capture the primary screen and convert it to a System.Drawing.Bitmap object
                using (Graphics g = Graphics.FromImage(screenBitmap))
                {
                    // Copy the pixels of the screen into the bitmap
                    // screenBounds.Location is the upper left corner of the screen rectangle
                    // Point.Empty is the offset within the bitmap to start copying the pixels from
                    // screenBounds.Size is the size of the rectangle to copy
                    g.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
                    // Convert the bitmap to an Emgu.CV Mat object
                    // screenMat is the output Mat object
                    BitmapExtension.ToMat(screenBitmap, screenMat);
                }

                // The Graphics object has been released
                // At this point the screen data is contained within screenMat

                #region Random Direction Fun
                direction = rnd.Next(1,5);
                // Check if the drawBox rectangle is within the screen bounds
                if (drawBox.X < 0)
                {
                    drawBox.X = 0;
                    direction = 4; // Change direction to right
                }
                else if (drawBox.X + drawBox.Width > screenBounds.Width)
                {
                    drawBox.X = screenBounds.Width - drawBox.Width;
                    direction = 3; // Change direction to left
                }
                if (drawBox.Y < 0)
                {
                    drawBox.Y = 0;
                    direction = 2; // Change direction to down
                }
                else if (drawBox.Y + drawBox.Height > screenBounds.Height)
                {
                    drawBox.Y = screenBounds.Height - drawBox.Height;
                    direction = 1; // Change direction to up
                }
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
                #endregion

                // Draw a rectangle on the image
                CvInvoke.Rectangle(screenMat, drawBox, new MCvScalar(0, 255, 0), default, LineType.AntiAlias);
                // Resize the captured screen image and display it in the named window
                CvInvoke.Resize(screenMat, frame, frame.Size);
                CvInvoke.Imshow(win1, frame);
            }
            CvInvoke.DestroyWindow(win1);
        }
    }
}
