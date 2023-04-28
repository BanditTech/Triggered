﻿namespace Triggered
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Numerics;
    using System.Text.Json;
    using System.Threading;
    using ClickableTransparentOverlay;
    using ClickableTransparentOverlay.Win32;
    using ImGuiNET;
    using Newtonsoft.Json;

    public class TriggeredMenu : Overlay
    {
        private readonly Thread logicThread;
        // Begin the LogicUpdate thread when the frame initiates
        //[RequiresUnreferencedCode("Calls App.UpdateTopGroups()")]
        //[RequiresDynamicCode("Calls App.UpdateTopGroups()")]
        public TriggeredMenu()
        {
            //App.UpdateTopGroups();
            DumpExampleJson();
            logicThread = new Thread(() =>
            {
                while (App.IsRunning)
                {
                    LogicUpdate();
                }
            });
            logicThread.Start();
        }
        private void LogicUpdate()
        {
            Thread.Sleep(App.LogicTickDelayInMilliseconds);
            App.Log("test Message", ExampleAppLog.logLevels[ExampleAppLog.logLevelIndex]);
            ExampleAppLog.logLevelIndex = (ExampleAppLog.logLevelIndex + 1) % ExampleAppLog.logLevels.Length;
        }
        // Render thread is run every frame
        // We can piggy back on the render thread for simple keybinds
        protected override void Render()
        {
            if (Utils.IsKeyPressedAndNotTimeout(VK.F12)) //F12.
            {
                App.MenuDisplay_Main = !App.MenuDisplay_Main;
            }

            if (Utils.IsKeyPressedAndNotTimeout(VK.F11)) //F11.
            {
                App.MenuDisplay_StashSorter = !App.MenuDisplay_StashSorter;
            }
            // We always render this invisible window
            // Having no viewport will break the children docks
            RenderViewPort();

            if (App.MenuDisplay_Main)
                RenderMainMenu();
            if (App.MenuDisplay_StashSorter)
                RenderStashSorter();
            if (App.MenuDisplay_Log)
                RenderLogWindow();

            return;
        }
        private void RenderLogWindow()
        {
            App.logimgui.Draw("Log Window", true);
        }
        private void RenderMainMenu()
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
                    Close();
                }
                return;
            }
            // Menu definition area
            float delay = App.LogicTickDelayInMilliseconds;
            ImGui.SliderFloat("Logic MS", ref delay, 10.0f, 1000.0f);
            App.LogicTickDelayInMilliseconds = (int)delay;
            ImGui.Checkbox("Show/Hide the Log", ref App.MenuDisplay_Log);
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

            if (ImGui.CollapsingHeader("Collapsible Group Box Label", ref App.ShowGroupBoxContents))
            {
                ImGui.Text("This is inside the collapsible group box.");
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
                    ImGui.MenuItem("Fullscreen", null, ref App.fullscreen);
                    ImGui.MenuItem("Padding", null, ref App.padding);
                    ImGui.Separator();

                    // Display a menu item to close this example.
                    if (ImGui.MenuItem("Close", null, false, true))
                        App.IsRunning = false; // Changing this variable to false will close the parent window, therefore closing the Dockspace as well.
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            // Menu definition complete
            ImGui.End();
        }
        private void RenderViewPort()
        {
            // Variables to configure the Dockspace example.
            // Includes App.fullscreen, App.padding
            ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.None | ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.AutoHideTabBar;

            // In this example, we're embedding the Dockspace into an invisible parent window to make it more configurable.
            // We set ImGuiWindowFlags_NoDocking to make sure the parent isn't dockable into because this is handled by the Dockspace.
            //
            // ImGuiWindowFlags_MenuBar is to show a menu bar with config options. This isn't necessary to the functionality of a
            // Dockspace, but it is here to provide a way to change the configuration flags interactively.
            // You can remove the MenuBar flag if you don't want it in your app, but also remember to remove the code which actually
            // renders the menu bar, found at the end of this function.
            ImGuiWindowFlags window_flags = ImGuiWindowFlags.NoDocking;

            // Is the example in Fullscreen mode?
            if (App.fullscreen)
            {
                // If so, get the main viewport:
                var viewport = ImGui.GetMainViewport();

                // Set the parent window's position, size, and viewport to match that of the main viewport. This is so the parent window
                // completely covers the main viewport, giving it a "full-screen" feel.
                ImGui.SetNextWindowPos(viewport.WorkPos);
                ImGui.SetNextWindowSize(viewport.WorkSize);
                ImGui.SetNextWindowViewport(viewport.ID);

                // Set the parent window's styles to match that of the main viewport:
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f); // No corner rounding on the window
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f); // No border around the window
                // Set the alpha component of the window background color to 0 to make it transparent
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));

                // Manipulate the window flags to make it inaccessible to the user (no titlebar, resize/move, or navigation)
                window_flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
                window_flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.DockNodeHost;
            }
            else
            {
                // The example is not in Fullscreen mode (the parent window can be dragged around and resized), disable the
                // ImGuiDockNodeFlags_PassthruCentralNode flag.
                dockspace_flags &= ~ImGuiDockNodeFlags.PassthruCentralNode;
            }

            // When using ImGuiDockNodeFlags_PassthruCentralNode, DockSpace() will render our background
            // and handle the pass-thru hole, so the parent window should not have its own background:
            if ((dockspace_flags & ImGuiDockNodeFlags.PassthruCentralNode) != 0)
                window_flags |= ImGuiWindowFlags.NoBackground;

            // If the padding option is disabled, set the parent window's padding size to 0 to effectively hide said padding.
            if (!App.padding)
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

            // Important: note that we proceed even if Begin() returns false (aka window is collapsed).
            // This is because we want to keep our DockSpace() active. If a DockSpace() is inactive,
            // all active windows docked into it will lose their parent and become undocked.
            // We cannot preserve the docking relationship between an active window and an inactive docking, otherwise
            // any change of dockspace/settings would lead to windows being stuck in limbo and never being visible.
            ImGui.Begin("DockSpace Demo", ref App.IsRunning, window_flags);

            // Remove the padding configuration - we pushed it, now we pop it:
            if (!App.padding)
                ImGui.PopStyleVar();

            // Pop the two style rules set in Fullscreen mode - the corner rounding and the border size.
            if (App.fullscreen)
            {
                ImGui.PopStyleColor();
                ImGui.PopStyleVar(2);
            }
            // Check if Docking is enabled:
            ImGuiIOPtr io = ImGui.GetIO();
            if ((io.ConfigFlags & ImGuiConfigFlags.DockingEnable) != 0)
            {
                // Set the alpha component of the docking area background color to 0 to make it transparent
                ImGui.PushStyleColor(ImGuiCol.DockingEmptyBg, new Vector4(0, 0, 0, 0));
                // If it is, draw the Dockspace with the DockSpace() function.
                // The GetID() function is to give a unique identifier to the Dockspace - here, it's "MyDockSpace".
                uint dockspace_id = ImGui.GetID("MyDockSpace");
                ImGui.DockSpace(dockspace_id, new Vector2(0.0f, 0.0f), dockspace_flags);
                ImGui.PopStyleColor();
            }
            else
            {
                // Docking is DISABLED - Show a warning message
                App.Log("Docking is disabled!");
            }
            // End the parent window that contains the Dockspace:
            ImGui.End();
        }
        private void RenderStashSorter()
        {
            // Create the main window
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(500, 500), ImGuiCond.FirstUseEver);
            ImGui.Begin("Edit TopGroup");

            // Select the TopGroup
            ImGui.Combo("Top Group", ref App.SelectedGroup, App.TopGroups, App.TopGroups.Length);

            //// Edit the TopGroup
            //ImGui.Text("Elements:");
            //foreach (Element element in App.StashSorterFile)
            //{
            //    ImGui.Text(element.Key);
            //    ImGui.SameLine();
            //    ImGui.Text(element.Min.ToString());
            //}

            // End the main window
            ImGui.End();
        }
        private void DumpExampleJson()
        {
            Group examplegroup = new Group("AND",0);
            Element exampleelement = new Element("KeyName to Match",">=","Value to Match with");
            //exampleelement.Dump("element");
            examplegroup.AddElement(exampleelement);
            //examplegroup.Dump("group");

            TopGroup example1 = new TopGroup("Example 1 AND", "AND", default, default, default);
            example1.AddElement(examplegroup);
            TopGroup example2 = new TopGroup("Example 2 NOT","NOT","1",1,1);
            example1.AddElement(examplegroup);
            example1.AddElement(exampleelement);
            TopGroup example2 = new TopGroup("Example 2 NOT", "NOT", default, default, default);
            example2.AddElement(exampleelement);
            TopGroup example3 = new TopGroup("Example 3 COUNT", "COUNT", default, default, default);
            example2.AddElement(exampleelement);
            TopGroup example4 = new TopGroup("Example 4 WEIGHT", "WEIGHT", default, default, default);
            example2.AddElement(exampleelement);
            List<TopGroup> dumpthis = new List<TopGroup>();
            dumpthis.Add(example1);
            dumpthis.Add(example2);
            dumpthis.Add(example3);
            dumpthis.Add(example4);

            string jsonString = JSON.Str(dumpthis);
            File.WriteAllText("example.json", jsonString);
        }
    }
}
