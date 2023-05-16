namespace Triggered
{
    using NLog;
    using NLog.Config;
    using System.Collections.Generic;
    using System.IO;
    using Triggered.modules.options;
    using Triggered.modules.panels;
    using Triggered.modules.struct_filter;
    using ImNodesNET;
    using System;

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

    /// <summary>
    /// Represents the Player status
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The player's current location.
        /// </summary>
        public string Location { get; set; } = "";

        /// <summary>
        /// The currently determined health.
        /// </summary>
        public float Health {get; set;} = 1f;

        /// <summary>
        /// The currently determined Mana.
        /// </summary>
        public float Mana {get; set;} = 1f;

        /// <summary>
        /// The currently determined Energy Shield.
        /// </summary>
        public float EnergyShield {get; set;} = 1f;

        /// <summary>
        /// The current Flask states
        /// </summary>
        public Flask[] Flasks { get; set; } = {
            new Flask(1),
            new Flask(2),
            new Flask(3),
            new Flask(4),
            new Flask(5),
        };
    }

    /// <summary>
    /// Represents each individual flask.
    /// </summary>
    public class Flask
    {
        /// <summary>
        /// The slot of the Flask
        /// </summary>
        protected int Slot;

        /// <summary>
        /// Determines the active state of a flask slot.
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Determines the ending timestamp of a flask.
        /// </summary>
        public DateTime EndsAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Determines the flask availability.
        /// </summary>
        public bool HasCharges { get; set; } = true;

        /// <summary>
        /// Sets the duration of a flask in seconds.
        /// </summary>
        public float Duration { get; set; } = 6.0f;

        /// <summary>
        /// Assign the bound key for this slot.
        /// </summary>
        public string boundKey { get; set; } = string.Empty;

        /// <summary>
        /// Assign a slot to the Flask to produce it.
        /// </summary>
        /// <param name="slot"></param>
        public Flask(int slot)
        {
            Slot = slot;
        }

        /// <summary>
        /// Checks if is not active, and has charges.
        /// </summary>
        /// <returns>If the flask is ready to use.</returns>
        public bool IsReady()
        {
            return !IsActive && HasCharges;
        }

        /// <summary>
        /// Determine if the flasks
        /// </summary>
        public void Fire()
        {
            if (!IsReady())
                return;
            IsActive = true;
            EndsAt = EndsAt.AddSeconds(Duration);
            // Fire boundKey or add it to keystroke manager
        }

        /// <summary>
        /// Check if any slot has expired
        /// </summary>
        public void Check()
        {
            if (IsActive && DateTime.Now >= EndsAt)
            {
                IsActive = false;
            }
            // Check for charges
        }
    }
}
