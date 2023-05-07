﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using ImGuiNET;
using Triggered.modules.wrapper;

/// <summary>
/// An abstract group for representing both Group and Element.
/// </summary>
public abstract class AGroupElement
{
    /// <summary>
    /// Inherited classes can export themselves to string.
    /// </summary>
    /// <returns>JSON string</returns>
    public string ToJson()
    {
        return JSON.Str(this);
    }
    /// <summary>
    /// Inherited classes can export themselves to clipboard.
    /// </summary>
    public void ToClipboard()
    {
        ImGui.SetClipboardText(ToJson());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    public void Dump(string fileName)
    {
        File.WriteAllText($"{fileName}.json", ToJson());
    }
}
public class AGroupElementJsonConverter : JsonConverter<AGroupElement>
{
    public override AGroupElement ReadJson(JsonReader reader, Type objectType, AGroupElement existingValue, bool hasExistingValue, JsonSerializer serializer)
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
            throw new JsonSerializationException("Invalid JSON object format: Does not match with any AGroupElement Member.");
        // Prior logic leaves us with a Group object
        Group group = new Group(jsonObject);
        return group;
    }
    public override void WriteJson(JsonWriter writer, AGroupElement value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value, value.GetType());
    }
}