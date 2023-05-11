using Emgu.CV.Flann;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using Triggered.modules.wrapper;

namespace Triggered.modules.options
{
    /// <summary>
    /// Instantiate
    /// </summary>
    public abstract class Options
    {
        /// <summary>
        /// We save the values within a JObject for flexibility
        /// </summary>
        public JObject keyList = new JObject();
        /// <summary>
        /// We use the Name key to build the filename
        /// </summary>
        public string Name = "";
        /// <summary>
        /// We track any changes that occur to the options.
        /// Any changes are saved each second.
        /// </summary>
        internal bool _changed = false;

        /// <summary>
        /// Class finalizer. 
        /// </summary>
        ~Options()
        {
            // When constructing JArray without list size, we set to 20
            // We trim the keyList to remove extra JArray Null objects 
            TrimNullValues(keyList);
        }
        /// <summary>
        /// Set the object located at keys to the content of value.
        /// This uses dot notation to navigate the object structure.
        /// </summary>
        /// <param name="keys">string</param>
        /// <param name="value">dynamic</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetKey(string key, object value)
        {
            string[] keyArray = key.Split('.');
            JToken target = keyList;
            for (int i = 0; i < keyArray.Length - 1; i++)
            {
                string currentKey = keyArray[i];
                if (target[currentKey] == null)
                {
                    target[currentKey] = new JObject();
                }
                target = target[currentKey];
            }
            _changed = true;
            target[keyArray.Last()] = JToken.FromObject(value);
        }

        /// <summary>
        /// Navigate the object structure using the dot notation keys string.<br/>
        /// You must state the type of the value which you are retrieving.
        /// </summary>
        /// <typeparam name="T">Pass the Type you want to return</typeparam>
        /// <param name="keys">dot notation string of object path</param>
        /// <returns>dynamic</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public T GetKey<T>(string keys)
        {
            string[] keyArray = keys.Split('.');
            JToken value = keyList;
            foreach (var key in keyArray)
            {
                if (value == null)
                    break;
                if (value.Type == JTokenType.Array) // check if current value is an array
                {
                    if (int.TryParse(key, out int index)) // try to parse current key as an int
                    {
                        value = value[index];
                        continue;
                    }
                    else
                    {
                        // throw an exception if the key is not a valid integer index
                        throw new ArgumentException($"Invalid key '{key}' for array type.");
                    }
                }
                value = value[key];
            }

            if (value == null)
                return default;

            if (typeof(T) == typeof(Vector4) && value is JObject obj)
            {
                var x = obj.GetValue("X").Value<float>();
                var y = obj.GetValue("Y").Value<float>();
                var z = obj.GetValue("Z").Value<float>();
                var w = obj.GetValue("W").Value<float>();
                return (T)(object)new Vector4(x, y, z, w);
            }
            else if (typeof(T) == typeof(Vector3) && value is JObject obj3)
            {
                var x = obj3.GetValue("X").Value<float>();
                var y = obj3.GetValue("Y").Value<float>();
                var z = obj3.GetValue("Z").Value<float>();
                return (T)(object)new Vector3(x, y, z);
            }
            else if (typeof(T) == typeof(Vector3) && value is JObject obj2)
            {
                var x = obj2.GetValue("X").Value<float>();
                var y = obj2.GetValue("Y").Value<float>();
                return (T)(object)new Vector2(x, y);
            }

            if (value.Type == JTokenType.String)
                return (T)Convert.ChangeType(value.Value<string>(), typeof(T));
            else if (value.Type == JTokenType.Integer)
                return (T)Convert.ChangeType(value.Value<int>(), typeof(T));
            else if (value.Type == JTokenType.Float)
                return (T)Convert.ChangeType(value.Value<float>(), typeof(T));
            else if (value.Type == JTokenType.Boolean)
                return (T)Convert.ChangeType(value.Value<bool>(), typeof(T));
            else if (value.Type == JTokenType.Object)
                return (T)(object)value;
            else if (value.Type == JTokenType.Array)
                return value.ToObject<T>();
            else if (value.Type == JTokenType.Null)
                return default;

            throw new InvalidOperationException($"Unsupported JTokenType {value.Type}");
        }
        /// <summary>
        /// Serialize the keyList into a string.
        /// </summary>
        /// <returns>JSON string</returns>
        public string ToJson()
        {
            return JSON.Str(keyList);
        }
        /// <summary>
        /// A bandaid function that allows us to build arrays of unknown length.<br/>
        /// Is run automatically by the class finalizer.
        /// </summary>
        /// <param name="token">This is usually the keyList</param>
        public void TrimNullValues(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (JProperty prop in token)
                    {
                        TrimNullValues(prop.Value);
                    }
                    break;
                case JTokenType.Array:
                    JArray arr = (JArray)token;
                    while (arr.Last?.Type == JTokenType.Null)
                    {
                        arr.Last.Remove();
                    }
                    foreach (JToken child in arr.Children())
                    {
                        TrimNullValues(child);
                    }
                    break;
            }
        }
        /// <summary>
        /// Utilize the merge method available within Newtonsoft JSON package.
        /// This will only replace values which we have present in the origin.
        /// </summary>
        /// <param name="import">JToken</param>
        public void Merge(JToken import)
        {
            var internalTarget = keyList;
            var importTarget = import;
            var mergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
            internalTarget.Merge(importTarget,mergeSettings);
        }
        /// <summary>
        /// Prepare a stripped down file which only contains changes from default
        /// </summary>
        /// <returns>JObject</returns>
        public JObject PrepareSaveObject()
        {
            var defaultOptions = Activator.CreateInstance(GetType());
            var saveObject = new JObject();
            CompareValuesAndAddToSaveFile(keyList, ((Options)defaultOptions).keyList, saveObject);
            //return keyList;
            return saveObject;
        }
        /// <summary>
        /// This is a recursive function which navigates the two object structures.
        /// We preserve the location using dot notation. 
        /// </summary>
        /// <param name="currentObject">JToken</param>
        /// <param name="defaultObject">JToken</param>
        /// <param name="saveObject">JObject</param>
        /// <param name="depth">string</param>
        private void CompareValuesAndAddToSaveFile(JToken currentObject, JToken defaultObject, JToken saveObject)
        {
            if (currentObject.Type == JTokenType.Object)
            {
                foreach (var prop in currentObject.Children<JProperty>())
                {
                    var currentValue = prop.Value;
                    var defaultValue = defaultObject[prop.Name];
                    if (!JToken.DeepEquals(currentValue, defaultValue))
                    {
                        if (currentValue.Type == JTokenType.Array)
                        {
                            ((JObject)saveObject).Add(prop.Name, new JArray());
                            CompareValuesAndAddToSaveFile(currentValue, defaultValue, saveObject[prop.Name]);
                        }
                        else if (currentValue.Type == JTokenType.Object)
                        {
                            ((JObject)saveObject).Add(prop.Name, new JObject());
                            CompareValuesAndAddToSaveFile(currentValue, defaultValue, saveObject[prop.Name]);
                        }
                        else
                        {
                            ((JObject)saveObject).Add(prop.Name, currentValue);
                        }
                    }
                }
            }
            else if (currentObject.Type == JTokenType.Array)
            {
                for (int i = 0; i < currentObject.Count(); i++)
                {
                    var currentValue = currentObject[i];
                    var defaultValue = defaultObject[i];
                    if (!JToken.DeepEquals(currentValue, defaultValue))
                    {
                        // We have a difference in JToken, determine type
                        if (currentValue.Type == JTokenType.Array)
                        {
                            saveObject[i] = new JArray();
                            CompareValuesAndAddToSaveFile(currentValue, defaultValue, saveObject[i]);
                        }
                        else if (currentValue.Type == JTokenType.Object)
                        {
                            saveObject[i] = new JObject();
                            CompareValuesAndAddToSaveFile(currentValue, defaultValue, saveObject[i]);
                        }
                        else
                        {
                            saveObject[i] = currentValue;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Allows the inherited classes to save their options to file.
        /// </summary>
        public void Save()
        {
            _changed = false;
            var saveObj = PrepareSaveObject();
            File.WriteAllText($"save\\{Name}.json", JSON.Str(saveObj));
        }
        /// <summary>
        /// Allows the inherited classes to merge the saved option subset.
        /// </summary>
        public void Load()
        {
            if (File.Exists($"save\\{Name}.json"))
            {
                string json = File.ReadAllText($"save\\{Name}.json");
                var obj = (JToken)JSON.Obj(json);
                Merge(obj);
            }
        }
        /// <summary>
        /// We determine if the options have been changed, in order to save to file.
        /// </summary>
        public void SaveChanged()
        {
            if (_changed)
            {
                App.Log($"We changed something inside {Name}");
                Save();
            }
        }
    }
}
