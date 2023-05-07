namespace Triggered
{
    using NLog;
    using NLog.Config;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Triggered.modules.options;
    using Triggered.modules.panels;

    public static class App
    {
        public static bool IsVisible = true;
        public static bool IsRunning = true;
        public static readonly string[] EvalOptions = new string[] { ">=", ">", "=", "<", "<=", "~=", ">0<", ">0<=", "!=" };
        public static readonly string[] GroupTypes = new string[] { "AND", "NOT", "COUNT", "WEIGHT" };
        public static readonly string[] objectTypes = new string[] { "Group", "Element" };
        public static LogWindow logimgui;
        public static Logger logger;
        public static Stopwatch Watch = Stopwatch.StartNew();
        public static LogLevel LogWindowMinimumLogLevel;
        public static string[] TopGroups;
        public static List<AGroupElement> StashSorterList;
        public static Type[] ObjectTypes = new Type[] { typeof(Group), typeof(Element) };
        public static Options_Manager Options = new Options_Manager();

        static App()
        {
            var selectedLogLevelIndex = App.Options.MainMenu.GetKey<int>("SelectedLogLevelIndex");
            LogWindowMinimumLogLevel = LogWindow.logLevels[selectedLogLevelIndex];
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logimgui = new LogWindow();
            logger = LogManager.GetCurrentClassLogger();
            Directory.CreateDirectory("save");
            Directory.CreateDirectory("profile");
            App.Options.Load();
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
