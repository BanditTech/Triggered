namespace Triggered.modules.panels
{
    using System.IO;
    using System;
    using System.Numerics;
    using System.Threading;
    using System.Threading.Tasks;
    using ClickableTransparentOverlay;
    using ClickableTransparentOverlay.Win32;
    using Emgu.CV;
    using ImGuiNET;
    using Triggered.modules.demo;
    using Triggered.modules.wrapper;
    using System.Drawing;
    using Emgu.CV.CvEnum;

    /// <summary>
    /// The main brains of the App Behavior
    /// </summary>
    public class MainMenu : Overlay
    {
        /// <summary>
        /// Constructing the menu class also initiates our threads.
        /// </summary>
        public MainMenu()
        {
            Thread logicThread;
            Thread optionThread;
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
        }

        /// <summary>
        /// Logic operates 
        /// </summary>
        private void LogicUpdate()
        {
            Thread.Sleep(App.Options.MainMenu.GetKey<int>("LogicTickDelayInMilliseconds"));
        }

        /// <summary>
        /// Enable VSync
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
            this.VSync = App.Options.MainMenu.GetKey<bool>("VSync");
            return Task.CompletedTask;
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

            // demoImNode.Render();
        }

        private void RenderLogWindow()
        {
            App.logimgui.Draw("Log Window");
        }

        private static string input = App.Options.MainMenu.GetKey<string>("TestText");
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
            var fontIndex = options.GetKey<int>("Font.Index");
            var fontSize = options.GetKey<int>("Font.Size");
            var fontRange = options.GetKey<int>("Font.Range");
            bool _adjusted = false;
            if (ImGui.Combo("Font",ref fontIndex, App.fonts, App.fonts.Length))
            {
                options.SetKey("Font.Index",fontIndex);
                _adjusted = true;
            }
            ImGui.SameLine();
            if (ImGui.InputInt("Size", ref fontSize))
            {
                options.SetKey("Font.Size", fontSize);
                _adjusted = true;
            }
            if (ImGui.Combo("Glyph Range", ref fontRange, App.glyphs, App.glyphs.Length))
            {
                options.SetKey("Font.Range", fontRange);
                _adjusted = true;
            }
            if (_adjusted)
            {
                string fontName = App.fonts[fontIndex];
                string fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", $"{fontName}.ttf");
                ReplaceFont(fontPath, fontSize, (FontGlyphRangeType)fontRange);
            }

            ImGui.Separator();
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
            if (ImGui.Button("Hello World"))
            {
                Task.Run(() =>
                {
                    DemoCV.ShowBlue();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Capture demo"))
            {
                Task.Run(() =>
                {
                    DemoCV.Capture();
                });
            }
            if (ImGui.Button("B/W demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustBW", true);
                Task.Run(() =>
                {
                    DemoCV.DemoBlackWhite();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Color demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustColor", true);
                Task.Run(() =>
                {
                    DemoCV.DemoColor();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Ind RGB demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustIndColor", true);
                Task.Run(() =>
                {
                    DemoCV.DemoIndColor();
                });
            }
            if (ImGui.Button("HSV demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustHSVColor", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColor();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("HSV Dual demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustHSVColorDual", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVColorDual();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("HSV Subset demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustHSVSubset", true);
                Task.Run(() =>
                {
                    DemoCV.DemoHSVSubset();
                });
            }
            if (ImGui.Button("Shape demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustShape", true);
                Task.Run(() =>
                {
                    DemoCV.DemoShapeDetection();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("Rectangle demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustRectangle", true);
                Task.Run(() =>
                {
                    DemoCV.DemoShapeRectangle();
                });
            }
            ImGui.SameLine();
            if (ImGui.Button("OCR demo"))
            {
                App.Options.DemoCV.SetKey("Display_AdjustOCR", true);
                Task.Run(() =>
                {
                    DemoCV.DemoOCR();
                });
            }
            ImGui.Separator();

            if (ImGui.Button("Planar Subdivision Image"))
            {
                Task.Run(() =>
                {
                    Mat image = DrawSubdivision.Draw(900f, 60);
                    CvInvoke.Imshow("Planar Subdivision",image);
                    image.Dispose();
                    CvInvoke.WaitKey();
                });
            }

            if (DropdownBoxUtility.DrawDropdownBox(ref input))
            {
                options.SetKey("TestText", input);
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
                    var vsync = options.GetKey<bool>("VSync");
                    if (ImGui.MenuItem("VSync", null, ref vsync))
                    {
                        this.VSync = vsync;
                        options.SetKey("VSync", vsync);
                    }
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
