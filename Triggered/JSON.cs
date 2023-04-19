using Newtonsoft.Json;

namespace Triggered
{
    public class JSON
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        public string Str(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, settings);
            return json;
        }
        public object Obj(string json)
        {
            object obj = JsonConvert.DeserializeObject(json);
            return obj;
        }
    }
}
