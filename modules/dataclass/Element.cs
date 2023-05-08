using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// This class represents the leaves of the filter tree.
/// Elements are the evaluators which return a true or false.
/// </summary>
public class Element : AGroupElement
{
    /// <summary>
    /// Represents an entry of content we evaluate.
    /// </summary>
    public string Key = "";
    /// <summary>
    /// Determine the type of evaluation.
    /// </summary>
    public string Eval = ">=";
    /// <summary>
    /// Discriminate the content of Key against the value of Min.
    /// </summary>
    public string Min = "";
    /// <summary>
    /// Provide a value to accrue for a match.
    /// </summary>
    public float Weight = 1f;
    /// <summary>
    /// Default constructor makes an empty >= element.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="eval"></param>
    /// <param name="min"></param>
    /// <param name="weight"></param>
    [JsonConstructor]
    public Element(string key = "", string eval = ">=", string min = "", float weight = 1f)
    {
        Key = key;
        Eval = eval;
        Min = min;
        Weight = weight;
    }
    /// <summary>
    /// This constructor checks for matching property in the JObject.
    /// If we match a property, we replace the default with it.
    /// </summary>
    /// <param name="jobj"></param>
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
    /// <summary>
    /// We do not want to make a reference loop.
    /// Simple method to deep clone is serialize.
    /// </summary>
    /// <returns>Copy of the Element without reference</returns>
    public Element Clone()
    {
        string jsonString = this.ToJson();
        JObject json = JObject.Parse(jsonString);
        return new Element(json);
    }
}
