namespace Triggered
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using System.Threading;
    using ClickableTransparentOverlay;
    using ClickableTransparentOverlay.Win32;
    using ImGuiNET;
    using NLog;

    public class TriggeredMenu : Overlay
    {
        private Dictionary<string, object> data;
        private readonly Thread logicThread;
        public TriggeredMenu()
        {
            data = I.O.Data;
            data["MenuDisplay_Main"] = true;
            data["MenuDisplay_Log"] = true;
            data["IsRunning"] = true;
            data["LogicTickDelayInMilliseconds"] = 100;
            data["selectedLogLevelIndex"] = 1;
            data["LogWindowMinimumLogLevel"] = LogLevel.Debug;

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
        protected override void Render()
        {
            if (Utils.IsKeyPressedAndNotTimeout(VK.F12)) //F12.
            {
                App.MenuDisplay_Main = !App.MenuDisplay_Main;
            }

            if (Utils.IsKeyPressedAndNotTimeout(VK.F11)) //F11.
            {
                App.MenuDisplay_Log = !App.MenuDisplay_Log;
            }

            if (App.MenuDisplay_Main)
            {
                RenderMainMenu();
            }

            if (App.MenuDisplay_Log)
            {
                App.logimgui.Draw("Log Window", true);
            }
            return;
        }
        // Define the menu to render
        private void RenderMainMenu()
        {
            bool isRunning = (bool)data["IsRunning"];
            bool isCollapsed = !ImGui.Begin(
                "Triggered Options",
                ref App.IsRunning,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
            data["IsRunning"] = isRunning;
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
            ImGui.Text("Try pressing F12 button to show/hide this Menu.");
            ImGui.Text("Try pressing F11 button to show/hide the Log.");
            if (ImGui.Button("Launch AHK Demo"))
            {
                Thread thread = new Thread(() =>
                {
                    AHK ahk = new AHK();
                    ahk.Demo();
                });
                thread.Start();
            }
            ImGui.Checkbox("Show/Hide the Log", ref App.MenuDisplay_Log);
            
            // Menu definition complete
            ImGui.End();
        }
    }
}
