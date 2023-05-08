using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Triggered.modules.wrapper
{
    /// <summary>
    /// JSON wrapper class to shorten 
    /// </summary>
    public static class JSON
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        /// <summary>
        /// Convert an object into a string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Serialized JSON string</returns>
        public static string Str(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, settings);
            return json;
        }
        /// <summary>
        /// Convert an object into a minified string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Serialized JSON string</returns>
        public static string Min(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return json;
        }
        /// <summary>
        /// Convert from JSON string to a object.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>generic object</returns>
        public static object Obj(string json)
        {
            object obj = JsonConvert.DeserializeObject(json);
            return obj;
        }
        /// <summary>
        /// Validates a JSON string for use in importing.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>True if valid JSON, False otherwise</returns>
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
        /// <summary>
        /// JSON parser for our custom dataclasses.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>List of TopGroup</returns>
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
        /// <summary>
        /// JSON parser for our custom dataclasses.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>Element, TopGroup, or Group</returns>
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
