using System.Runtime.InteropServices;
using System;

namespace Triggered.modules.wrapper
{
    internal static class GDI32
    {
        /// <summary>
        /// Copies a bitmap from one device context (source) to another (destination).
        /// </summary>
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        // Copy pixel operation codes (raster-operation codes) for BitBlt function
        internal const int SRCCOPY = 0x00CC0020;
        internal const int SRCPAINT = 0x00EE0086;
        internal const int SRCAND = 0x008800C6;
        internal const int SRCINVERT = 0x00660046;
        internal const int SRCERASE = 0x00440328;
        internal const int NOTSRCCOPY = 0x00330008;
        internal const int NOTSRCERASE = 0x001100A6;
        internal const int MERGECOPY = 0x00C000CA;
        internal const int MERGEPAINT = 0x00BB0226;
        internal const int PATCOPY = 0x00F00021;
        internal const int PATPAINT = 0x00FB0A09;
        internal const int PATINVERT = 0x005A0049;
        internal const int DSTINVERT = 0x00550009;
        internal const int BLACKNESS = 0x00000042;
        internal const int WHITENESS = 0x00FF0062;
    }
}
