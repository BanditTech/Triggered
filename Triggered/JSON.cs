using System.Collections.Generic;
using System.Text.Json;

namespace Triggered
{
    public class JSON
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreNullValues = true
        };

        public static string Str(object obj)
        {
            string json = JsonSerializer.Serialize(obj, options);
            return json;
        }

        public static List<object> Obj(string json)
        {
            List<object> obj = JsonSerializer.Deserialize<List<object>>(json, options);
            return obj;
        }
    }
}
