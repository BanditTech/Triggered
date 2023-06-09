using ImGuiNET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using Triggered.modules.wrapper;
using static Triggered.modules.wrapper.PointScaler;
using static Triggered.modules.wrapper.ImGuiNet;

namespace Triggered.modules.options
{
    /// <summary>
    /// Instantiate
    /// </summary>
    public abstract class Options
    {
        #region Options properties
        /// <summary>
        /// We save the values within a JObject for flexibility
        /// </summary>
        public JObject keyList = new();

        /// <summary>
        /// Associate each setting key with its Type.
        /// </summary>
        private Dictionary<string, Type> keyTypes = new();

        /// <summary>
        /// Associate each setting key with its Label.
        /// </summary>
        internal Dictionary<string, string> keyLabels = new();

        /// <summary>
        /// We use the Name key to build the filename
        /// </summary>
        public string Name = "";

        /// <summary>
        /// We track any changes that occur to the options.
        /// Any changes are saved each second.
        /// </summary>
        internal bool _changed = false;
        #endregion

        /// <summary>
        /// Set the object located at keys to the content of value.
        /// This uses dot notation to navigate the object structure.
        /// </summary>
        /// <param name="keys">string</param>
        /// <param name="value">dynamic</param>
        /// <param name="label"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetKey(string keys, object value, string label = "")
        {
            if (!keyTypes.ContainsKey(keys))
            {
                keyTypes[keys] = value.GetType();
                keyLabels[keys] = label;
            }
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

        #region Save/Load options
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
        #endregion

        #region Iterate methods
        /// <summary>
        /// Iterate through the entries of this Options object.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(string Keys, Type Type)> IterateKeys()
        {
            foreach (var entry in keyTypes)
                yield return (entry.Key, entry.Value);
        }
        /// <summary>
        /// Iterates over the key-value pairs in the <see cref="keyTypes"/> dictionary
        /// and dynamically determines the type of the generic method "GetKey" at runtime.
        /// </summary>
        /// <returns>
        /// An enumerable collection of tuples.
        /// Each tuple contains the key and the corresponding retrieved value.
        /// </returns>
        [RequiresDynamicCode("Determines the type of the generic method at runtime")]
        public IEnumerable<(string Keys, object Obj)> IterateObjects()
        {
            var getKeyMethod = GetType().GetMethod("GetKey");
            foreach (var (key, type) in keyTypes)
            {
                var value = getKeyMethod
                    .MakeGenericMethod(type)
                    .Invoke(this, new object[] { key });
                yield return (key, value);
            }
        }
        #endregion

        #region Render
        private static string currentSection;
        private static string _selected;

        [RequiresDynamicCode("Calls Triggered.modules.options.Options.IterateObjects()")]
        internal void Render()
        {
            if (!App.Options.Panel.GetKey<bool>(Name))
                return;
            ImGui.Begin(Name);
            foreach (var (key, obj) in IterateObjects())
            {
                bool treeOpen = false;
                var keySplit = key.Split('.');
                var label = keyLabels[key];
                var displayedKey = string.IsNullOrEmpty(label) ? string.Join(" ", keySplit) : label;
                if (keySplit.Length > 1 && keySplit[0] != currentSection)
                {
                    NewSection(1);
                    currentSection = keySplit[0];
                    CenteredText(currentSection, .3f, 100);
                    NewSection(1);
                }
                else if (currentSection != null && keySplit.Length <= 1)
                    currentSection = null;
                // Produce a treeNode of the option label
                ImGui.PushID($"{key} Treenode");
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.6f, 1f, .5f, 1f));
                treeOpen = ImGui.TreeNode(displayedKey);
                ImGui.PopStyleColor();
                ImGui.PopID();
                // We can continue if we will not render the contained information
                if (!treeOpen)
                {
                    Spacers(2);
                    continue;
                }

                // We have an open treeNode, so we render all editable fields
                // Depending on the object type, we can produce a GUI to edit it

                // Value type objects
                if (obj is string str)
                {
                    var availableSpace = ImGui.GetContentRegionAvail().X;
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(availableSpace);
                    ImGui.PushID($"{key} InputText");
                    if (ImGui.InputText($"##{key} InputText",ref str, 256))
                        SetKey(key,str);
                    ImGui.PopID();
                }
                else if (obj is float floatValue)
                {
                    var availableSpace = ImGui.GetContentRegionAvail().X;
                    ImGui.SetNextItemWidth(availableSpace);
                    ImGui.PushID($"{key} InputFloat");
                    if (ImGui.InputFloat($"##{key} InputFloat", ref floatValue))
                        SetKey(key, floatValue);
                    ImGui.PopID();
                }
                // Locations objects
                else if (obj is ScaledRectangle scaledRectangle)
                {
                    ImGui.PushID($"{key} Button");
                    if (ImGui.Button("Select Rectangle"))
                        _selected = key;
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.Text("Anchor:");
                    ImGui.SameLine();
                    var anchorIndex = Array.IndexOf(App.anchorValues, scaledRectangle.Start.Anchor);
                    ImGui.PushID($"{key} Combo");
                    ImGui.SetNextItemWidth(130);
                    if (ImGui.Combo("##Anchor{key}", ref anchorIndex, App.anchorNames, App.anchorNames.Length))
                    {
                        string anchorPositionName = Enum.GetName(App.anchorPosType, anchorIndex);
                        AnchorPosition newAnchorPosition = (AnchorPosition)Enum.Parse(App.anchorPosType, anchorPositionName);
                        scaledRectangle.Start = new(scaledRectangle.Start.Point, scaledRectangle.Start.Height, newAnchorPosition);
                        scaledRectangle.End = new(scaledRectangle.End.Point, scaledRectangle.End.Height, newAnchorPosition);
                        SetKey(key, scaledRectangle);
                    }
                    ImGui.PopID();
                    ImGui.TextColored(new Vector4(.6f, 1f, .8f, 1f),
                        $"Area: ({scaledRectangle.Start.Point.X}, {scaledRectangle.Start.Point.Y})" +
                        $" to ({scaledRectangle.End.Point.X}, {scaledRectangle.End.Point.Y})");
                    ImGui.TextColored(new Vector4(.5f, .5f, 1f, 1f),
                        $"Size: H{scaledRectangle.Height} W{scaledRectangle.Width}");
                    SameLineSpacers(3);
                    ImGui.TextColored(new Vector4(1f, .5f, 1f, 1f),
                        $"ScaleH: {scaledRectangle.Start.Height}");

                    if (_selected == key && Selector.ScaledRectangle(ref scaledRectangle, scaledRectangle.Start.Anchor))
                    {
                        SetKey(key, scaledRectangle);
                        App.Log($"New \"{keySplit.Last()}\" scaled rectangle taken\n{JSON.Min(scaledRectangle)}");
                        _selected = null;
                    }
                }
                else if (obj is Coordinate coordinate)
                {
                    ImGui.PushID($"{key} Button");
                    if (ImGui.Button("Select Point"))
                        _selected = key;
                    ImGui.PopID();
                    ImGui.SameLine();
                    ImGui.Text("Anchor:");
                    ImGui.SameLine();
                    ImGui.PushID($"{key} Combo");
                    ImGui.SetNextItemWidth(130);
                    var anchorIndex = Array.IndexOf(App.anchorValues, coordinate.Anchor);
                    if (ImGui.Combo("##Anchor{key}", ref anchorIndex, App.anchorNames, App.anchorNames.Length))
                    {
                        string anchorPositionName = Enum.GetName(App.anchorPosType, anchorIndex);
                        AnchorPosition newAnchorPosition = (AnchorPosition)Enum.Parse(App.anchorPosType, anchorPositionName);
                        coordinate = new(coordinate.Point, coordinate.Height, newAnchorPosition);
                        SetKey(key, coordinate);
                    }
                    ImGui.PopID();
                    ImGui.TextColored(new Vector4(.6f, 1f, .8f, 1f),
                        $"Point: ({coordinate.Point.X}, {coordinate.Point.Y})");
                    SameLineSpacers(3);
                    ImGui.TextColored(new Vector4(1f, .5f, 1f, 1f),
                        $"ScaleH: {coordinate.Height}");

                    if (_selected == key && Selector.Coordinate(ref coordinate, coordinate.Anchor))
                    {
                        SetKey(key, coordinate);
                        App.Log($"New \"{keySplit.Last()}\" coordinate taken\n{JSON.Min(coordinate)}");
                        _selected = null;
                    }
                }
                else if (obj is Measurement measurement)
                {
                    ImGui.PushID($"{key} Button");
                    if (ImGui.Button("Select Line"))
                        _selected = key;
                    ImGui.PopID();

                    ImGui.TextColored(new Vector4(.6f, 1f, .8f, 1f),
                        $"Value: ({measurement.Value})");
                    SameLineSpacers(3);
                    ImGui.TextColored(new Vector4(1f, .5f, 1f, 1f),
                        $"ScaleH: {measurement.Height}");

                    if (_selected == key && Selector.Measurement(ref measurement))
                    {
                        SetKey(key, measurement);
                        App.Log($"New \"{keySplit.Last()}\" measurement taken\n{JSON.Min(measurement)}");
                        _selected = null;
                    }
                }

                // Close the option's treeNode.
                ImGui.TreePop();
                Spacers(2);
            }
            ImGui.End();
        }
        #endregion
    }
}
