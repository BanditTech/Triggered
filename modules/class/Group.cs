using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class Group : AGroupElement
{
    public string GroupType;
    public int Min;
    public int Weight;
    public List<Element> ElementList;
    public List<Group> GroupList;
    public Group()
    {
        GroupType = "AND";
        Min = 1;
        ElementList = new List<Element>();
        GroupList = new List<Group>();
    }
    public Group(JObject jobj)
    {
        if (jobj.TryGetValue("GroupType", out JToken typeToken))
            GroupType = typeToken.Value<string>();
        if (jobj.TryGetValue("Min", out JToken minToken))
            Min = minToken.Value<int>();
        if (jobj.TryGetValue("ElementList", out JToken elementToken))
            ElementList = elementToken.ToObject<List<Element>>();
        if (jobj.TryGetValue("GroupList", out JToken groupToken))
            GroupList = groupToken.ToObject<List<Group>>();
    }
    public Group(string groupType, int min = 1)
    {
        GroupType = groupType;
        Min = min;
        ElementList = new List<Element>();
        GroupList = new List<Group>();
    }
    public Group(List<Element> elementList, List<Group> groupList, string groupType, int min)
    {
        GroupType = groupType;
        Min = min;
        ElementList = elementList;
        GroupList = groupList;
    }
    public void AddElement(Element element)
    {
        ElementList.Add(element.Clone());
    }
    public void RemoveElement(Element element)
    {
        ElementList.Remove(element);
    }
    public void AddGroup(Group group)
    {
        GroupList.Add(group.Clone());
    }
    public void RemoveGroup(Group group)
    {
        GroupList.Remove(group);
    }
    public void Remove(AGroupElement item)
    {
        if (item is Group group)
            RemoveGroup(group);
        else if (item is Element element)
            RemoveElement(element);
    }
    public void Add(AGroupElement item)
    {
        if (item is Group group)
            AddGroup(group);
        else if (item is Element element)
            AddElement(element);
    }
    public void Insert(int index, AGroupElement item)
    {
        if (item is Group group)
            GroupList.Insert(index,group.Clone());
        else if (item is Element element)
            ElementList.Insert(index, element.Clone());
    }
    public Group Clone()
    {
        string jsonString = this.ToJson();
        JObject json = JObject.Parse(jsonString);
        return new Group(json);
    }
}
