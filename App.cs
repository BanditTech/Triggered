namespace Triggered
{
    using NLog;
    using NLog.Config;
    using System.Collections.Generic;
    using System.Diagnostics;
    public static class App
    {
        public static ExampleAppLog logimgui;
        public static Logger logger;
        public static Stopwatch Watch = Stopwatch.StartNew();

        public static LogLevel LogWindowMinimumLogLevel = LogLevel.Debug;
        public static bool MenuDisplay_Main = true;
        public static bool MenuDisplay_Log = true;
        public static bool IsVisible = true;
        public static bool IsRunning = true;
        public static bool ShowTransparentViewport = true;
        public static bool DockSpaceOpen = true;
        public static int LogicTickDelayInMilliseconds = 100;
        public static int SelectedLogLevelIndex = 1;
        public static bool fullscreen = true;
        public static bool padding = false;

        static App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logimgui = new ExampleAppLog();
            logger = LogManager.GetCurrentClassLogger();
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
