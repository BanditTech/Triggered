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

    /// <summary>
    /// The is the main hub for the application.
    /// Holds most of the shared resources and all options.
    /// </summary>
    public static class App
    {
        /// <summary>
        /// This is the on/off switch for the application.
        /// </summary>
        public static bool IsRunning = true;
        /// <summary>
        /// A Log Window rendered using ImGui.
        /// </summary>
        public static LogWindow logimgui;
        /// <summary>
        /// An instance of NLog LogManager.
        /// </summary>
        public static Logger logger;
        /// <summary>
        /// A stored list of the loaded StashFilter groups.
        /// </summary>
        public static string[] TopGroups;
        /// <summary>
        /// The loaded Stash Sorter list, which will contain the [{TopGroup},{TopGroup}] structure.
        /// </summary>
        public static List<AGroupElement> StashSorterList;
        /// <summary>
        /// Options are loaded as a group using a Manager.
        /// </summary>
        public static Options_Manager Options = new Options_Manager();

        static App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logimgui = new LogWindow();
            logger = LogManager.GetCurrentClassLogger();
            Directory.CreateDirectory("save");
            Directory.CreateDirectory("profile");
            App.Options.Load();
        }

        #region Log(string log, LogLevel level)
        /// <summary>
        /// A uniform point to send log entries to both 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level">NLog LogLevel</param>
        public static void Log(string log, LogLevel level)
        {
            var selectedLogLevelIndex = App.Options.MainMenu.GetKey<int>("SelectedLogLevelIndex");
            // Only send message to the log window above Debug level
            if (level.Ordinal >= selectedLogLevelIndex)
            {
                logimgui.AddLog(string.Format("{0}: {1}", level.ToString(), log),level);
            }
            logger.Log(level,log);
        }
        /// <summary>
        /// Simplify the format of creating info log entries.
        /// </summary>
        /// <param name="log"></param>
        public static void Log(string log)
        {
            Log(log, LogLevel.Info);
        }
        #endregion
    }
}
