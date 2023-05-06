using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using Triggered.modules.wrapper;

namespace Triggered.modules.options
{
    public class Options
    {
        public JObject keyList = new JObject();
        public string Name = "";
        internal bool _changed = false;

        ~Options()
        {
            // When constructing JArray without list size, we set to 20
            // We trim the keyList to remove extra JArray Null objects 
            TrimNullValues(keyList);
        }

        public void SetKey(string keys, object value)
        {
            int index;
            string[] keyArray = keys.Split('.');
            JToken target = keyList;
            for (int i = 0; i < keyArray.Length - 1; i++)
            {
                var key = keyArray[i];
                var next = keyArray[i + 1];
                if (target.Type == JTokenType.Array && int.TryParse(key, out index))
                {
                    if (target[index] == null || target[index].Type == JTokenType.Null)
                    {
                        bool willInt = int.TryParse(next, out int _);
                        target[index] = willInt ? new JArray(new object[20]) : new JObject();
                    }
                    target = target[index];
                    continue;
                }
                else if (target[key] == null)
                {
                    bool willInt = int.TryParse(next, out int _);
                    target[key] = willInt ? new JArray(new object[20]) : new JObject();
                }
                target = target[key];
            }

            // Determine if we are changing the value
            switch (value)
            {
                case string stringValue:
                    if (GetKey<string>(keys) != stringValue)
                        _changed = true;
                    break;
                case int intValue:
                    if (GetKey<int>(keys) != intValue)
                        _changed = true;
                    break;
                case float floatValue:
                    if (GetKey<float>(keys) != floatValue)
                        _changed = true;
                    break;
                case bool boolValue:
                    if (GetKey<bool>(keys) != boolValue)
                        _changed = true;
                    break;
                case Vector4 v4Value:
                    if (GetKey<Vector4>(keys) != v4Value)
                        _changed = true;
                    break;
                default:
                    _changed = true;
                    break;
            }

            // Set the key to the value depending on type
            if (target.Type == JTokenType.Array)
            {
                if (int.TryParse(keyArray.Last(), out index))
                    target[index] = JToken.FromObject(value);
                else
                    throw new ArgumentException("The target is an Array but the index is not valid");
            }
            else
                target[keyArray.Last()] = JToken.FromObject(value);
        }
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
                var x = obj.GetValue("x").Value<float>();
                var y = obj.GetValue("y").Value<float>();
                var z = obj.GetValue("z").Value<float>();
                var w = obj.GetValue("w").Value<float>();
                return (T)(object)new Vector4(x, y, z, w);
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
        public string ToJson()
        {
            return JSON.Str(keyList);
        }
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
        public void Merge(JToken import)
        {
            var internalTarget = keyList;
            var importTarget = import;
            Merge(internalTarget, importTarget);
        }
        private void Merge(JToken internalTarget, JToken importTarget)
        {
            switch (importTarget.Type)
            {
                case JTokenType.Object:
                    foreach (var prop in importTarget.Children<JProperty>())
                    {
                        var internalProp = ((JObject)internalTarget).Property(prop.Name);
                        if (internalProp != null)
                        {
                            Merge(internalProp.Value, prop.Value);
                        }
                    }
                    break;

                case JTokenType.Array:
                    for (int i = 0; i < importTarget.Count(); i++)
                    {
                        Merge(internalTarget[i], importTarget[i]);
                    }
                    break;

                default:
                    // Replace the value in the internal target with the value in the import target
                    internalTarget.Replace(importTarget);
                    break;
            }
        }
        public JToken PrepareSaveObject()
        {
            var defaultOptions = Activator.CreateInstance(GetType());
            var saveObject = new JObject();
            CompareValuesAndAddToSaveFile(keyList, ((Options)defaultOptions).keyList, saveObject);
            return saveObject;
        }
        private void CompareValuesAndAddToSaveFile(JToken currentObject, JToken defaultObject, JObject saveObject, string depth = "")
        {
            if (currentObject.Type == JTokenType.Object)
            {
                foreach (var prop in currentObject.Children<JProperty>())
                {
                    var path = string.IsNullOrEmpty(depth) ? prop.Name : $"{depth}.{prop.Name}";
                    var currentValue = prop.Value;
                    var defaultValue = defaultObject[prop.Name];
                    if (!JToken.DeepEquals(currentValue, defaultValue))
                    {
                        if (currentValue.Type == JTokenType.Object)
                        {
                            saveObject.Add(prop.Name, new JObject());
                            CompareValuesAndAddToSaveFile(currentValue, defaultValue, (JObject)saveObject[prop.Name], path);
                        }
                        else
                        {
                            saveObject.Add(path, currentValue);
                        }
                    }
                }
            }
            else if (currentObject.Type == JTokenType.Array)
            {
                for (int i = 0; i < currentObject.Count(); i++)
                {
                    var path = $"{depth}.{i}";
                    var currentValue = currentObject[i];
                    var defaultValue = defaultObject[i];
                    if (!JToken.DeepEquals(currentValue, defaultValue))
                    {
                        // We have a difference in JToken, determine type
                        if (currentValue.Type == JTokenType.Object)
                        {
                            saveObject.Add(path, new JObject());
                            CompareValuesAndAddToSaveFile(currentValue, defaultValue, (JObject)saveObject[path], path);
                        }
                        else
                        {
                            saveObject.Add(path, currentValue);
                        }
                    }
                }
            }
        }
        public void Save()
        {
            _changed = false;
            var saveObj = PrepareSaveObject();
            File.WriteAllText($"save\\{Name}.json", JSON.Str(saveObj));
        }
        public void Load()
        {
            if (File.Exists($"save\\{Name}.json"))
            {
                string json = File.ReadAllText($"save\\{Name}.json");
                var obj = (JToken)JSON.Obj(json);
                Merge(obj);
            }
        }
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
