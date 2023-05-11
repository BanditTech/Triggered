namespace Triggered.modules.panels
{
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using ClickableTransparentOverlay;
    using ClickableTransparentOverlay.Win32;
    using ImGuiNET;
    using Triggered.modules.wrapper;

    /// <summary>
    /// The main brains of the App Behavior
    /// </summary>
    public class MainMenu : Overlay
    {
        /// <summary>
        /// Running logic thread for handling decision making.
        /// </summary>
        private readonly Thread logicThread;
        /// <summary>
        /// Running option thread for saving any changed variables.
        /// </summary>
        private readonly Thread optionThread;
        /// <summary>
        /// Constructing the menu class also initiates our threads.
        /// </summary>
        public MainMenu()
        {
            logicThread = new Thread(() =>
            {
                while (App.IsRunning)
                {
                    LogicUpdate();
                }
            });
            logicThread.Start();
            optionThread = new Thread(() =>
            {
                while (App.IsRunning)
                {
                    App.Options.SaveChanged();
                    Thread.Sleep(1000);
                }
            });
            optionThread.Start();

            // if we have set these options to true, open the window when we start the menu
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustBW"))
            {
                Task.Run(() =>
                {
                    demoCV.AdjustBlackWhite();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustColor"))
            {
                Task.Run(() =>
                {
                    demoCV.AdjustColor();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustIndColor"))
            {
                Task.Run(() =>
                {
                    demoCV.AdjustIndColor();
                });
            }
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHSVColor"))
            {
                Task.Run(() =>
                {
                    demoCV.AdjustHSVColor();
                });
            }
        }
        /// <summary>
        /// Logic operates 
        /// </summary>
        private void LogicUpdate()
        {
            Thread.Sleep(App.Options.MainMenu.GetKey<int>("LogicTickDelayInMilliseconds"));
        }

        /// <summary>
        /// RenderBW thread is run every frame
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

            if (App.Options.MainMenu.GetKey<bool>("Display_Main"))
                RenderMainMenu();
            if (App.Options.MainMenu.GetKey<bool>("Display_StashSorter"))
                StashSorter.Render();
            if (App.Options.MainMenu.GetKey<bool>("Display_Log"))
                RenderLogWindow();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustBW"))
                demoCV.RenderBW();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustColor"))
                demoCV.RenderColor();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustIndColor"))
                demoCV.RenderIndColor();
            if (App.Options.DemoCV.GetKey<bool>("Display_AdjustHSVColor"))
                demoCV.RenderHSVColor();

            return;
        }
        private void RenderLogWindow()
        {
            App.logimgui.Draw("Log Window");
        }
        private void RenderMainMenu()
        {
            var options = App.Options.MainMenu;
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
            int delay = options.GetKey<int>("LogicTickDelayInMilliseconds");
            if (ImGui.SliderInt("Logic MS", ref delay, 10, 1000))
                options.SetKey("LogicTickDelayInMilliseconds", delay);
            var displayLog = options.GetKey<bool>("Display_Log");
            if (ImGui.Checkbox("Show/Hide the Log", ref displayLog))
                options.SetKey("Display_Log", displayLog);
            var displayStashSorter = options.GetKey<bool>("Display_StashSorter");
            if (ImGui.Checkbox("Show/Hide the Stash Sorter", ref displayStashSorter))
                options.SetKey("Display_StashSorter", displayStashSorter);
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
            ImGui.SameLine();
            if (ImGui.Button("Open Blue window"))
            {
                Task.Run(() =>
                {
                    demoCV.ShowBlue();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Capture window"))
            {
                Task.Run(() =>
                {
                    demoCV.Capture();
                });
            }
            if (ImGui.Button("Open B/W window"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustBW", true);
                Task.Run(() =>
                {
                    demoCV.AdjustBlackWhite();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Color window"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustColor", true);
                Task.Run(() =>
                {
                    demoCV.AdjustColor();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Ind RGB window"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustIndColor", true);
                Task.Run(() =>
                {
                    demoCV.AdjustIndColor();
                });
            }
            if (ImGui.Button("Open HSV window"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustHSVColor", true);
                Task.Run(() =>
                {
                    demoCV.AdjustHSVColor();
                });
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
                    var fullscreen = options.GetKey<bool>("Fullscreen");
                    if (ImGui.MenuItem("Fullscreen", null, ref fullscreen))
                        options.SetKey("Fullscreen", fullscreen);
                    var padding = options.GetKey<bool>("Padding");
                    if (ImGui.MenuItem("Padding", null, ref padding))
                        options.SetKey("Padding", padding);
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
