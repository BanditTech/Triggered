using NLog;
using NLog.Config;
using System.Collections.Generic;
using System.IO;
using Triggered.modules.options;
using Triggered.modules.panels;
using Triggered.modules.struct_game;
using Triggered.modules.struct_filter;

namespace Triggered
{
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
        public static Options_Manager Options = new();

        /// <summary>
        /// The current values for Player resources and location.
        /// </summary>
        public static Player Player = new();

        /// <summary>
        /// Constructing the App is a good entry point for basic configuration.
        /// </summary>
        static App()
        {
            // Create the default folders if they do not exist
            Directory.CreateDirectory("save");
            Directory.CreateDirectory("profile");
            // Load our Options before anything else
            App.Options.Load();
            // Now we can start our ImGui LogWindow
            logimgui = new LogWindow();
            // NLog requires some setup to begin logging to file
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetCurrentClassLogger();
        }

        #region Log(string log, LogLevel level)
        /// <summary>
        /// A uniform point to send log entries to both logger systems.<br/>
        /// Log levels ordered by severity:<br/>
        /// - <see cref="LogLevel.Trace"/> (Ordinal = 0) : Most verbose level. Used for development and seldom enabled in production.<br/>
        /// - <see cref="LogLevel.Debug"/> (Ordinal = 1) : Debugging the application behavior from internal events of interest.<br/>
        /// - <see cref="LogLevel.Info"/>  (Ordinal = 2) : Information that highlights progress or application lifetime events.<br/>
        /// - <see cref="LogLevel.Warn"/>  (Ordinal = 3) : Warnings about validation issues or temporary failures that can be recovered.<br/>
        /// - <see cref="LogLevel.Error"/> (Ordinal = 4) : Errors where functionality has failed or <see cref="System.Exception"/> have been caught.<br/>
        /// - <see cref="LogLevel.Fatal"/> (Ordinal = 5) : Most critical level. Application is about to abort.<br/>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level">NLog LogLevel</param>
        public static void Log(string log, LogLevel level)
        {
            var selectedLogLevelIndex = App.Options.MainMenu.GetKey<int>("SelectedLogLevelIndex");
            // Only send message to the log window above Debug level
            if (level.Ordinal >= selectedLogLevelIndex)
            {
                logimgui.AddLog(string.Format("{0}: {1}", level.ToString(), log), level);
            }
            logger.Log(level, log);
        }
        /// <summary>
        /// Simplify the format of creating info log entries.<br/>
        /// Log levels ordered by severity:<br/>
        /// - <see cref="LogLevel.Trace"/> (Ordinal = 0) : Most verbose level. Used for development and seldom enabled in production.<br/>
        /// - <see cref="LogLevel.Debug"/> (Ordinal = 1) : Debugging the application behavior from internal events of interest.<br/>
        /// - <see cref="LogLevel.Info"/>  (Ordinal = 2) : Information that highlights progress or application lifetime events.<br/>
        /// - <see cref="LogLevel.Warn"/>  (Ordinal = 3) : Warnings about validation issues or temporary failures that can be recovered.<br/>
        /// - <see cref="LogLevel.Error"/> (Ordinal = 4) : Errors where functionality has failed or <see cref="System.Exception"/> have been caught.<br/>
        /// - <see cref="LogLevel.Fatal"/> (Ordinal = 5) : Most critical level. Application is about to abort.<br/>
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level"></param>
        public static void Log(string log, int level = 2)
        {
            if (level > 5)
                level = 5;
            else if (level < 0)
                level = 0;

            var levelType = LogLevel.FromOrdinal(level);
            Log(log, levelType);
        }
        #endregion
    }
}
