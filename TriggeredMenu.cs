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
    using NLog;

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
        private LogLevel[] logLevels = { LogLevel.Trace, LogLevel.Debug, LogLevel.Info, LogLevel.Warn, LogLevel.Error, LogLevel.Fatal };
        private int logLevelIndex = 0;
        private void LogicUpdate()
        {
            Thread.Sleep(app.LogicTickDelayInMilliseconds);
            app.Log("test Message", logLevels[logLevelIndex]);
            logLevelIndex = (logLevelIndex + 1) % logLevels.Length;
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
