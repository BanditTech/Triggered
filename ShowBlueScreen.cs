using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

namespace Triggered
{
    public class ShowBlueScreen
    {
        ImageViewer viewer;
        public ShowBlueScreen()
        {
            //Create a 3 channel image of 400x200
            using (Mat img = new Mat(200, 400, DepthType.Cv8U, 3))
            {
                img.SetTo(new Bgr(255, 0, 0).MCvScalar); // set it to Blue color

                //Draw "Hello, world." on the image using the specific font
                CvInvoke.PutText(
                   img,
                   "Hello, world",
                   new System.Drawing.Point(10, 80),
                   FontFace.HersheyComplex,
                   1.0,
                   new Bgr(0, 255, 0).MCvScalar);
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
    }
}

/* System.NotSupportedException
 * HResult=0x80131515
 * Message=Use of ResourceManager for custom types is disabled. Set the MSBuild Property CustomResourceTypesSupport to true in order to enable it.
 * Source=System.Private.CoreLib
 * StackTrace:
 *  at System.Resources.ManifestBasedResourceGroveler.CreateResourceSet(Stream store, Assembly assembly)
 *  at System.Resources.ResourceManager.GetResourceSet(CultureInfo culture, Boolean createIfNotExists, Boolean tryParents)
 *  at System.ComponentModel.ComponentResourceManager.FillResources(CultureInfo culture, ResourceSet& resourceSet)
 *  at System.ComponentModel.ComponentResourceManager.FillResources(CultureInfo culture, ResourceSet& resourceSet)
 *  at System.ComponentModel.ComponentResourceManager.FillResources(CultureInfo culture, ResourceSet& resourceSet)
 *  at System.ComponentModel.ComponentResourceManager.ApplyResources(Object value, String objectName, CultureInfo culture)
 *  at Emgu.CV.UI.ImageViewer.InitializeComponent()
 *  at Emgu.CV.UI.ImageViewer..ctor()
 *  at Emgu.CV.UI.ImageViewer..ctor(IInputArray image)
 *  at Emgu.CV.UI.ImageViewer..ctor(IInputArray image, String imageName)
 *  at Emgu.CV.UI.ImageViewer.Show(IInputArray image, String windowName)
 *  at Triggered.ShowBlueScreen..ctor() in C:\Users\thebb\source\repos\Triggered\ShowBlueScreen.cs:line 30
 *  at Triggered.App.<>c.<.cctor>b__6_0() in C:\Users\thebb\source\repos\Triggered\App.cs:line 61
 *  at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
 */
