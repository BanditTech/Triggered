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
using Newtonsoft.Json;
using System.Reflection;

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
        private Dictionary<string, JObject> internals = new();

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

        #region SetKey Overloads
        /// <summary>
        /// Set the object located at keys to the content of value.
        /// This uses dot notation to navigate the object structure.
        /// </summary>
        /// <param name="keys">string</param>
        /// <param name="value">dynamic</param>
        /// <param name="label"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetKey(string keys, object value)
        {
            if (!keyTypes.ContainsKey(keys))
                keyTypes[keys] = value.GetType();

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

            var internalOptions = internals[keys];
            bool hasCallback = internalOptions?["callback"]?.ToObject<bool>() ?? false;

            if (hasCallback)
            {
                string methodName = internalOptions["method"]?.ToObject<string>();
                string assemblyFullName = internalOptions["assembly"]?.ToObject<string>();
                string targetTypeFullName = internalOptions["targetType"]?.ToObject<string>();
                JArray parameterTypeNames = internalOptions["parameterTypes"] as JArray;

                if (!string.IsNullOrEmpty(methodName) && !string.IsNullOrEmpty(assemblyFullName) &&
                    !string.IsNullOrEmpty(targetTypeFullName) && parameterTypeNames != null)
                {
                    Assembly assembly = Assembly.Load(assemblyFullName);
                    Type targetType = assembly.GetType(targetTypeFullName);

                    if (targetType != null)
                    {
                        MethodInfo methodInfo = targetType.GetMethod(methodName, parameterTypeNames.Select(p => Type.GetType(p.ToString())).ToArray());

                        if (methodInfo != null)
                            methodInfo.Invoke(null, new object[] { value });
                    }
                }
            }
        }

        /// <summary>
        /// Override to allow setting a label
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="value"></param>
        /// <param name="label"></param>
        public void SetKey(string keys, object value, string label, Delegate callback = null)
        {
            if (!internals.ContainsKey(keys))
            {
                var jObject = new JObject();
                jObject["label"] = label;

                SetCallback(ref jObject, callback);
                internals[keys] = jObject;
            }
            SetKey(keys,value);
        }
        public void SetKey(string keys, object value, bool hidden)
        {
            if (!internals.ContainsKey(keys))
            {
                var jObject = new JObject();
                jObject["hidden"] = hidden;
                internals[keys] = jObject;
            }
            SetKey(keys,value);
        }

        public void SetKey(string keys, object value, string label, float min, float max, Delegate callback = null)
        {
            if (!internals.ContainsKey(keys))
            {
                var jObject = new JObject();
                jObject["label"] = label;
                jObject["slider"] = true;
                jObject["minFloat"] = min;
                jObject["maxFloat"] = max;

                SetCallback(ref jObject, callback);
                internals[keys] = jObject;
            }
            SetKey(keys,value);
        }
        public void SetKey(string keys, object value, string label, int min, int max, Delegate callback = null)
        {
            if (!internals.ContainsKey(keys))
            {
                var jObject = new JObject();
                jObject["label"] = label;
                jObject["slider"] = true;
                jObject["minInt"] = min;
                jObject["maxInt"] = max;

                SetCallback(ref jObject, callback);
                internals[keys] = jObject;
            }
            SetKey(keys,value);
        }

        public void SetKey(string keys, object value, string label, string[] items, Delegate callback = null)
        {
            if (!internals.ContainsKey(keys))
            {
                var jObject = new JObject();
                jObject["label"] = label;
                jObject["combo"] = true;
                jObject["items"] = new JArray(items);
                jObject["count"] = items.Length;

                SetCallback(ref jObject,callback);
                internals[keys] = jObject;
            }
            SetKey(keys,value);
        }

        /// <summary>
        /// Set the entire internals object at once
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="value"></param>
        /// <param name="internals"></param>
        public void SetKey(string keys, object value, JObject internals)
        {
            if (!this.internals.ContainsKey(keys))
                this.internals[keys] = internals;
            SetKey(keys,value);
        }

        public void SetCallback(ref JObject jObject, Delegate callback = null)
        {
            if (callback == null)
                return;
            MethodInfo methodInfo = callback.Method;
            Type declaringType = methodInfo.DeclaringType;
            jObject["callback"] = true;
            jObject["method"] = methodInfo.Name;
            jObject["assembly"] = declaringType.Assembly.FullName;
            jObject["targetType"] = declaringType.FullName;
            jObject["parameterTypes"] = new JArray(methodInfo.GetParameters().Select(p => p.ParameterType.FullName));
        }
        #endregion

        #region GetKey and GetInternals
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

        public JObject GetInternals(string keys)
        {
            return internals[keys];
        }
        #endregion

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
        private string currentSection;
        private string _selected;
        private bool panelOpen = true;
        private Dictionary<string, bool> expanded;

        [RequiresDynamicCode("Calls Triggered.modules.options.Options.IterateObjects()")]
        internal void Render(bool expand = false)
        {
            if (expanded == null)
                InitializeExpanded();
            bool panelOption = App.Options.Panel.GetKey<bool>(Name);
            if (!panelOption)
                return;
            if (!panelOpen)
            {
                panelOpen = true;
                App.Options.Panel.SetKey(Name, false);
                return;
            }
            ImGui.PushID($"{Name} option panel");
            ImGui.Begin(Name, ref panelOpen);
            ImGui.PopID();

            foreach (var (key, obj) in IterateObjects())
            {
                // Split the key string into its sections
                var keySplit = key.Split('.');
                // Retreive the internal settings for this option
                JObject localInternals = internals[key];
                var label = localInternals["label"] != null ? localInternals["label"].Value<string>() : "";
                // If we do not have a provided Label we produce one
                var displayedKey = string.IsNullOrEmpty(label) ? string.Join(" ", keySplit) : label;
                // Determine if we should produce a section header
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
                if (!expanded.ContainsKey(key))
                    expanded[key] = expand;
                ImGui.SetNextItemOpen(expanded[key]);
                bool treeOpen = ImGui.TreeNode(displayedKey);
                if (treeOpen != expanded[key])
                {
                    expanded[key] = treeOpen;
                    string filePath = Path.Combine(AppContext.BaseDirectory, "expand", $"{Name}.json");
                    File.WriteAllText(filePath, JSON.Min(expanded));
                }
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
                // Use internal config to pick the type of GUI element to make

                // Value type objects
                if (obj is string stringValue)
                {
                    var availableSpace = ImGui.GetContentRegionAvail().X;
                    ImGui.SetNextItemWidth(availableSpace);
                    ImGui.PushID($"{key} InputText");
                    if (ImGui.InputText($"##{key} InputText",ref stringValue, 256))
                        SetKey(key,stringValue);
                    ImGui.PopID();
                }
                else if (obj is float floatValue)
                {
                    bool isSlider = localInternals["slider"] != null && localInternals["slider"].Value<bool>();
                    var availableSpace = ImGui.GetContentRegionAvail().X;
                    ImGui.SetNextItemWidth(availableSpace);
                    ImGui.PushID($"{key} InputFloat");
                    if (isSlider)
                    {
                        float minFloat = localInternals["minFloat"] != null ? localInternals["minFloat"].Value<float>() : 0f;
                        float maxFloat = localInternals["maxFloat"] != null ? localInternals["maxFloat"].Value<float>() : 1f;
                        if (ImGui.SliderFloat($"##{key} InputFloat", ref floatValue, minFloat, maxFloat))
                            SetKey(key, floatValue);
                    }
                    else if (ImGui.InputFloat($"##{key} InputFloat", ref floatValue))
                            SetKey(key, floatValue);
                    ImGui.PopID();
                }
                else if (obj is int intValue)
                {
                    bool isSlider = localInternals["slider"] != null && localInternals["slider"].Value<bool>();
                    bool isCombo = localInternals["combo"] != null && localInternals["combo"].Value<bool>();
                    var availableSpace = ImGui.GetContentRegionAvail().X;
                    ImGui.SetNextItemWidth(availableSpace);
                    ImGui.PushID($"{key} InputInt");
                    if (isSlider)
                    {
                        int minInt = localInternals["minInt"] != null ? localInternals["minInt"].Value<int>() : 0;
                        int maxInt = localInternals["maxInt"] != null ? localInternals["maxInt"].Value<int>() : 100;
                        if (ImGui.SliderInt($"##{key} SliderInt", ref intValue, minInt, maxInt))
                            SetKey(key, intValue);
                    }
                    else if (isCombo)
                    {
                        string[] items = localInternals["items"] != null ? localInternals["items"].ToObject<string[]>().ToArray() : default;
                        int count = localInternals["count"] != null ? localInternals["count"].Value<int>() : default;
                        if (ImGui.Combo($"##{key} ComboIndex", ref intValue, items, count))
                            SetKey(key, intValue);
                    }
                    else if (ImGui.InputInt($"##{key} InputInt", ref intValue))
                            SetKey(key, intValue);
                    ImGui.PopID();
                }
                else if (obj is double doubleValue)
                {
                    var availableSpace = ImGui.GetContentRegionAvail().X;
                    ImGui.SetNextItemWidth(availableSpace);
                    ImGui.PushID($"{key} InputDouble");
                    if (ImGui.InputDouble($"##{key} InputDouble", ref doubleValue))
                        SetKey(key, doubleValue);
                    ImGui.PopID();
                }
                else if (obj is bool boolValue)
                {
                    var availableSpace = ImGui.GetContentRegionAvail().X;
                    ImGui.SetNextItemWidth(availableSpace);
                    ImGui.PushID($"{key} Checkbox");
                    if (ImGui.Checkbox($"##{key} Checkbox", ref boolValue))
                        SetKey(key, boolValue);
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
                // Color objects
                else if (obj is Vector3 vector3)
                {
                    bool isEdit = localInternals["edit"] != null && localInternals["edit"].Value<bool>();
                    bool isHSV = localInternals["hsv"] != null && localInternals["hsv"].Value<bool>();

                    ImGuiColorEditFlags flags = ImGuiColorEditFlags.None;
                    if (isHSV)
                    {
                        flags |= ImGuiColorEditFlags.DisplayHSV;
                        flags |= ImGuiColorEditFlags.InputHSV;
                    }

                    if (isEdit)
                    {
                        ImGui.PushID($"{key} ColorEdit");
                        if (ImGui.ColorEdit3($"##{key} ColorEdit", ref vector3,flags))
                            SetKey(key,vector3);
                        ImGui.PopID();
                    }
                    else
                    {
                        ImGui.PushID($"{key} ColorPicker");
                        if (ImGui.ColorPicker3($"##{key} ColorPicker", ref vector3,flags))
                            SetKey(key,vector3);
                        ImGui.PopID();
                    }
                }
                else if (obj is Vector4 vector4)
                {
                    bool isEdit = localInternals["edit"] != null && localInternals["edit"].Value<bool>();
                    bool isHSV = localInternals["hsv"] != null && localInternals["hsv"].Value<bool>();

                    ImGuiColorEditFlags flags = ImGuiColorEditFlags.None;
                    if (isHSV)
                    {
                        flags |= ImGuiColorEditFlags.DisplayHSV;
                        flags |= ImGuiColorEditFlags.InputHSV;
                    }

                    if (isEdit)
                    {
                        ImGui.PushID($"{key} ColorEdit");
                        if (ImGui.ColorEdit4($"##{key} ColorEdit", ref vector4,flags))
                            SetKey(key,vector4);
                        ImGui.PopID();
                    }
                    else
                    {
                        ImGui.PushID($"{key} ColorPicker");
                        if (ImGui.ColorPicker4($"##{key} ColorPicker", ref vector4,flags))
                            SetKey(key,vector4);
                        ImGui.PopID();
                    }
                }

                // Close the option's treeNode.
                ImGui.TreePop();
                Spacers(2);
            }


            ImGui.End();
        }

        public void InitializeExpanded()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory,"expand", $"{Name}.json");

            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                expanded = JsonConvert.DeserializeObject<Dictionary<string, bool>>(jsonContent);
            }
            else
            {
                expanded = new Dictionary<string, bool>();
            }
        }
        #endregion
    }
}
