using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using TextCopy;

public abstract class IGroupElement
{
    public string ToJson()
    {
        return Triggered.JSON.Str(this);
    }
    public void CopyToClipboard()
    {
        string groupData = Triggered.JSON.Str(this);
        ClipboardService.SetText(groupData);
    }
    public void Dump(string fileName)
    {
        string groupData = Triggered.JSON.Str(this);
        File.WriteAllText($"{fileName}.json", groupData);
    }
}
public class Group : IGroupElement
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
    public Group(string groupType, int min)
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
        ElementList.Add(element);
    }
    public void RemoveElement(Element element)
    {
        ElementList.Remove(element);
    }
    public void AddGroup(Group group)
    {
        GroupList.Add(group);
    }
    public void RemoveGroup(Group group)
    {
        GroupList.Remove(group);
    }
    public void Remove(IGroupElement item)
    {
        if (item is Group group)
            RemoveGroup(group);
        else if (item is Element element)
            RemoveElement(element);
    }
    public void Add(IGroupElement item)
    {
        if (item is Group group)
            AddGroup(group);
        else if (item is Element element)
            AddElement(element);
    }
    public void Insert(int index, IGroupElement item)
    {
        if (item is Group group)
            GroupList.Insert(index,group);
        else if (item is Element element)
            ElementList.Insert(index, element);
    }
}
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
public class Element : IGroupElement
{
    public string Key;
    public string Eval;
    public string Min;
    public int Weight;
    public Element()
    {
        Key = "Some Key";
        Eval = ">=";
        Min = "Some Value";
        Weight = 1;
    }
    public Element(JObject jobj)
    {
        if (jobj.TryGetValue("Key", out JToken keyToken))
            Key = keyToken.Value<string>();
        if (jobj.TryGetValue("Eval", out JToken evalToken))
        Eval = evalToken.Value<string>();
        if (jobj.TryGetValue("Min", out JToken minToken))
        Min = minToken.Value<string>();
        if (jobj.TryGetValue("Weight", out JToken weightToken))
        Weight = weightToken.Value<int>();
    }
    [JsonConstructor]
    public Element(string key, string eval, string min, int weight = 1)
    {
        Key = key;
        Eval = eval;
        Min = min;
        Weight = weight;
    }
}
public class IGroupElementJsonConverter : JsonConverter<IGroupElement>
{
    public override IGroupElement ReadJson(JsonReader reader, Type objectType, IGroupElement existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Load JObject from stream
        JObject jsonObject = JObject.Load(reader);
        var min = jsonObject["Min"];
        var groupName = jsonObject["GroupName"];
        var groupType = jsonObject["GroupType"];
        // First we validate the objects and ensure they share a common key "Min".
        if (min == null || min.Type == JTokenType.Null)
            throw new JsonSerializationException("Invalid JSON object format: missing or invalid 'Min' property.");
        if (jsonObject["Key"] != null && jsonObject["Eval"] != null && min != null && jsonObject["Weight"] != null)
        {
            // We have an instance of an Element
            Element element = new Element(jsonObject);
            return element;
        }
        else if (groupName != null && groupType != null && min != null && jsonObject["StashTab"] != null && jsonObject["Strictness"] != null && jsonObject["ElementList"] != null)
        {
            // We have an instance of a TopGroup
            TopGroup topGroup = new TopGroup(jsonObject);
            return topGroup;
        }
        else if (groupType == null || min == null || jsonObject["ElementList"] == null)
            throw new JsonSerializationException("Invalid JSON object format: Does not match with any IGroupElement Member.");
        // Prior logic leaves us with a Group object
        Group group = new Group(jsonObject);
        return group;
    }
    public override void WriteJson(JsonWriter writer, IGroupElement value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value, value.GetType());
    }
}