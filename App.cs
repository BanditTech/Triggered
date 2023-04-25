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
        private string LogRegexPattern = "[c={0}]{2}:[/c] [c={0}text]{1}[/c]";
        //Wrapper for our two logging systems
        public void Log(string log, LogLevel level)
        {
            // Only send message to the log window above info level
            if (level.Ordinal >= LogLevel.Trace.Ordinal)
            {
                string lString = level.ToString();
                this.log.AddLog(string.Format(LogRegexPattern,lString.ToLowerInvariant(),log,lString));
            }
            this.logger.Log(level,log);
        }
        public void Log(string log)
        {
            this.Log(log, LogLevel.Info);
        }
    }
}
