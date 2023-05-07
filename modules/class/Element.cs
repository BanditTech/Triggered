using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Element : AGroupElement
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
    public Element Clone()
    {
        string jsonString = this.ToJson();
        JObject json = JObject.Parse(jsonString);
        return new Element(json);
    }
}
