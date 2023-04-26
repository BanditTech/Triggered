namespace Triggered
{
    using System.Numerics;
    using System.Threading;
    using ClickableTransparentOverlay;
    using ClickableTransparentOverlay.Win32;
    using ImGuiNET;

    public class TriggeredMenu : Overlay
    {
        private readonly Thread logicThread;
        public TriggeredMenu()
        {
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

            if (Utils.IsKeyPressedAndNotTimeout(VK.F10)) //F10.
            {
                App.ShowTransparentViewport = !App.ShowTransparentViewport;
            }

            if (App.ShowTransparentViewport)
            {
                ShowExampleAppDockSpace(ref App.DockSpaceOpen);
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
            bool isCollapsed = !ImGui.Begin(
                "Triggered Options",
                ref App.IsRunning,
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);
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
            ImGui.Text("Try pressing F10 button to show/hide the Transparent Overlay.");
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
        private void ShowExampleAppDockSpace(ref bool p_open)
        {
            ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.None;
            ImGuiWindowFlags window_flags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
            window_flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
            window_flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.WorkPos);
            ImGui.SetNextWindowSize(viewport.WorkSize);
            ImGui.SetNextWindowViewport(viewport.ID);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
            ImGui.SetNextWindowBgAlpha(0.0f);
            ImGui.Begin("DockSpace Demo", ref p_open, window_flags);
            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor();
            var dockspace_id = ImGui.GetID("MyDockSpace");
            ImGui.DockSpace(dockspace_id, new Vector2(0.0f, 0.0f), dockspace_flags);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Options"))
                {
                    ImGui.Separator();
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
        }
    }   
}
