using ImGuiNET;
using System.Collections.Generic;

namespace Triggered.modules.wrapper
{
    internal class ImHotkey
    {
        private List<Key> keyList;
        public ImHotkey()
        {
            keyList = new List<Key>();
        }
        public struct HotKey
        {
            public string functionName;
            public string functionLib;
            public uint functionKeys;
        }

        public struct Key
        {
            public Key(string lib = "", uint order = 0, uint scanCodePage1 = 0, uint scanCodePage7 = 0, int offset = 0, int width = 40)
            {
                this.lib = lib;
                this.order = order;
                this.scanCodePage1 = scanCodePage1; // win32 scancode https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
                this.scanCodePage7 = scanCodePage7; // HID (SDL,...) https://gist.github.com/MightyPork/6da26e382a7ad91b5496ee55fdc73db2
                this.offset = offset;
                this.width = width;
            }

            public string lib;
            public uint order;
            public uint scanCodePage1; // win32 scancode
            public uint scanCodePage7; // HID (SDL,...)
            public int offset;
            public int width;
        }

        public Key GetKeyForScanCode(uint scanCode)
        {
            Key key;
            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    if (Keys[y, x].lib != "")
                        key = Keys[y, x];
                    else
                        continue;
                    if (key.scanCodePage1 == scanCode || key.scanCodePage7 == scanCode)
                    {
                        return key;
                    }
                }
            }

            return default;
        }

        public static readonly Key[,] Keys = new Key[6, 17]
        {
            { new Key("Esc", 4, 0x1, 0x29, 18), new Key("F1", 5, 0x3B, 0x3A, 18), new Key("F2", 6, 0x3C, 0x3B), new Key("F3", 7, 0x3D, 0x3C),
                new Key("F4", 8, 0x3E, 0x3D), new Key("F5", 9, 0x3F, 0x3E, 24), new Key("F6", 10, 0x40, 0x3F), new Key("F7", 11, 0x41, 0x40),
                new Key("F8", 12, 0x42, 0x41), new Key("F9", 13, 0x43, 0x42, 24), new Key("F10", 14, 0x44, 0x43), new Key("F11", 15, 0x57, 0x44),
                new Key("F12", 16, 0x58, 0x45), new Key("PrSn", 17, 0x37, 0x46, 24), new Key("ScLk", 18, 0x46), new Key("Brk", 19, 126, 0x47),
                new Key() },
            { new Key("~", 20, 0x29, 0x35), new Key("1", 21, 0x2, 0x1E), new Key("2", 22, 0x3, 0x1F), new Key("3", 23, 0x4, 0x20),
                new Key("4", 24, 0x5, 0x21), new Key("5", 25, 0x6, 0x22), new Key("6", 26, 0x7, 0x23), new Key("7", 27, 0x8, 0x24),
                new Key("8", 28, 0x9, 0x25), new Key("9", 29, 0xA, 0x26), new Key("0", 30, 0xB, 0x27), new Key("-", 31, 0xC, 0x2D),
                new Key("+", 32, 0xD, 0x2E), new Key("Backspace", 33, 0xE, 0x2A, 0, 80), new Key("Ins", 34, 0x52, 0x49, 24),
                new Key("Hom", 35, 0x47, 0x4A), new Key("PgU", 36, 0x49, 0x4B) },
            { new Key("Tab", 3, 0xF, 0x2B, 0, 60), new Key("Q", 37, 0x10, 0x14), new Key("W", 38, 0x11, 0x1A), new Key("E", 39, 0x12, 0x08),
                new Key("R", 40, 0x13, 0x15), new Key("T", 41, 0x14, 0x17), new Key("Y", 42, 0x15, 0x1C), new Key("U", 43, 0x16, 0x18),
                new Key("I", 44, 0x17, 0x0C), new Key("O", 45, 0x18, 0x12), new Key("P", 46, 0x19, 0x13), new Key("[", 47, 0x1A, 0x2F),
                new Key("]", 48, 0x1B, 0x30), new Key("|", 49, 0x2B, 0x31, 0, 60), new Key("Del", 50, 0x53, 0x4C, 24),
                new Key("End", 51, 0x4F, 0x4D), new Key("PgD", 52, 0x51, 0x4E)},
            { new Key("Caps Lock", 53, 0x3A, 0x39, 0, 80), new Key("A", 54, 0x1E, 0x04), new Key("S", 55, 0x1F, 0x16),
                new Key("D", 56, 0x20, 0x07), new Key("F", 57, 0x21, 0x09), new Key("G", 58, 0x22, 0x0A), new Key("H", 59, 0x23, 0x0B),
                new Key("J", 60, 0x24, 0x0D), new Key("K", 61, 0x25, 0x0E), new Key("L", 62, 0x26, 0x0F), new Key(";", 63, 0x27, 0x33),
                new Key("'", 64, 0x28, 0x34), new Key("Ret", 65, 0x1C, 0X28, 0, 84),
                new Key(), new Key(), new Key(), new Key() },
            { new Key("Shift", 2, 0x2A, 0xE1, 0, 104), new Key("Z", 66, 0x2C, 0x1D), new Key("X", 67, 0x2D, 0x1B),
                new Key("C", 68, 0x2E, 0x06), new Key("V", 69, 0x2F, 0x19), new Key("B", 70, 0x30, 0x05), new Key("N", 71, 0x31, 0x11),
                new Key("M", 72, 0x32, 0x10), new Key(",", 73, 0x33, 0x36), new Key(".", 74, 0x34, 0x37), new Key("/", 75, 0x35, 0x38),
                new Key("Shift", 2, 0x2A, 0xE5, 0, 104), new Key("Up", 76, 0x48, 0x52, 68),
                new Key(), new Key(), new Key(), new Key() },
            { new Key("Ctrl", 0, 0x1D, 0xE0, 0, 60), new Key("Alt", 1, 0x38, 0xE2, 68, 60), new Key("Space", 77, 0x39, 0X2c, 0, 260),
                new Key("Alt", 1, 0x38, 0xE6, 0, 60), new Key("Ctrl", 0, 0x1D, 0xE4, 68, 60), new Key("Left", 78, 0x4B, 0x50, 24),
                new Key("Down", 79, 0x50, 0x51), new Key("Right", 80, 0x4D, 0x52),
                new Key(), new Key(), new Key(), new Key(), new Key(), new Key(), new Key(), new Key(), new Key() }
        };
    }
}
