using System;
using System.Collections.Generic;
using System.Text.Json;
using TextCopy;

public interface IGroupElement
{
}

public class Element : IGroupElement
{
    public string Key { get; set; }
    public string Eval { get; set; }
    public int Min { get; set; }
    public int Weight { get; set; }
}

public class Group : IGroupElement
{
    public string GroupType { get; set; }
    public int Min { get; set; }
    public string GroupName { get; set; }
    public string StashTab { get; set; }
    public List<IGroupElement> ElementList { get; set; }

    public Group(string groupType, int min = 0, string groupName = null, string stashTab = null)
    {
        GroupName = groupName;
        GroupType = groupType;
        Min = min;
        StashTab = stashTab;
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

    public void CopyToClipboard()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var groupData = JsonSerializer.Serialize(this, options);
        ClipboardService.SetText(groupData);
    }

    public static Group PasteFromClipboard()
    {
        var groupData = ClipboardService.GetText();
        return JsonSerializer.Deserialize<Group>(groupData);
    }

    public void CopyAllElementsToClipboard()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var elementsData = JsonSerializer.Serialize(ElementList, options);
        ClipboardService.SetText(elementsData);
    }

    public void PasteAllElementsFromClipboard()
    {
        var elementsData = ClipboardService.GetText();
        var options = new JsonSerializerOptions { Converters = { new IGroupElementJsonConverter() } };
        var elements = JsonSerializer.Deserialize<List<IGroupElement>>(elementsData, options);
        foreach (var element in elements)
        {
            AddElement(element);
        }
    }
}

public class IGroupElementJsonConverter : System.Text.Json.Serialization.JsonConverter<IGroupElement>
{
    public override IGroupElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Determine the concrete type of the element based on the "GroupType" property
        var elementJson = JsonSerializer.Deserialize<JsonElement>(ref reader);
        var groupType = elementJson.GetProperty("GroupType").GetString();
        return groupType switch
        {
            "Element" => JsonSerializer.Deserialize<Element>(elementJson.GetRawText()),
            "Group" => JsonSerializer.Deserialize<Group>(elementJson.GetRawText()),
            _ => throw new JsonException($"Unknown GroupType: {groupType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, IGroupElement value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
