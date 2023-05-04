﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
        public static List<AGroupElement> AGroupElementList(string json)
        {
            // Deserialize the JSON into a list of AGroupElement objects
            var options = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new AGroupElementJsonConverter() }
            };
            List<AGroupElement> IGroupElementList = JsonConvert.DeserializeObject<List<AGroupElement>>(json, options);
            return IGroupElementList;
        }
    }
}