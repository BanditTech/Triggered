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
using System.Drawing;

namespace Triggered.modules.panel
{
    internal static  class MainMenu
    {
        private static Options_Panel Panel => App.Options.Panel;
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

            ImGui.Text("F12 button: toggle menu.");
            ImGui.Separator();

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
                if (ImGui.BeginMenu("Window"))
                {
                    var stashSorter = Panel.GetKey<bool>("StashSorter");
                    if (ImGui.MenuItem("StashSorter", null, ref stashSorter))
                        Panel.SetKey("StashSorter", stashSorter);
                    var log = Panel.GetKey<bool>("LogWindow");
                    if (ImGui.MenuItem("Log Window", null, ref log))
                        Panel.SetKey("LogWindow", log);

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
