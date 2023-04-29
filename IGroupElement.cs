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
    public int Min { get; set; }
    public List<IGroupElement> ElementList { get; set; }
    public Group()
    {
        GroupType = "AND";
        Min = 1;
        ElementList = new List<IGroupElement>();
    }
    public Group(string groupType, int min)
    {
        GroupType = groupType;
        Min = min;
        ElementList = new List<IGroupElement>();
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
    public TopGroup() : base()
    {
        GroupName = "";
        StashTab = 0;
        Strictness = 0;
    }
    public TopGroup(string groupName, string groupType, int min = 1, int stashTab = 0, int strictness = -1) : base(groupType, min)
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
    public Element()
    {
        Key = "Some Key";
        Eval = ">=";
        Min = "Some Value";
        Weight = 1;
    }
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
        // Validate 'Min' property
        var min = jsonObject["Min"];
        var groupName = jsonObject["GroupName"];
        var groupType = jsonObject["GroupType"];
        // First we validate the objects and ensure they share a common key.
        if (min == null || min.Type == JTokenType.Null)
            throw new JsonSerializationException("Invalid JSON object format: missing or invalid 'Min' property.");
        if (jsonObject["Key"] != null && jsonObject["Eval"] != null && min != null)
        {
            // Manually create an instance of the Element class and populate its properties
            Element element = new Element((string)jsonObject["Key"], (string)jsonObject["Eval"], (string)min, (int)jsonObject["Weight"]);
            return element;
        }
        else if (groupName != null && jsonObject["StashTab"] != null && min != null)
        {
            // Manually create an instance of the TopGroup class and populate its properties
            TopGroup topGroup = new TopGroup((string)groupName, (string)groupType, (int)min, (int)jsonObject["StashTab"], (int)jsonObject["Strictness"]);
            return topGroup;
        }
        else if (groupType == null)
            throw new JsonSerializationException("Invalid JSON object format: Does not match with any IGroupElement Member.");
        // 
        Group group = new Group((string)groupType,(int)min);
        return group;
    }
    public override void WriteJson(JsonWriter writer, IGroupElement value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value, value.GetType());
    }
}