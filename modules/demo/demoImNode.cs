using Emgu.CV;
using ImGuiNET;
using ImNodesNET;
using System.Collections.Generic;
using Triggered.modules.struct_node;

namespace Triggered.modules.demo
{
    internal static class demoImNode
    {
        private static nint imguiContext;
        private static nint imnodesContext;
        private static nint editorContext;
        private static List<(int, int)> links = new List<(int, int)>();
        private static List<Node> Nodes = new();

        static demoImNode()
        {
            imguiContext = ImGui.GetCurrentContext();
            ImNodes.SetImGuiContext(imguiContext);
            imnodesContext = ImNodes.CreateContext();
            ImNodes.SetCurrentContext(imnodesContext);
            editorContext = ImNodes.EditorContextCreate();
            ImNodes.StyleColorsDark();

            Nodes.Add(new NPlayer());
        }
        public static void Render()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.Begin("node editor");

            if (imguiContext != 0 && imnodesContext != 0 && editorContext != 0)
            {
                RightClickPopup();

                ImNodes.BeginNodeEditor();

                DefineNodes();

                RenderLinks();

                ImNodes.EndNodeEditor();

                // Now we are able to check for new connections
                UpdateLinks();
            }
            else
                App.Log("Node editor initialized to 0", 4);

            ImGui.End();
        }
        private static void DefineNodes()
        {
            foreach (var node in Nodes)
            {
                node.DrawTitleAndIO();
            }
        }
        private static void UpdateLinks()
        {
            // Check if a new link was created
            int startAttr = 0, endAttr = 0;
            if (ImNodes.IsLinkCreated(ref startAttr, ref endAttr))
            {
                links.Add((startAttr, endAttr));
            }
        }
        private static void RenderLinks()
        {
            // Render links
            for (int i = 0; i < links.Count; ++i)
            {
                (int startAttr, int endAttr) = links[i];
                ImNodes.Link(i, startAttr, endAttr);
            }
        }
        private static void RightClickPopup()
        {
            // Right-click menu
            if (ImGui.BeginPopupContextItem("add_component_menu"))
            {
                if (ImGui.BeginMenu("Data Source"))
                {
                    if (ImGui.MenuItem("Player"))
                    {
                        // Code to add a player component goes here
                    }
                    if (ImGui.MenuItem("Actuator"))
                    {
                        // Code to add an actuator component goes here
                    }
                    if (ImGui.MenuItem("Property"))
                    {
                        // Code to add a property component goes here
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Evaluator"))
                {
                    if (ImGui.MenuItem("Greater Than"))
                    {
                        // Code to add a greater than component goes here
                    }
                    if (ImGui.MenuItem("Less Than"))
                    {
                        // Code to add a less than component goes here
                    }
                    if (ImGui.MenuItem("Equal"))
                    {
                        // Code to add an equal component goes here
                    }
                    if (ImGui.MenuItem("Not Equal"))
                    {
                        // Code to add a not equal component goes here
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Logic Gate"))
                {
                    if (ImGui.MenuItem("And"))
                    {
                        // Code to add an and gate component goes here
                    }
                    if (ImGui.MenuItem("Or"))
                    {
                        // Code to add an or gate component goes here
                    }
                    if (ImGui.MenuItem("Nor"))
                    {
                        // Code to add a nor gate component goes here
                    }
                    if (ImGui.MenuItem("Not"))
                    {
                        // Code to add a not gate component goes here
                    }
                    if (ImGui.MenuItem("Xor"))
                    {
                        // Code to add an xor gate component goes here
                    }
                    if (ImGui.MenuItem("Xnor"))
                    {
                        // Code to add an xnor gate component goes here
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Action"))
                {
                    if (ImGui.MenuItem("Click"))
                    {
                        // Code to add a click action component goes here
                    }
                    if (ImGui.MenuItem("Behavior"))
                    {
                        // Code to add a behavior action component goes here
                    }
                    if (ImGui.MenuItem("Apply Currency"))
                    {
                        // Code to add an apply currency action component goes here
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }
        }
    }
}
