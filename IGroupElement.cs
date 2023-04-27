using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TextCopy;

public interface IGroupElement
{
}

public abstract class BaseGroup : IGroupElement
{
    public string GroupType { get; set; }
    public int Min { get; set; }
    public List<IGroupElement> ElementList { get; set; }

    protected BaseGroup(string groupType)
    {
        GroupType = groupType;
        ElementList = new List<IGroupElement>();
    }
    protected BaseGroup(string groupType, List<IGroupElement> elementList)
    {
        GroupType = groupType;
        ElementList = elementList;
    }
    protected BaseGroup(string groupType, int min)
    {
        GroupType = groupType;
        Min = min;
        ElementList = new List<IGroupElement>();
    }
    protected BaseGroup(string groupType, int min, List<IGroupElement> elementList)
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

    [RequiresUnreferencedCode("JSON")]
    [RequiresDynamicCode("JSON")]
    public void CopyToClipboard()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string groupData = JsonSerializer.Serialize(this, options);
        ClipboardService.SetText(groupData);
    }
}
public class Group : BaseGroup
{
    // Constructor that passes all parameters through to the base constructor
    public Group(string groupType) : base(groupType)
    {
    }
    public Group(string groupType, List<IGroupElement> elementList) : base(groupType, elementList)
    {
    }
    public Group(string groupType, int min) : base(groupType, min)
    {
    }
    public Group(string groupType, int min, List<IGroupElement> elementList) : base(groupType, min, elementList)
    {
    }
}
public class TopGroup : Group
{
    public string GroupName { get; set; }
    public int StashTab { get; set; }
    public int Strictness { get; set; }

    public TopGroup() : base("AND", 0, new List<IGroupElement>())
    {
        GroupName = "TopGroup";
        StashTab = 0;
        Strictness = -1;
    }

    public TopGroup(string groupName, int stashTab, int strictness) : base("AND", 0, new List<IGroupElement>())
    {
        GroupName = groupName;
        StashTab = stashTab;
        Strictness = strictness;
    }

    public TopGroup(string groupName, int stashTab, int strictness, int min) : base("AND", min, new List<IGroupElement>())
    {
        GroupName = groupName;
        StashTab = stashTab;
        Strictness = strictness;
    }

    public TopGroup(string groupName, int stashTab, int strictness, string groupType, int min) : base(groupType, min, new List<IGroupElement>())
    {
        GroupName = groupName;
        StashTab = stashTab;
        Strictness = strictness;
    }

    public TopGroup(string groupName, int stashTab, int strictness, string groupType, int min, List<IGroupElement> elementList) : base(groupType, min, elementList)
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
    public int Min { get; set; }
    public int Weight { get; set; }

    public Element(string key, string eval, int min)
    {
        Key = key;
        Eval = eval;
        Min = min;
        Weight = 1;
    }
    public Element(string key, string eval, int min, int weight)
    {
        Key = key;
        Eval = eval;
        Min = min;
        Weight = weight;
    }
}