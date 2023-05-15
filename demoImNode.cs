using Emgu.CV;
using ImGuiNET;
using ImNodesNET;
using Triggered.modules.struct_nodes;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
    /// <summary>
    /// Represents an object in our editor space.
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// The ID of the Node
        /// </summary>
        public int Id { get; set; }
        
        // Internal values for handling object list
        internal static int internalIndex = 0;
        internal static List<int> occupiedKeys = new List<int>();
        internal static List<(int, string)> Tags;
        internal static List<(int, int)> nodeList = new();

        /// <summary>
        /// Default constructor produces any missing ID.
        /// </summary>
        protected Node(int nodeId = 0)
        {
            if (nodeId == 0)
                nodeId = GetUniqueId();
            else
                RegisterKey(nodeId);
            Id = nodeId;
            AddRef(Id, Id);
        }

        private static void RegisterKey(int key)
        {
            if (!occupiedKeys.Contains(key))
                occupiedKeys.Add(key);
        }

        public static void AddRef(int componentId, int nodeId)
        {
            if (!occupiedKeys.Contains(componentId))
            {
                nodeList.Add((componentId, nodeId));
                RegisterKey(componentId);
            }
        }

        public static void DelRef(int intId)
        {
            nodeList.RemoveAll(node => node.Item1 == intId);
        }

        public static int GetNodeId(int intId)
        {
            foreach (var (id, node) in nodeList)
                if (id == intId)
                    return node;
            Node.GetTags(intId);
            return 0;
        }

        static bool ContainsReference(int intId)
        {
            foreach (var (id, _) in nodeList)
                if (id == intId)
                    return true;
            return false;
        }

        private static int GetUniqueId()
        {
            // Incriment until unoccupied
            internalIndex += 10;
            while (occupiedKeys.Contains(internalIndex))
                internalIndex += 10;
            // Register the generated key as occupied
            RegisterKey(internalIndex); 
            return internalIndex;
        }

        public static string GetTags(int nodeId)
        {
            foreach (var (intId, tag) in Tags)
                if (intId == nodeId) return tag;
            return "";
        }

        public void SetTags(int nodeId, string tags)
        {
            AddRef(nodeId, Id);
            RegisterKey(nodeId);
            // Check if the node already has a tag, and update it
            for (int i = 0; i < Tags.Count; i++)
            {
                if (Tags[i].Item1 == nodeId)
                {
                    Tags[i] = (nodeId, tags);
                    return;
                }
            }

            // If the node doesn't have a tag, add a new one
            Tags.Add((nodeId, tags));
        }
    }
    class NPlayer : Node
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

        internal new List<(int, string)> Tags = new();
        public NPlayer(int nodeId = 0) : base(nodeId)
        {
            SetTags(Id + 1, "Output,T(string),Location");
        }
        public void Node()
        {
            // Draw
            ImNodes.BeginNode(Id);
            ImNodes.BeginNodeTitleBar();
            ImGui.Indent(50);
            ImGui.Text("Player");
            ImNodes.EndNodeTitleBar();
            ImNodes.BeginOutputAttribute(Id + 1);
            ImGui.Text("(string) Location");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(Id + 2);
            ImGui.Text("(float) Health");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(Id + 3);
            ImGui.Text("(float) Mana");
            ImNodes.EndOutputAttribute();
            ImNodes.BeginOutputAttribute(Id + 4);
            ImGui.Text("(float) Energy Shield");
            ImNodes.EndOutputAttribute();
            ImNodes.EndNode();

        }
    }
}
