using NLog;
using NLog.Config;
using System.Collections.Generic;
using System.IO;
using Triggered.modules.panel;
using Triggered.modules.struct_game;
using Triggered.modules.struct_filter;
using System;
using ClickableTransparentOverlay;
using Triggered.modules.option;
using static Triggered.modules.wrapper.PointScaler;
using System.Diagnostics.CodeAnalysis;

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
        public static modules.options.Manager Options = new();

        /// <summary>
        /// The current values for Player resources and location.
        /// </summary>
        public static Player Player = new();

        public static Profiles Profiles = new();

        /// <summary>
        /// Font files available
        /// </summary>
        internal static readonly string[] fonts;

        /// <summary>
        /// what range
        /// </summary>
        internal static readonly string[] glyphs;

        internal static readonly Type anchorPosType = typeof(AnchorPosition);
        internal static readonly string[] anchorNames = Enum.GetNames(anchorPosType);
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Required for dynamic menu creation")]
        internal static readonly Array anchorValues = Enum.GetValues(anchorPosType);


        /// <summary>
        /// Constructing the App is a good entry point for basic configuration.
        /// </summary>
        static App()
        {
            // Create the default folders if they do not exist
            Directory.CreateDirectory("save");
            Directory.CreateDirectory("expand");
            Directory.CreateDirectory("profile");
            // Load our Options before anything else
            App.Options.Load();
            // Now we can start our ImGui LogWindow
            logimgui = new LogWindow();
            // NLog requires some setup to begin logging to file
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetCurrentClassLogger();

            // Gather a list of ttf files in our fonts directory
            string[] fontFiles = Directory.GetFiles("fonts","*.ttf");
            // Create a list to store the font names
            List<string> fontNames = new List<string>();
            foreach (string fontFile in fontFiles)
            {
                string fontName = Path.GetFileNameWithoutExtension(fontFile);
                fontNames.Add(fontName);
            }
            fonts = fontNames.ToArray();

            glyphs = Enum.GetNames(typeof(FontGlyphRangeType));

            Profiles.Initialize();
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
            var selectedLogLevelIndex = Options.Log.GetKey<int>("MinimumLogLevel");
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
