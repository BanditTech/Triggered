using Emgu.CV;
using ImGuiNET;
using ImNodesNET;
using System;
using System.Collections.Generic;

namespace Triggered
{
    internal static class demoImNode
    {
        private static nint imguiContext;
        private static nint imnodesContext;
        private static nint editorContext;
        private static List<(int, int)> links = new List<(int, int)>();

        static demoImNode()
        {
            imguiContext = ImGui.GetCurrentContext();
            ImNodes.SetImGuiContext(imguiContext);
            imnodesContext = ImNodes.CreateContext();
            ImNodes.SetCurrentContext(imnodesContext);
            editorContext = ImNodes.EditorContextCreate();
            ImNodes.StyleColorsDark();
        }
        public static void Render()
        {
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
                App.Log("Node editor initialized to 0",4);

            ImGui.End();
        }
        private static void DefineNodes()
        {
            // Player node 1
            ImNodes.BeginNode(1);
            ImNodes.BeginNodeTitleBar();
            ImGui.Indent(50);
            ImGui.Text("Player");
            ImNodes.EndNodeTitleBar();
            ImNodes.BeginOutputAttribute(2);
            ImGui.Text("(string) Location");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(3);
            ImGui.Text("(float) Health");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(4);
            ImGui.Text("(float) Mana");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(5);
            ImGui.Text("(float) Energy Shield");
            ImNodes.EndOutputAttribute();
            ImNodes.EndNode();

            // AND node 6
            ImNodes.BeginNode(6);
            ImNodes.BeginNodeTitleBar();
            ImGui.Indent(35);
            ImGui.Text("AND");
            ImNodes.EndNodeTitleBar();
            ImNodes.BeginInputAttribute(7);
            ImGui.Text("Bool 1");
            ImNodes.EndInputAttribute();
            ImGui.SameLine();
            ImNodes.BeginOutputAttribute(9);
            ImGui.Indent(10);
            ImGui.Text("True");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginInputAttribute(8);
            ImGui.Text("Bool 2");
            ImNodes.EndInputAttribute();
            ImGui.SameLine();
            ImNodes.BeginOutputAttribute(10);
            ImGui.Indent(10);
            ImGui.Text("False");
            ImNodes.EndOutputAttribute();
            ImNodes.EndNode();
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
    abstract class Node
    {
        internal int id = App.GetUniqueId();
        internal int[] NodeIn { get; set; } = new int[] { };
        internal int[] NodeOut { get; set; } = new int[] { };

        public int GetId() { return id; }

        public bool IsInNode(int nodeId)
        {
            if (id == nodeId) return true;
            if (Array.IndexOf(NodeIn, nodeId) != -1) return true;
            if (Array.IndexOf(NodeOut, nodeId) != -1) return true;
            return false;
        }
    }
    class nPlayer : Node
    {
        /// <summary>
        /// The zone of the player
        /// </summary>
        public string Location;
        /// <summary>
        /// Health value in a range of 0f to 1f
        /// </summary>
        public float Health;
        /// <summary>
        /// Mana value in a range of 0f to 1f
        /// </summary>
        public float Mana;
        /// <summary>
        /// Energy Shield value in a range of 0f to 1f
        /// </summary>
        public float EnergyShield;
        internal int[] NodeOut { get; set; } = new int[] { App.GetUniqueId(), App.GetUniqueId(), App.GetUniqueId(), App.GetUniqueId() };
        public void Node()
        {
            // Draw
            ImNodes.BeginNode(id);
            ImNodes.BeginNodeTitleBar();
            ImGui.Indent(50);
            ImGui.Text("Player");
            ImNodes.EndNodeTitleBar();
            ImNodes.BeginOutputAttribute(NodeOut[0]);
            ImGui.Text("(string) Location");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(NodeOut[1]);
            ImGui.Text("(float) Health");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(NodeOut[2]);
            ImGui.Text("(float) Mana");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(NodeOut[3]);
            ImGui.Text("(float) Energy Shield");
            ImNodes.EndOutputAttribute();
            ImNodes.EndNode();

        }
    }
}
