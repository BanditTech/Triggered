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
using System.Drawing;

namespace Triggered.modules.panel
{
    internal static  class MainMenu
    {
        private static Options_Panel Panel => App.Options.Panel;
        private static bool saveProfile = false;
        private static bool loadProfile = false;
        private static bool makeSelection = false;
        private static Rectangle rectSelection = new();
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

            var displayLog = Panel.GetKey<bool>("LogWindow");
            if (ImGui.Checkbox("Show/Hide the Log", ref displayLog))
                Panel.SetKey("LogWindow", displayLog);
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
            ImGui.SameLine();
            if (ImGui.Button("Select Rectangle"))
            {
                makeSelection = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Locations"))
            {
                Panel.SetKey("Locations", !Panel.GetKey<bool>("Locations"));
            }

            #endregion

            if (makeSelection && Selector.Rectangle(ref rectSelection))
            {
                makeSelection = false;
                App.Log($"Rectangle was produced {rectSelection.Left},{rectSelection.Top} - {rectSelection.Bottom},{rectSelection.Right} - W{rectSelection.Width} H{rectSelection.Height}",2);
            }

            #region Menu Bar
            if (ImGui.BeginMenuBar())
            {
                // Display a menu item to close this example.
                if (ImGui.MenuItem("Close", null, false, true))
                    App.IsRunning = false; // Changing this variable to false will close the parent window, therefore closing the Dockspace as well.
                if (ImGui.BeginMenu("Options"))
                {

                    var log = Panel.GetKey<bool>("Log");
                    if (ImGui.MenuItem("Log", null, ref log))
                        Panel.SetKey("Log", log);
                    var locations = Panel.GetKey<bool>("Locations");
                    if (ImGui.MenuItem("Locations", null, ref locations))
                        Panel.SetKey("Locations", locations);
                    var font = Panel.GetKey<bool>("Font");
                    if (ImGui.MenuItem("Font", null, ref font))
                        Panel.SetKey("Font", font);

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
