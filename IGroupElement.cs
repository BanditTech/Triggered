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
    public string GroupType { get; set; }
    public string Min { get; set; }
    public List<IGroupElement> ElementList { get; set; }

    public Group(string groupType, string min)
    {
        GroupType = groupType;
        Min = min;
        ElementList = new List<IGroupElement>();
    }
    public Group(string groupType, string min, List<IGroupElement> elementList)
    {
        GroupType = groupType;
        Min = min;
        ElementList = elementList;
    }

    public void AddElement(IGroupElement element)
    {
        ElementList.Add(element);
    }
    public void RemoveElement(IGroupElement element)
    {
        ElementList.Remove(element);
    }
}
public class TopGroup : Group
{
    public string GroupName { get; set; }
    public int StashTab { get; set; }
    public int Strictness { get; set; }

    public TopGroup(string groupName, string groupType, string min = "1", int stashTab = 0, int strictness = -1) : base(groupType, min)
    {
        GroupName = groupName;
        StashTab = stashTab;
        Strictness = strictness;
    }
}
public class Element : IGroupElement
{
    public string Key { get; set; }
    public string Eval { get; set; }
    public string Min { get; set; }
    public int Weight { get; set; }

    public Element(string key, string eval, string min)
    {
        Key = key;
        Eval = eval;
        Min = min;
        Weight = 1;
    }
    public Element(string key, string eval, string min, int weight)
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
        JObject jsonObject = JObject.Load(reader);
        var min = jsonObject.Value<string>("Min");
        if (min == null)
            throw new JsonSerializationException("Invalid JSON object format: missing 'Min' property.");
        var groupType = jsonObject.Value<string>("GroupType");
        if (groupType == null)
            return jsonObject.ToObject<Element>(serializer);
        var groupName = jsonObject.Value<string>("GroupName");
        if (groupName == null)
            return jsonObject.ToObject<Group>(serializer);
        return jsonObject.ToObject<TopGroup>(serializer);
    }

    public override void WriteJson(JsonWriter writer, IGroupElement value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value, value.GetType());
    }
}