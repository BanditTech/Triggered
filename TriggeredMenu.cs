namespace Triggered
{
    using System.Collections.Generic;
    using System.Diagnostics;
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
                while ((bool)data["IsRunning"])
                {
                    LogicUpdate();
                }
            });
            logicThread.Start();
        }
        private void LogicUpdate()
        {
            Thread.Sleep((int)data["LogicTickDelayInMilliseconds"]);
            App.Log("test Message", ExampleAppLog.logLevels[ExampleAppLog.logLevelIndex]);
            ExampleAppLog.logLevelIndex = (ExampleAppLog.logLevelIndex + 1) % ExampleAppLog.logLevels.Length;
        }
        protected override void Render()
        {
            if (Utils.IsKeyPressedAndNotTimeout(VK.F12)) //F12.
            {
                data["MenuDisplay_Main"] = !(bool)data["MenuDisplay_Main"];
            }

            if (Utils.IsKeyPressedAndNotTimeout(VK.F11)) //F11.
            {
                data["MenuDisplay_Log"] = !(bool)data["MenuDisplay_Log"];
            }

            if ((bool)data["MenuDisplay_Main"])
            {
                RenderMainMenu();
            }

            if ((bool)data["MenuDisplay_Log"])
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
                ref isRunning,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
            data["IsRunning"] = isRunning;
            // Determine if we need to draw the menu
            if (!(bool)data["IsRunning"] || isCollapsed)
            {
                ImGui.End();
                if (!(bool)data["IsRunning"])
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
            bool menuDisplay_Log = (bool)data["MenuDisplay_Log"];
            ImGui.Checkbox("Show/Hide the Log", ref menuDisplay_Log);
            data["MenuDisplay_Log"] = menuDisplay_Log;
            
            // Menu definition complete
            ImGui.End();
        }
    }
}
