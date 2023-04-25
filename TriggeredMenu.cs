namespace Triggered
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using ClickableTransparentOverlay;
    using ClickableTransparentOverlay.Win32;
    using ImGuiNET;

    public class TriggeredMenu :  Overlay
    {
        private volatile App app;
        private readonly Thread logicThread;
        public TriggeredMenu() 
        {
            app = new App();
            logicThread = new Thread(() =>
            {

                while (app.IsRunning)
                {
                    LogicUpdate();
                }
            });

            logicThread.Start();
        }
        private void LogicUpdate()
        {
            Thread.Sleep(app.LogicTickDelayInMilliseconds);
        }
        protected override void Render()
        {
            if (Utils.IsKeyPressedAndNotTimeout(VK.F12)) //F12.
            {
                app.MenuDisplay_Main = !app.MenuDisplay_Main;
            }

            if (Utils.IsKeyPressedAndNotTimeout(VK.F11)) //F11.
            {
                app.MenuDisplay_Log = !app.MenuDisplay_Log;
            }

            if (app.MenuDisplay_Main)
            {
                RenderMainMenu();
            }

            if (app.MenuDisplay_Log)
            {
                app.log.AddLog("Hello world [c=red]{0}[/c] [c=green]{1}[/c]", app.Watch.ElapsedMilliseconds, app.Watch.ElapsedTicks);
                app.log.AddLog("[c=blue]-Debug-[/c]: [c=coral]{0}[/c] [c=#00FF00]green[/c] [c=coral]Extra[/c]", "This is the inserted message");
                app.log.Draw("Log Window", true);
            }
            return;
        }
        private void RenderMainMenu()
        {
            bool isCollapsed = !ImGui.Begin(
                "Triggered Options",
                ref app.IsRunning,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
            // Determine if we need to draw the menu
            if (!app.IsRunning || isCollapsed)
            {
                ImGui.End();
                if (!app.IsRunning)
                {
                    Close();
                }

                return;
            }

            // Menu definition area
            ImGui.Text("Try pressing F12 button to show/hide this Menu.");
            ImGui.Text("Try pressing F11 button to show/hide the Log.");

            // Menu definition complete
            ImGui.End();
        }
    }
}
