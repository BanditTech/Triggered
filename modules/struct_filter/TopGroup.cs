using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Triggered.modules.struct_filter
{

    /// <summary>
    /// This class represents the trunk of the filter tree.
    /// All the branches of this filter will start from this group.
    /// </summary>
    public class TopGroup : Group
    {
        /// <summary>
        /// GroupName is purely an identifier for the user.
        /// </summary>
        public string GroupName = "";
        /// <summary>
        /// Determines the output tab of the item.
        /// An output of a positive value is a matched item, with a stash to navigate to.
        /// </summary>
        public int StashTab = 0;
        /// <summary>
        /// Determines if the filter should be enabled or not.
        /// If the strictness is at or above the minimum its considered for match.
        /// Is only enabled if above 0.
        /// </summary>
        public int Strictness = 0;

        /// <summary>
        /// Default constructor produces an AND group with a blank name.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="groupType"></param>
        /// <param name="min"></param>
        /// <param name="weight"></param>
        /// <param name="stashTab"></param>
        /// <param name="strictness"></param>
        public TopGroup(string groupName = "", string groupType = "AND", int min = 1, float weight = 1f, int stashTab = 0, int strictness = -1) : base(groupType, min, weight)
        {
            GroupName = groupName;
            StashTab = stashTab;
            Strictness = strictness;
        }
        /// <summary>
        /// This constructor checks for matching property in the JObject.
        /// If we match a property, we replace the default with it.
        /// </summary>
        /// <param name="jobj"></param>
        public TopGroup(JObject jobj) : base(jobj)
        {
            if (jobj.TryGetValue("GroupName", out JToken nameToken))
                GroupName = nameToken.Value<string>();
            if (jobj.TryGetValue("StashTab", out JToken stashToken))
                StashTab = stashToken.Value<int>();
            if (jobj.TryGetValue("Strictness", out JToken strictnessToken))
                Strictness = strictnessToken.Value<int>();
        }
    }
}
