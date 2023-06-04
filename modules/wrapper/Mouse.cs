namespace Triggered.modules.wrapper
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wrapper class for Win32 API calls.
    /// </summary>
    public class Mouse
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, UIntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_XDOWN = 0x0080;
        private const uint MOUSEEVENTF_XUP = 0x0100;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_HWHEEL = 0x1000;
        private static TextInfo textInfo = CultureInfo.GetCultureInfo("en-US").TextInfo;
        
        /// <summary>
        /// General method for calling all other Mouse clicks:<br/>
        /// - "Left"       - <see cref="Mouse.Left"/><br/>
        /// - "Right"      - <see cref="Mouse.Right"/><br/>
        /// - "Middle"     - <see cref="Mouse.Middle"/><br/>
        /// - "M4"         - <see cref="Mouse.M4"/><br/>
        /// - "M5"         - <see cref="Mouse.M5"/><br/>
        /// - "WheelUp"    - <see cref="Mouse.WheelUp"/><br/>
        /// - "WheelDown"  - <see cref="Mouse.WheelDown"/><br/>
        /// - "WheelLeft"  - <see cref="Mouse.WheelLeft"/><br/>
        /// - "WheelRight" - <see cref="Mouse.WheelRight"/><br/>
        /// </summary>
        /// <param name="methodname"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void Click(string methodname, int x, int y)
        {
            string MethodName = textInfo.ToTitleCase(methodname);
            var mouse = new Mouse();
            var method = typeof(Mouse).GetMethod(MethodName);
            if (method != null)
                method.Invoke(mouse, new object[] { x, y });
            else
                throw new ArgumentException($"Invalid click method name: {methodname}");
        }

        /// <summary>
        /// Left click at location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Left(int x, int y)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// Right click at location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Right(int x, int y)
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)x, (uint)y, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// Middle click at location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void Middle(int x, int y)
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_MIDDLEUP, (uint)x, (uint)y, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// M4 Click at location.
        /// Also known as Browser_Back or XButton1.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void M4(int x, int y)
        {
            mouse_event(MOUSEEVENTF_XDOWN | MOUSEEVENTF_XUP, (uint)x, (uint)y, 1, UIntPtr.Zero);
        }

        /// <summary>
        /// M5 Click at location.
        /// Also known as Browser_Forward or XButton2.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void M5(int x, int y)
        {
            mouse_event(MOUSEEVENTF_XDOWN | MOUSEEVENTF_XUP, (uint)x, (uint)y, 2, UIntPtr.Zero);
        }

        /// <summary>
        /// Turn the wheel upward (away from you) at location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void WheelUp(int x, int y)
        {
            mouse_event(MOUSEEVENTF_WHEEL, (uint)x, (uint)y, 120, UIntPtr.Zero);
        }

        /// <summary>
        /// Turn the wheel downward (toward you) at location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void WheelDown(int x, int y)
        {
            mouse_event(MOUSEEVENTF_WHEEL, (uint)x, (uint)y, -120, UIntPtr.Zero);
        }

        /// <summary>
        /// Scroll to the left at location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void WheelLeft(int x, int y)
        {
            mouse_event(MOUSEEVENTF_HWHEEL, (uint)x, (uint)y, -120, UIntPtr.Zero);
        }

        /// <summary>
        /// Scroll to the right at location.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void WheelRight(int x, int y)
        {
            mouse_event(MOUSEEVENTF_HWHEEL, (uint)x, (uint)y, 120, UIntPtr.Zero);
        }
    }
}
