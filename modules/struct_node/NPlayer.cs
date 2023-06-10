using System.Collections.Generic;

namespace Triggered.modules.struct_node
{
    class NPlayer : Node
    {
        public string TitleBar = "Player Object";
        /// <summary>
        /// The zone of the player
        /// </summary>
        public string Location => App.Player.Location;
        /// <summary>
        /// Health value in a range of 0f to 1f
        /// </summary>
        public float Health => App.Player.Health;
        /// <summary>
        /// Mana value in a range of 0f to 1f
        /// </summary>
        public float Mana => App.Player.Mana;
        /// <summary>
        /// Energy Shield value in a range of 0f to 1f
        /// </summary>
        public float Shield => App.Player.Shield;
        /// <summary>
        /// Rage value in a range of 0f to 1f
        /// </summary>
        public float Rage => App.Player.Rage;
        /// <summary>
        /// Ward value in a range of 0f to 1f
        /// </summary>
        public float Ward => App.Player.Ward;
        internal override List<(int, string)> Tags { get; set; } = new();

        public NPlayer(int nodeId = 0) : base(nodeId)
        {
            SetTags(Id, $"TitleBar,{TitleBar},indent 30");
            SetTags(GetNewNodeId(Id), "Output,T(string),Location");
            SetTags(GetNewNodeId(Id), "Output,T(float),Health");
            SetTags(GetNewNodeId(Id), "Output,T(float),Mana");
            SetTags(GetNewNodeId(Id), "Output,T(float),Shield");
            SetTags(GetNewNodeId(Id), "Output,T(float),Ward");
            SetTags(GetNewNodeId(Id), "Output,T(float),Rage");
        }
    }
}
