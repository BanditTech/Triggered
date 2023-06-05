using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public JObject keyList = new();

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
        /// Set the object located at keys to the content of value.
        /// This uses dot notation to navigate the object structure.
        /// </summary>
        /// <param name="keys">string</param>
        /// <param name="value">dynamic</param>
        /// <exception cref="ArgumentException"></exception>
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

            _changed = true;

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
            // Begin with key navigation
            foreach (var key in keyArray)
            {
                if (value == null)
                    break;
                if (value.Type == JTokenType.Array) // check if current value is an array
                {
                    if (int.TryParse(key, out int index)) // try to parse current key as an int
                    {
                        // Recurse into each index
                        value = value[index];
                        continue;
                    }
                    else
                    {
                        // throw an exception if the key is not a valid integer index
                        throw new ArgumentException($"Invalid key '{key}' for array type.");
                    }
                }
                // Recurse into each property
                value = value[key];
            }
            // validate the object exists
            if (value == null)
                return default;

            // Prepare outcome based on jtoken type
            var jtokenType = value.Type;
            switch (jtokenType)
            {
                case JTokenType.String:
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                    return value.Value<T>();
                case JTokenType.Object:
                case JTokenType.Array:
                    return value.ToObject<T>();
                case JTokenType.Null:
                    return default;
                default:
                    throw new ArgumentException("Unsupported JTokenType");
            }
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
            Default();
            var internalTarget = keyList;
            var importTarget = import;
            var mergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };
            internalTarget.Merge(importTarget,mergeSettings);
            _changed = true;
        }

        /// <summary>
        /// Set the default values for the options object
        /// </summary>
        internal abstract void Default();

        /// <summary>
        /// Prepare a stripped down file which only contains changes from default
        /// </summary>
        /// <returns>JObject</returns>
        public JObject PrepareSaveObject()
        {
            var defaultOptions = Activator.CreateInstance(GetType());
            var saveObject = new JObject();
            CompareValuesAndAddToSaveFile(keyList, ((Options)defaultOptions).keyList, saveObject);
            // Remember to trim the Array which have been initialized to 20 objects!
            TrimNullValues(saveObject);
            return saveObject;
        }

        /// <summary>
        /// This is a recursive function which navigates the two object structures.
        /// We preserve the location using dot notation. 
        /// </summary>
        /// <param name="currentObject">JToken</param>
        /// <param name="defaultObject">JToken</param>
        /// <param name="saveObject">JObject</param>
        private void CompareValuesAndAddToSaveFile(JToken currentObject, JToken defaultObject, JToken saveObject)
        {
            // This first logic block determines if the currentObject is an JArray or JObject
            if (currentObject.Type == JTokenType.Object)
            {
                // Inside this block we know we are dealing with a JObject (Dict style)
                foreach (var prop in currentObject.Children<JProperty>())
                {

                    var currentValue = prop.Value;
                    var defaultValue = defaultObject[prop.Name];
                    if (defaultValue == null)
                        continue;
                    if (!JToken.DeepEquals(currentValue, defaultValue))
                    {
                        if (currentValue.Type == JTokenType.Array)
                        {
                            ((JObject)saveObject).Add(prop.Name, new JArray(new object[20]));
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
                // Inside this block we know we are dealing with a JArray (Index style)
                for (int i = 0; i < currentObject.Count(); i++)
                {
                    var currentValue = currentObject[i];
                    var defaultValue = defaultObject.ElementAtOrDefault(i);
                    if (defaultValue == null)
                        continue;
                    if (!JToken.DeepEquals(currentValue, defaultValue))
                    {
                        // We have a difference in JToken, determine type
                        if (currentValue.Type == JTokenType.Array)
                        {
                            saveObject[i] = new JArray(new object[20]);
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
                _changed = false;
            }
        }

        /// <summary>
        /// We determine if the options have been changed, in order to save to file.
        /// </summary>
        public void SaveChanged()
        {
            if (_changed)
            {
                App.Log($"Running save on {Name} because of changed settings.",0);
                Save();
            }
        }

        public IEnumerable<object> Iterate()
        {
            var optionsFields = keyList
                .GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(field =>
                    GetFieldsRecursive(field.GetValue(keyList))
                );

            foreach (var field in optionsFields)
            {
                yield return field.GetValue(keyList);
            }
        }

        private IEnumerable<FieldInfo> GetFieldsRecursive(object obj)
        {
            return obj?.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
                .SelectMany(field =>
                    field.FieldType.IsClass && field.FieldType != typeof(string)
                        ? GetFieldsRecursive(field.GetValue(obj))
                        : new[] { field }
                ) ?? Enumerable.Empty<FieldInfo>();
        }
    }
}
