using System.IO;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ClickableTransparentOverlay.Win32;
using ImGuiNET;
using Triggered.modules.demo;


namespace Triggered.modules.panel
{

    /// <summary>
    /// The main brains of the App Behavior
    /// </summary>
    public class Viewport : Overlay
    {
        /// <summary>
        /// Save any changed options each second.
        /// </summary>
        public Viewport()
        {
            Thread optionThread;
            optionThread = new Thread(() =>
            {
                while (App.IsRunning)
                {
                    App.Options.SaveChanged();
                    Thread.Sleep(1000);
                }
            });
            optionThread.Start();
            LaunchWindows();
        }

        private static void LaunchWindows()
        {
            // if we have set these options to true, open the window when we start the menu
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustBW"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoBlackWhite();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustColor"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoColor();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustIndColor"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoIndColor();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHSVColor"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColor();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHSVColorDual"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColorDual();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustRectangle"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoShapeRectangle();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHSVSubset"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoHSVSubset();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustOCR"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoOCR();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHWND"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoHWND();
                });
            }
        }

        /// <summary>
        /// Enable VSync and configure 
        /// </summary>
        /// <returns></returns>
        protected override Task PostInitialized()
        {
            var options = App.Options.MainMenu;
            int fontSize = options.GetKey<int>("Font.Size");
            var fontRange = options.GetKey<int>("Font.Range");
            string fontName = App.fonts[options.GetKey<int>("Font.Index")];
            string fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", $"{fontName}.ttf");
            ReplaceFont(fontPath, fontSize, (FontGlyphRangeType)fontRange);
            this.VSync = options.GetKey<bool>("VSync");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Render thread is run every frame
        /// We can piggy back on the render thread for simple keybinds
        /// </summary>

        protected override void Render()
        {
            if (Utils.IsKeyPressedAndNotTimeout(VK.F12)) //F12.
            {
                var mainMenu = App.Options.MainMenu;
                var value = mainMenu.GetKey<bool>("Display_Main");
                mainMenu.SetKey("Display_Main", !value);
            }

            if (Utils.IsKeyPressedAndNotTimeout(VK.F11)) //F11.
            {
                var mainMenu = App.Options.MainMenu;
                var value = mainMenu.GetKey<bool>("Display_StashSorter");
                mainMenu.SetKey("Display_StashSorter", !value);
            }
            // We always render this invisible window
            // Having no viewport will break the children docks
            RenderViewPort();

            // Render the children to the viewport
            RenderChildren();
        }

        private static void RenderChildren()
        {
            if (App.Options.MainMenu.GetKey<bool>("Display_Main"))
                MainMenu.Render();
            if (App.Options.MainMenu.GetKey<bool>("Display_StashSorter"))
                StashSorter.Render();
            if (App.Options.MainMenu.GetKey<bool>("Display_Log"))
                App.logimgui.Draw("Log Window");
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustBW"))
                DemoCV.RenderBW();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustColor"))
                DemoCV.RenderColor();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustIndColor"))
                DemoCV.RenderIndColor();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHSVColor"))
                DemoCV.RenderHSVColor();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHSVColorDual"))
                DemoCV.RenderHSVColorDual();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustShape"))
                DemoCV.RenderShapeDetection();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustRectangle"))
                DemoCV.RenderShapeRectangle();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHSVSubset"))
                DemoCV.RenderHSVSubset();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustOCR"))
                DemoCV.RenderOCR();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHWND"))
                DemoCV.RenderHWND();
        }

        private static void RenderViewPort()
        {
            // Load our options for the main menu
            var options = App.Options.MainMenu;
            // Prepare the local variables
            var fullscreen = options.GetKey<bool>("Fullscreen");
            var padding = options.GetKey<bool>("Padding");

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
            if (fullscreen)
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
            if (!padding)
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

            // Important: note that we proceed even if Begin() returns false (aka window is collapsed).
            // This is because we want to keep our DockSpace() active. If a DockSpace() is inactive,
            // all active windows docked into it will lose their parent and become undocked.
            // We cannot preserve the docking relationship between an active window and an inactive docking, otherwise
            // any change of dockspace/settings would lead to windows being stuck in limbo and never being visible.
            ImGui.Begin("DockSpace Demo", ref App.IsRunning, window_flags);

            // Remove the padding configuration - we pushed it, now we pop it:
            if (!padding)
                ImGui.PopStyleVar();

            // Pop the two style rules set in Fullscreen mode - the corner rounding and the border size.
            if (fullscreen)
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
    }
}
