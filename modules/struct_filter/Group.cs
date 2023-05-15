using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

/// <summary>
/// This class represents the branches of the filter tree.
/// Groups are the meta evaluators which return a true or false.
/// </summary>
public class Group : AGroupElement
{
    /// <summary>
    /// GroupType is used to determine the evaluation for the group.
    /// Possible group types are AND, NOT, COUNT, WEIGHT.
    /// </summary>
    public string GroupType = "AND";
    /// <summary>
    /// Min is the required value in regards to WEIGHT and COUNT.
    /// Min value is ignored for AND and NOT groups.
    /// </summary>
    public int Min = 1;
    /// <summary>
    /// Weight represents the value that this matching would provide.
    /// Inside a COUNT group, it adds this value to the sum.
    /// Inside a WEIGHT group, it would multiply this value by the stat value then add to the sum.
    /// AND and NOT groups ignore Weight values entirely.
    /// </summary>
    public float Weight = 1f;
    /// <summary>
    /// This contains the Child Elements.
    /// </summary>
    public List<Element> ElementList = new List<Element>();
    /// <summary>
    /// This contains  the Child Groups.
    /// This allows for object nesting.
    /// </summary>
    public List<Group> GroupList = new List<Group>();
    /// <summary>
    /// Default constructor makes an empty AND group.
    /// </summary>
    [JsonConstructor]
    public Group(string groupType = "AND", int min = 1, float weight = 1f)
    {
        GroupType = groupType;
        Min = min;
        Weight = weight;
    }
    /// <summary>
    /// This constructor checks for matching property in the JObject.
    /// If we match a property, we replace the default with it.
    /// </summary>
    /// <param name="jobj"></param>
    public Group(JObject jobj)
    {
        if (jobj.TryGetValue("GroupType", out JToken typeToken))
            GroupType = typeToken.Value<string>();
        if (jobj.TryGetValue("Min", out JToken minToken))
            Min = minToken.Value<int>();
        if (jobj.TryGetValue("Weight", out JToken weightToken))
            Weight = weightToken.Value<float>();
        if (jobj.TryGetValue("ElementList", out JToken elementToken))
            ElementList = elementToken.ToObject<List<Element>>();
        if (jobj.TryGetValue("GroupList", out JToken groupToken))
            GroupList = groupToken.ToObject<List<Group>>();
    }
    /// <summary>
    /// Add an Element to the list.
    /// </summary>
    /// <param name="element"></param>
    public void AddElement(Element element)
    {
        ElementList.Add(element.Clone());
    }
    /// <summary>
    /// Remove an Element from the list.
    /// </summary>
    /// <param name="element"></param>
    public void RemoveElement(Element element)
    {
        ElementList.Remove(element);
    }
    /// <summary>
    /// Add a Group to the list.
    /// </summary>
    /// <param name="group"></param>
    public void AddGroup(Group group)
    {
        GroupList.Add(group.Clone());
    }
    /// <summary>
    /// Remove a Group from the list.
    /// </summary>
    /// <param name="group"></param>
    public void RemoveGroup(Group group)
    {
        GroupList.Remove(group);
    }
    /// <summary>
    /// This removes either Group or Element.
    /// </summary>
    /// <param name="item"></param>
    public void Remove(AGroupElement item)
    {
        if (item is Group group)
            RemoveGroup(group);
        else if (item is Element element)
            RemoveElement(element);
    }
    /// <summary>
    /// This adds either Group or Element.
    /// </summary>
    /// <param name="item"></param>
    public void Add(AGroupElement item)
    {
        if (item is Group group)
            AddGroup(group);
        else if (item is Element element)
            AddElement(element);
    }
    /// <summary>
    /// Insert the object at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, AGroupElement item)
    {
        if (item is Group group)
            GroupList.Insert(index,group.Clone());
        else if (item is Element element)
            ElementList.Insert(index, element.Clone());
    }
    /// <summary>
    /// We do not want to make a reference loop.
    /// Simple method to deep clone is serialize.
    /// </summary>
    /// <returns>Copy of the Group without reference</returns>
    public Group Clone()
    {
        string jsonString = this.ToJson();
        JObject json = JObject.Parse(jsonString);
        return new Group(json);
    }
}
