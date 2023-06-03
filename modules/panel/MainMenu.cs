﻿using System.IO;
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
        private static Options_MainMenu mmOpts => App.Options.MainMenu;
        private static Options_DemoCV cvOpts => App.Options.DemoCV;
        private static string input = mmOpts.GetKey<string>("TestText");
        private static bool saveProfile = false;
        private static bool loadProfile = false;
        internal static void Render()
        {
            bool isCollapsed = !ImGui.Begin(
                "Triggered Options",
                ref App.IsRunning,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.MenuBar);
            // Determine if we need to draw the menu
            if (!App.IsRunning || isCollapsed)
            {
                ImGui.End();
                if (!App.IsRunning)
                {
                    Program.viewport.Close();
                }
                return;
            }
            // Menu definition area
            var fontIndex = mmOpts.GetKey<int>("Font.Index");
            var fontSize = mmOpts.GetKey<int>("Font.Size");
            var fontRange = mmOpts.GetKey<int>("Font.Range");
            bool _adjusted = false;
            if (ImGui.Combo("Font", ref fontIndex, App.fonts, App.fonts.Length))
            {
                mmOpts.SetKey("Font.Index", fontIndex);
                _adjusted = true;
            }
            ImGui.SameLine();
            if (ImGui.InputInt("Size", ref fontSize))
            {
                mmOpts.SetKey("Font.Size", fontSize);
                _adjusted = true;
            }
            if (ImGui.Combo("Glyph Range", ref fontRange, App.glyphs, App.glyphs.Length))
            {
                mmOpts.SetKey("Font.Range", fontRange);
                _adjusted = true;
            }
            if (_adjusted)
            {
                string fontName = App.fonts[fontIndex];
                string fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", $"{fontName}.ttf");
                Program.viewport.ReplaceFont(fontPath, fontSize, (FontGlyphRangeType)fontRange);
            }

            ImGui.Separator();
            int delay = mmOpts.GetKey<int>("LogicTickDelayInMilliseconds");
            if (ImGui.SliderInt("Logic MS", ref delay, 10, 1000))
                mmOpts.SetKey("LogicTickDelayInMilliseconds", delay);
            var displayLog = mmOpts.GetKey<bool>("Display_Log");
            if (ImGui.Checkbox("Show/Hide the Log", ref displayLog))
                mmOpts.SetKey("Display_Log", displayLog);
            var displayStashSorter = mmOpts.GetKey<bool>("Display_StashSorter");
            if (ImGui.Checkbox("Show/Hide the Stash Sorter", ref displayStashSorter))
                mmOpts.SetKey("Display_StashSorter", displayStashSorter);
            ImGui.Separator();
            ImGui.Text("Try pressing F12 button to show/hide this Menu.");
            ImGui.Text("Try pressing F11 button to show/hide the Stash Sorter.");
            ImGui.Separator();
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
                cvOpts.SetKey("Display_AdjustBW", true);
                Task.Run(() =>
                {
                    DemoCV.DemoBlackWhite();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Color demo"))
            {
                cvOpts.SetKey("Display_AdjustColor", true);
                Task.Run(() =>
                {
                    DemoCV.DemoColor();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Ind RGB demo"))
            {
                cvOpts.SetKey("Display_AdjustIndColor", true);
                Task.Run(() =>
                {
                    DemoCV.DemoIndColor();
                });
            }
            if (ImGui.Button("HSV demo"))
            {
                cvOpts.SetKey("Display_AdjustHSVColor", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColor();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("HSV Dual demo"))
            {
                cvOpts.SetKey("Display_AdjustHSVColorDual", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColorDual();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("HSV Subset demo"))
            {
                cvOpts.SetKey("Display_AdjustHSVSubset", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVSubset();
                });
            }
            if (ImGui.Button("Shape demo"))
            {
                cvOpts.SetKey("Display_AdjustShape", true);
                Task.Run(() =>
                {
                    DemoCV.DemoShapeDetection();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Rectangle demo"))
            {
                cvOpts.SetKey("Display_AdjustRectangle", true);
                Task.Run(() =>
                {
                    DemoCV.DemoShapeRectangle();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("OCR demo"))
            {
                cvOpts.SetKey("Display_AdjustOCR", true);
                Task.Run(() =>
                {
                    DemoCV.DemoOCR();
                });
            }
            if (ImGui.Button("HWND demo"))
            {
                cvOpts.SetKey("Display_AdjustHWND", true);
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

            // if we have a change in the Dropdown input, we save it
            if (AffixFilter.DrawTextBox(ref input))
            {
                mmOpts.SetKey("TestText", input);
            }

            // This is to show the menu bar that will change the config settings at runtime.
            // If you copied this demo function into your own code and removed ImGuiWindowFlags_MenuBar at the top of the function,
            // you should remove the below if-statement as well.
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Options"))
                {
                    // Disabling fullscreen would allow the window to be moved to the front of other windows,
                    // which we can't undo at the moment without finer window depth/z control.
                    var fullscreen = mmOpts.GetKey<bool>("Fullscreen");
                    if (ImGui.MenuItem("Fullscreen", null, ref fullscreen))
                        mmOpts.SetKey("Fullscreen", fullscreen);
                    var padding = mmOpts.GetKey<bool>("Padding");
                    if (ImGui.MenuItem("Padding", null, ref padding))
                        mmOpts.SetKey("Padding", padding);
                    var vsync = mmOpts.GetKey<bool>("VSync");
                    if (ImGui.MenuItem("VSync", null, ref vsync))
                    {
                        Program.viewport.VSync = vsync;
                        mmOpts.SetKey("VSync", vsync);
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
            // Menu definition complete
            ImGui.End();
        }
    }
}
