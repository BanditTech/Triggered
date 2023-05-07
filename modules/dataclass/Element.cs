using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// This class represents logic units of the filter tree.
/// Elements are the evaluators which return a true or false.
/// </summary>
public class Element : AGroupElement
{
    /// <summary>
    /// Represents an entry of content we evaluate.
    /// </summary>
    public string Key;
    /// <summary>
    /// Determine the type of evaluation.
    /// </summary>
    public string Eval;
    /// <summary>
    /// Discriminate the content of Key against the value of Min.
    /// </summary>
    public string Min;
    /// <summary>
    /// Provide a value to accrue for a match.
    /// </summary>
    public float Weight;
    public Element()
    {
        Key = "Some Key";
        Eval = ">=";
        Min = "Some Value";
        Weight = 1f;
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
            Weight = weightToken.Value<float>();
    }
    [JsonConstructor]
    public Element(string key, string eval, string min, float weight = 1f)
    {
        Key = key;
        Eval = eval;
        Min = min;
        Weight = weight;
    }
    public Element Clone()
    {
        string jsonString = this.ToJson();
        JObject json = JObject.Parse(jsonString);
        return new Element(json);
    }
}
