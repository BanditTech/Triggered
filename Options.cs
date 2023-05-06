using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Triggered
{
    public class Options
    {
        public JObject keyList = new JObject();
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
                    if (target[index] == null || (target[index]).Type == JTokenType.Null)
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
            Merge(internalTarget,importTarget);
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
            var defaultOptions = new MainMenuOptions();
            var saveObject = new JObject();
            CompareValuesAndAddToSaveFile(keyList, defaultOptions.keyList, saveObject);
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
    }
    public class AppOptions
    {
        public MainMenuOptions MainMenu = new MainMenuOptions();
    }
    public class MainMenuOptions : Options
    {
        public MainMenuOptions()
        {
            SetKey("MenuDisplay_StashSorter", 1);
            SetKey("Change_Me", true);
            SetKey("Not_Me", true);
            TrimNullValues(keyList);
        }
    }
}
