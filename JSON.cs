using Newtonsoft.Json;

namespace Triggered
{
    public static class JSON
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        public static string Str(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, settings);
            return json;
        }
        public static string Min(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return json;
        }
        public static object Obj(string json)
        {
            object obj = JsonConvert.DeserializeObject(json);
            return obj;
        }
    }
}