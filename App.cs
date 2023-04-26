namespace Triggered
{
    using NLog;
    using NLog.Config;
    using System.Diagnostics;
    internal class App
    {
        public readonly Stopwatch Watch = Stopwatch.StartNew();
        public bool MenuDisplay_Main = true;
        public bool MenuDisplay_Log = true;
        public bool IsRunning = true;
        public int LogicTickDelayInMilliseconds = 100;
        public ExampleAppLog log;
        public Logger logger;

        public App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            log = new ExampleAppLog();
            logger = LogManager.GetCurrentClassLogger();
        }
        #region Log(string log, LogLevel level)
        public void Log(string log, LogLevel level)
        {
            // Only send message to the log window above Debug level
            if (level.Ordinal >= LogLevel.Debug.Ordinal)
            {
                this.log.AddLog(string.Format("{0}: {1}", level.ToString(), log),level);
            }
            this.logger.Log(level,log);
        }
        public void Log(string log)
        {
            this.Log(log, LogLevel.Info);
        }
        #endregion
    }
}
