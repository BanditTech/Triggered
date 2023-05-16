using ImGuiNET;
using ImNodesNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Triggered.modules.struct_node
{
    /// <summary>
    /// Generic shape of all Nodes which will occupy the editor.
    /// Node abstract superclass handles the internal Id scheme.
    /// Uses the tag list to draw the object, provides methods for adding 
    /// </summary>
    public abstract class Node
    {
        /// <summary>
        /// The ID of the Node
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tags will be filled in by our inherited class.
        /// </summary>
        internal abstract List<(int, string)> Tags { get; set; }

        /// <summary>
        /// Internal values for handling object list
        /// </summary>
        protected private static int internalIndex = 0;

        /// <summary>
        /// A list of tuples containing node pairings.
        /// </summary>
        protected private static List<(int, int)> nodeList = new();

        /// <summary>
        /// Default constructor produces any missing ID.
        /// </summary>
        protected Node(int nodeId = 0)
        {
            if (nodeId == 0)
                nodeId = GetNewNodeId();
            else if (nodeList.Any(node => node.Item1 == nodeId))
                nodeId = GetNewNodeId();
            else
                NodeRef(nodeId);
            Id = nodeId;
        }

        internal static int GetNewNodeId(int parent = 0)
        {
            //Incriment our Index
            ++internalIndex;
            // Check if unoccupied, then continue to incriment if so
            while (nodeList.Any(tuple => tuple.Item1 == internalIndex))
                ++internalIndex;
            // Add its reference, either to a parent node or itself 
            NodeRef(internalIndex, parent);
            // Return the newly minted ID
            return internalIndex;
        }

        private static void NodeRef(int componentId, int nodeId = 0)
        {
            // Determine if we have a parent node
            bool hasParent = nodeId != 0;
            var asignment = hasParent ? nodeId : componentId;
            if (nodeList.Any(node => node.Item1 == componentId && node.Item2 == asignment))
                return;
            // Add our component to the nodeList
            nodeList.Add((componentId, asignment));
        }

        internal static void RemoveNode(int nodeId)
        {
            nodeList.RemoveAll(node => node.Item1 == nodeId);
            nodeList.RemoveAll(node => node.Item2 == nodeId);
        }

        internal string GetTags(int nodeId)
        {
            foreach (var (intId, tag) in Tags)
                if (intId == nodeId) return tag;
            return "";
        }

        /// <summary>
        /// Set the Tag for a nodeId.
        /// A Tag determines valid links.
        /// This also determines how to draw the object.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="tags"></param>
        internal void SetTags(int nodeId, string tags)
        {
            NodeRef(nodeId, Id);
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

        /// <summary>
        /// Determine if the list of tags contains option flags
        /// </summary>
        /// <param name="parts"></param>
        internal void HandleFlags(string[] parts)
        {
            if (parts.Any(flag => flag.ToLower().StartsWith("spacing")))
                ImGui.Spacing();
            if (parts.Any(flag => flag.ToLower().StartsWith("sameline")))
                ImGui.SameLine();
            if (parts.Any(flag => flag.ToLower().StartsWith("indent ")))
            {
                int value = parts.Where(flag => flag.ToLower().StartsWith("indent "))
                                        .Select(flag => int.Parse(flag[7..]))
                                        .FirstOrDefault(0);
                ImGui.Indent(value);
            }
        }
        /// <summary>
        /// Draw the Title bar, and Attribute (Input, Output) fields.
        /// </summary>
        public void DrawTitleAndIO()
        {
            // First we begin our node with its assigned Id
            ImNodes.BeginNode(Id);
            // Use the List<(int, string)> Tags to construct
            foreach (var (nodeId, tag) in Tags)
            {
                string[] parts = tag.Split(',', StringSplitOptions.TrimEntries);
                switch (parts[0].ToLower())
                {
                    case "titlebar":
                        string title = parts[1];
                        ImNodes.BeginNodeTitleBar();
                        HandleFlags(parts);
                        ImGui.Text(title);
                        ImNodes.EndNodeTitleBar();
                        break;
                    case "output":
                        string outType = parts[1];
                        string outName = parts[2];
                        ImNodes.BeginOutputAttribute(nodeId);
                        HandleFlags(parts);
                        ImGui.Text($"{outType} {outName}");
                        ImNodes.EndOutputAttribute();
                        break;
                    case "input":
                        string inType = parts[1];
                        string inName = parts[2];
                        ImNodes.BeginInputAttribute(nodeId);
                        HandleFlags(parts);
                        ImGui.Text($"{inType} {inName}");
                        ImNodes.EndInputAttribute();
                        break;
                    case "dropdown":
                        break;
                    default:
                        break;
                }
            }
            // We are finished constructing the node object
            ImNodes.EndNode();
        }

    }
}
