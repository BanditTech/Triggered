using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using Emgu.CV;
using ImGuiNET;
using Triggered.modules.demo;
using Triggered.modules.wrapper;
using Triggered.modules.options;

namespace Triggered.modules.panel
{
    internal static  class MainMenu
    {
        private static Options_Viewport Viewport => App.Options.Viewport;
        private static Options_Panel Panel => App.Options.Panel;
        private static Options_Font Font => App.Options.Font;
        private static bool saveProfile = false;
        private static bool loadProfile = false;
        internal static void Render()
        {
            if (!Panel.GetKey<bool>("MainMenu"))
                return;
            bool isCollapsed = !ImGui.Begin(
                "Triggered Options",
                ref App.IsRunning,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.MenuBar);

            // Determine if we need to draw the menu
            if (!App.IsRunning || isCollapsed)
            {
                ImGui.End();
                if (!App.IsRunning)
                    Program.viewport.Close();
                return;
            }

            #region Font Configuration
            var fontIndex = Font.GetKey<int>("Selection");
            var fontSize = Font.GetKey<int>("Size");
            var fontRange = Font.GetKey<int>("Range");
            bool _adjusted = false;
            if (ImGui.Combo("Font", ref fontIndex, App.fonts, App.fonts.Length))
            {
                Font.SetKey("Selection", fontIndex);
                string fontName = App.fonts[fontIndex];
                Font.SetKey("Name", fontName);
                _adjusted = true;
            }
            ImGui.SameLine();
            if (ImGui.InputInt("Size", ref fontSize))
            {
                Font.SetKey("Size", fontSize);
                _adjusted = true;
            }
            if (ImGui.Combo("Glyph Range", ref fontRange, App.glyphs, App.glyphs.Length))
            {
                Font.SetKey("Range", fontRange);
                _adjusted = true;
            }
            if (_adjusted)
            {
                string fontName = App.fonts[fontIndex];
                string fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", $"{fontName}.ttf");
                Program.viewport.ReplaceFont(fontPath, fontSize, (FontGlyphRangeType)fontRange);
            }
            ImGui.Separator();
            #endregion

            var displayLog = Panel.GetKey<bool>("Log");
            if (ImGui.Checkbox("Show/Hide the Log", ref displayLog))
                Panel.SetKey("Log", displayLog);
            var displayStashSorter = Panel.GetKey<bool>("StashSorter");
            if (ImGui.Checkbox("Show/Hide the Stash Sorter", ref displayStashSorter))
                Panel.SetKey("StashSorter", displayStashSorter);
            ImGui.Separator();

            ImGui.Text("F12 button: show/hide this Menu.");
            ImGui.Text("F11 button: show/hide the Stash Sorter.");
            ImGui.Separator();

            #region Demonstration Buttons
            if (ImGui.Button("Launch AHK Demo"))
            {
                Thread thread = new Thread(() =>
                {
                    AHK ahk = new AHK();
                    ahk.Demo();
                });
                thread.Start();
            }
            ImGui.SameLine();
            if (ImGui.Button("Hello World"))
            {
                Task.Run(() =>
                {
                    DemoCV.ShowBlue();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Capture demo"))
            {
                Task.Run(() =>
                {
                    DemoCV.Capture();
                });
            }
            if (ImGui.Button("B/W demo"))
            {
                Panel.SetKey("CV.BlackWhite", true);
                Task.Run(() =>
                {
                    DemoCV.DemoBlackWhite();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Color demo"))
            {
                Panel.SetKey("CV.Color", true);
                Task.Run(() =>
                {
                    DemoCV.DemoColor();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Ind RGB demo"))
            {
                Panel.SetKey("CV.IndividualColor", true);
                Task.Run(() =>
                {
                    DemoCV.DemoIndColor();
                });
            }
            if (ImGui.Button("HSV demo"))
            {
                Panel.SetKey("CV.HSV", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColor();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("HSV Dual demo"))
            {
                Panel.SetKey("CV.DualHSV", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColorDual();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("HSV Subset demo"))
            {
                Panel.SetKey("CV.SubsetHSV", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVSubset();
                });
            }
            if (ImGui.Button("Shape demo"))
            {
                Panel.SetKey("CV.Shape", true);
                Task.Run(() =>
                {
                    DemoCV.DemoShapeDetection();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Rectangle demo"))
            {
                Panel.SetKey("CV.Rectangle", true);
                Task.Run(() =>
                {
                    DemoCV.DemoShapeRectangle();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("OCR demo"))
            {
                Panel.SetKey("CV.OCR", true);
                Task.Run(() =>
                {
                    DemoCV.DemoOCR();
                });
            }
            if (ImGui.Button("HWND demo"))
            {
                Panel.SetKey("CV.WindowHandle", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHWND();
                });
            }
            ImGui.Separator();

            if (ImGui.Button("Planar Subdivision Image"))
            {
                Task.Run(() =>
                {
                    Mat image = DrawSubdivision.Draw(900f, 60);
                    CvInvoke.Imshow("Planar Subdivision", image);
                    image.Dispose();
                    CvInvoke.WaitKey();
                });
            }
            #endregion

            #region Menu Bar
            // This is to show the menu bar that will change the config settings at runtime.
            // If you copied this demo function into your own code and removed ImGuiWindowFlags_MenuBar at the top of the function,
            // you should remove the below if-statement as well.
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Options"))
                {
                    // Disabling fullscreen would allow the window to be moved to the front of other windows,
                    // which we can't undo at the moment without finer window depth/z control.
                    var fullscreen = Viewport.GetKey<bool>("Fullscreen");
                    if (ImGui.MenuItem("Fullscreen", null, ref fullscreen))
                        Viewport.SetKey("Fullscreen", fullscreen);
                    var padding = Viewport.GetKey<bool>("Padding");
                    if (ImGui.MenuItem("Padding", null, ref padding))
                        Viewport.SetKey("Padding", padding);
                    var vsync = Viewport.GetKey<bool>("VSync");
                    if (ImGui.MenuItem("VSync", null, ref vsync))
                    {
                        Program.viewport.VSync = vsync;
                        Viewport.SetKey("VSync", vsync);
                    }
                    ImGui.Separator();

                    // Display a menu item to close this example.
                    if (ImGui.MenuItem("Close", null, false, true))
                        App.IsRunning = false; // Changing this variable to false will close the parent window, therefore closing the Dockspace as well.
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Profile"))
                {
                    if (ImGui.MenuItem("Save"))
                        saveProfile = true;
                    if (ImGui.MenuItem("Load"))
                        loadProfile = true;
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            if (saveProfile && App.Profiles.RenderSave())
                saveProfile = false;
            if (loadProfile && App.Profiles.RenderLoad())
                loadProfile = false;
            #endregion

            // Menu definition complete
            ImGui.End();
        }
    }
}
