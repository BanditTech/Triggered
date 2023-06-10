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
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Triggered.modules.panel
{

    /// <summary>
    /// App elements are all nested from this Viewport.
    /// </summary>
    public class Viewport : Overlay
    {
        /// <summary>
        /// Gets the viewport options from the application's options.
        /// </summary>
        private static Options_Viewport Opts => App.Options.Viewport;

        /// <summary>
        /// Gets the font options from the application's options.
        /// </summary>
        private static Options_Font Font => App.Options.Font;

        /// <summary>
        /// Gets the panel options from the application's options.
        /// </summary>
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
        }

        /// <summary>
        /// Enable VSync then validate the font
        /// </summary>
        /// <returns></returns>
        protected override Task PostInitialized()
        {
            // VSync
            this.VSync = true;
            // Font
            int fontSize = Font.GetKey<int>("Size");
            int fontRange = Font.GetKey<int>("Range");
            string fontName = Font.GetKey<string>("Name");
            if (!App.fonts.Contains(fontName) )
            {
                int fontSelection = Font.GetKey<int>("Selection");
                if (App.fonts[fontSelection] != null)
                    fontName = App.fonts[fontSelection];
                else
                    fontName = App.fonts.FirstOrDefault();

                if (string.IsNullOrEmpty(fontName))
                {
                    App.Log("Critical error attempting to fix font selection",5);
                    throw new NullReferenceException("Critical error attempting to fix font selection");
                }

                Font.SetKey("Name", fontName);
                Font.SetKey("Selection", Array.IndexOf(App.fonts,fontName));
            }
            string fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", $"{fontName}.ttf");
            ReplaceFont(fontPath, fontSize, (FontGlyphRangeType)fontRange);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Render a viewport for which to render further children menu.
        /// </summary>
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Dynamic code warning")]
        protected override void Render()
        {
            CheckHotkeys();
            Brain.Process();
            RenderViewPort();
            RenderChildren();
        }

        /// <summary>
        /// Checks the status of hotkeys and updates the corresponding panel options accordingly.
        /// </summary>
        private static void CheckHotkeys()
        {
            if (Utils.IsKeyPressedAndNotTimeout(VK.F12))
            {
                var renderChildren = Panel.GetKey<bool>("RenderChildren");
                var renderMainMenu = Panel.GetKey<bool>("MainMenu");
                if (renderChildren && renderMainMenu)
                {
                    Panel.SetKey("RenderChildren", false);
                    Panel.SetKey("MainMenu", false);
                }
                else if (!renderChildren)
                    Panel.SetKey("RenderChildren", true);
                else
                    Panel.SetKey("MainMenu", true);
            }
        }

        /// <summary>
        /// Renders the viewport using the specified options.
        /// </summary>
        private static void RenderViewPort()
        {
            // Prepare the local variables
            var fullscreen = Opts.GetKey<bool>("Fullscreen");
            var padding = Opts.GetKey<bool>("Padding");

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

        /// <summary>
        /// Renders the child elements, including the main menu, stash sorter, log window, and demo CV.
        /// </summary>
        [RequiresDynamicCode("Calls Triggered.modules.options.Options.Render(Boolean)")]
        private static void RenderChildren()
        {
            if (!Panel.GetKey<bool>("RenderChildren"))
                return;
            MainMenu.Render();
            StashSorter.Render();
            App.logimgui.Draw("Log Window");
            DemoCV.Render();
            App.Options.Locations.Render();
            App.Options.Log.Render(true);
            App.Options.Font.Render(true);
        }
    }
}
