using System.IO;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ClickableTransparentOverlay.Win32;
using ImGuiNET;
using Triggered.modules.demo;
using Triggered.modules.options;

namespace Triggered.modules.panel
{

    /// <summary>
    /// The main brains of the App Behavior
    /// </summary>
    public class Viewport : Overlay
    {
        private static Options_MainMenu mmOpts => App.Options.MainMenu;
        private static Options_Panel Panel => App.Options.Panel;
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
            if (Panel.GetKey<bool>("CV.BlackWhite"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoBlackWhite();
                });
            }
            if (Panel.GetKey<bool>("CV.Color"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoColor();
                });
            }
            if (Panel.GetKey<bool>("CV.IndividualColor"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoIndColor();
                });
            }
            if (Panel.GetKey<bool>("CV.HSV"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColor();
                });
            }
            if (Panel.GetKey<bool>("CV.DualHSV"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColorDual();
                });
            }
            if (Panel.GetKey<bool>("CV.Rectangle"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoShapeRectangle();
                });
            }
            if (Panel.GetKey<bool>("CV.SubsetHSV"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoHSVSubset();
                });
            }
            if (Panel.GetKey<bool>("CV.OCR"))
            {
                Task.Run(() =>
                {
                    DemoCV.DemoOCR();
                });
            }
            if (Panel.GetKey<bool>("CV.WindowHandle"))
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
            int fontSize = mmOpts.GetKey<int>("Font.Size");
            var fontRange = mmOpts.GetKey<int>("Font.Range");
            string fontName = App.fonts[mmOpts.GetKey<int>("Font.Index")];
            string fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", $"{fontName}.ttf");
            ReplaceFont(fontPath, fontSize, (FontGlyphRangeType)fontRange);
            this.VSync = mmOpts.GetKey<bool>("VSync");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Render a viewport for which to render further children menu.
        /// </summary>
        protected override void Render()
        {
            CheckHotkeys();
            Brain.Process();
            RenderViewPort();
            RenderChildren();
        }

        private static void CheckHotkeys()
        {
            if (Utils.IsKeyPressedAndNotTimeout(VK.F12)) //F12.
                Panel.SetKey("MainMenu", !Panel.GetKey<bool>("MainMenu"));

            if (Utils.IsKeyPressedAndNotTimeout(VK.F11)) //F11.
                Panel.SetKey("StashSorter", !Panel.GetKey<bool>("StashSorter"));
        }

        private static void RenderViewPort()
        {
            // Prepare the local variables
            var fullscreen = mmOpts.GetKey<bool>("Fullscreen");
            var padding = mmOpts.GetKey<bool>("Padding");

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

        private static void RenderChildren()
        {
            MainMenu.Render();
            StashSorter.Render();
            App.logimgui.Draw("Log Window");
            DemoCV.Render();
        }
    }
}
