namespace Triggered
{
    using Newtonsoft.Json;
    using NLog;
    using NLog.Config;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Text.Json;

    public static class App
    {
        public static ExampleAppLog logimgui;
        public static Logger logger;
        public static Stopwatch Watch = Stopwatch.StartNew();
        public static LogLevel LogWindowMinimumLogLevel = LogLevel.Debug;
        public static string[] TopGroups;
        public static List<IGroupElement> StashSorterFile;

        public static int SelectedGroup = 0;
        public static int LogicTickDelayInMilliseconds = 100;
        public static int SelectedLogLevelIndex = 1;
        public static bool MenuDisplay_Main = true;
        public static bool MenuDisplay_Log = true;
        public static bool MenuDisplay_StashSorter = true;
        public static bool IsVisible = true;
        public static bool IsRunning = true;
        public static bool ShowTransparentViewport = true;
        public static bool fullscreen = true;
        public static bool padding = false;
        public static bool ShowGroupBoxContents = true;

        static App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logimgui = new ExampleAppLog();
            logger = LogManager.GetCurrentClassLogger();
            string[] TopGroups = new string[5];
            TopGroups[0] = "Lets";
            TopGroups[1] = "Debug";
            TopGroups[2] = "This";
            TopGroups[3] = "Combo";
            TopGroups[4] = "Box";
            App.TopGroups = TopGroups;
        }
        #region Log(string log, LogLevel level)
        public static void Log(string log, LogLevel level)
        {
            // Only send message to the log window above Debug level
            if (level.Ordinal >= App.LogWindowMinimumLogLevel.Ordinal)
            {
                logimgui.AddLog(string.Format("{0}: {1}", level.ToString(), log),level);
            }
            logger.Log(level,log);
        }
        public static void Log(string log)
        {
            Log(log, LogLevel.Info);
        }
        #endregion
        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
        [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Deserialize<TValue>(String, JsonSerializerOptions)")]
        public static void UpdateTopGroups()
        {
            // Load the JSON file into a string
            string jsonString = File.ReadAllText("example.json");

            // Deserialize the JSON into a list of IGroupElement objects
            var options = new JsonSerializerSettings
            {
                Converters = { new IGroupElementJsonConverter() }
            };
            List<IGroupElement> StashSorterFile = JsonConvert.DeserializeObject<List<IGroupElement>>(jsonString, options);

            // Fetch the GroupName of each TopGroup and save to a string array in App.TopGroups
            List<string> topGroupsList = new List<string>();
            foreach (IGroupElement group in StashSorterFile)
            {
                if (group is TopGroup topGroup)
                {
                    topGroupsList.Add(topGroup.GroupName);
                }
            }
            App.TopGroups = topGroupsList.ToArray();
            App.StashSorterFile = StashSorterFile;
        }
    }
}
