namespace Triggered
{
    using NLog;
    using NLog.Config;
    using System.Collections.Generic;
    using System.Diagnostics;
    public static class App
    {
        public static Dictionary<string, object> data;
        public static ExampleAppLog logimgui;
        public static Logger logger;
        public static int selectedLogLevelIndex;

        static App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logimgui = new ExampleAppLog();
            logger = LogManager.GetCurrentClassLogger();
            data = I.O.Data;
        }
        #region Log(string log, LogLevel level)
        public static void Log(string log, LogLevel level)
        {
            // Only send message to the log window above Debug level
            if (level.Ordinal >= ((LogLevel)data["LogWindowMinimumLogLevel"]).Ordinal)
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
