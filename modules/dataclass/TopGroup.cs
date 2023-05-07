using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class TopGroup : Group
{
    public string GroupName;
    public int StashTab;
    public int Strictness;
    public TopGroup() : base()
    {
        GroupName = "";
        StashTab = 0;
        Strictness = 0;
    }
    public TopGroup(JObject jobj) : base(jobj)
    {
        if (jobj.TryGetValue("GroupName", out JToken nameToken))
            GroupName = nameToken.Value<string>();
        if (jobj.TryGetValue("StashTab", out JToken stashToken))
            StashTab = stashToken.Value<int>();
        if (jobj.TryGetValue("Strictness", out JToken strictnessToken))
            Strictness = strictnessToken.Value<int>();
    }
    public TopGroup(string groupName, string groupType, int min = 1, int stashTab = 0, int strictness = -1) : base(groupType, min)
    {
        GroupName = groupName;
        StashTab = stashTab;
        Strictness = strictness;
    }
    public TopGroup(List<Element> elementList, List<Group> groupList, string groupName, string groupType, int min = 1, int stashTab = 0, int strictness = -1) : base(elementList, groupList, groupType, min)
    {
        GroupName = groupName;
        StashTab = stashTab;
        Strictness = strictness;
    }
}
