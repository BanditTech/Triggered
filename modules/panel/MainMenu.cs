using ImGuiNET;
using Triggered.modules.options;

namespace Triggered.modules.panel
{
    internal static  class MainMenu
    {
        private static Options_Panel Panel => App.Options.Panel;
        private static bool saveProfile = false;
        private static bool loadProfile = false;
        private static System.Numerics.Vector4 color = new(.5f, .8f, .8f, 1f);
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

            ImGui.TextColored(color,"F12 button: toggle GUI");
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
                    var viewport = Panel.GetKey<bool>("Viewport");
                    if (ImGui.MenuItem("Viewport", null, ref viewport))
                        Panel.SetKey("Viewport", viewport);

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
