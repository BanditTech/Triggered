using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Triggered.modules.wrapper
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
        public static bool Validate(string json)
        {
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                while (reader.Read())
                {
                    // Do nothing
                }
                // if the final token is None, we have a valid string.
                return reader.TokenType == JsonToken.None;
            }
        }
        public static List<AGroupElement> AGroupElementList(string json)
        {
            // Deserialize the JSON into a list of AGroupElement objects
            var options = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new AGroupElementJsonConverter() }
            };
            List<AGroupElement> AGroupElementList = JsonConvert.DeserializeObject<List<AGroupElement>>(json, options);
            return AGroupElementList;
        }
        public static AGroupElement AGroupElement(string json)
        {
            var options = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new AGroupElementJsonConverter() }
            };
            AGroupElement AGroupElementObj = JsonConvert.DeserializeObject<AGroupElement>(json, options);
            return AGroupElementObj;
        }
    }
}
