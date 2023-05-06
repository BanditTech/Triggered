namespace Triggered
{
    using Newtonsoft.Json.Linq;
    using NLog;
    using NLog.Config;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public static class App
    {
        public static ExampleAppLog logimgui;
        public static Logger logger;
        public static Stopwatch Watch = Stopwatch.StartNew();
        public static LogLevel LogWindowMinimumLogLevel = LogLevel.Debug;
        public static string[] TopGroups;
        public static List<AGroupElement> StashSorterList;

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
        public static string[] EvalOptions = new string[] { ">=", ">", "=", "<", "<=", "~=", ">0<", ">0<=", "!=" };
        public static string[] GroupTypes = new string[] { "AND", "NOT", "COUNT", "WEIGHT" };
        public static string[] objectTypes = new string[] { "Group", "Element" };
        public static Type[] ObjectTypes = new Type[] { typeof(Group), typeof(Element) };
        public static AppOptions Options = new AppOptions();

        static App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logimgui = new ExampleAppLog();
            logger = LogManager.GetCurrentClassLogger();

            //MainMenuOptions mainMenuOptions = new MainMenuOptions();
            //App.Log($"Main Menu Options created this object:\n" +
            //    $"{mainMenuOptions.ToJson()}");
            //mainMenuOptions.SetKey("Change_Me",false);
            //mainMenuOptions.SetKey("MenuDisplay_StashSorter", 2);
            //App.Log($"Changing the values resulted in:\n" +
            //    $"{mainMenuOptions.ToJson()}");
            var saveObj = App.Options.MainMenu.PrepareSaveObject();
            App.Log($"Stripped save file is:\n" +
                $"{saveObj}");
            Directory.CreateDirectory("save");
            File.WriteAllText("save\\save.json",JSON.Str(saveObj));
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
    }
}
