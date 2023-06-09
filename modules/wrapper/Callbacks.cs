using ClickableTransparentOverlay;
using System;
using System.Drawing;
using System.IO;

namespace Triggered.modules.wrapper
{
    public static class Callbacks
    {
        public static void FontIndexEdit(int index)
        {
            if (App.Options == null || App.logger == null || App.fonts == null)
                return;

            App.Log("Callback successfully fired");
            if (App.Options.Font.GetKey<string>("Name") != App.fonts[index])
            {
                App.Options.Font.SetKey("Name", App.fonts[index]);
                SetFont();
            }
        }

        public static void SetFont()
        {
            var fontIndex = App.Options.Font.GetKey<int>("Selection");
            var fontSize = App.Options.Font.GetKey<int>("Size");
            var fontRange = App.Options.Font.GetKey<int>("Range");
            string fontName = App.fonts[fontIndex];
            string fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", $"{fontName}.ttf");
            Program.viewport.ReplaceFont(fontPath, fontSize, (FontGlyphRangeType)fontRange);
        }
    }
}
