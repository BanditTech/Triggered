using ClickableTransparentOverlay.Win32;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Triggered
{
    public static class demoCV
    {
        static bool _capturing = false;
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
            string win1 = "Primary Screen Capture";
            CvInvoke.NamedWindow(win1);
            _capturing = true;
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            while (CvInvoke.WaitKey(1) != (int)Keys.Escape)
            {
                // Capture the primary screen and convert it to an Emgu.CV Mat object
                Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
                using (Graphics g = Graphics.FromImage(screenBitmap))
                {
                    g.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size);
                    Mat screenMat = new Mat();
                    BitmapExtension.ToMat(screenBitmap,screenMat);
                    // Resize the captured screen image and display it in the named window
                    using (Mat frame = new Mat(screenBounds.Height / 2, screenBounds.Width / 2, DepthType.Cv8U, 3))
                    {
                        CvInvoke.Resize(screenMat, frame, frame.Size);
                        CvInvoke.Imshow(win1, frame);
                    }
                }
            }
            CvInvoke.DestroyWindow(win1);
        }
    }
}
