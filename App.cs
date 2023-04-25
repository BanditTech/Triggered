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

        //Wrapper for our two logging systems
        public void Log(string log, LogLevel level)
        {
            // Only send message to the log window above info level
            if (level.Ordinal >= LogLevel.Trace.Ordinal)
            {
                this.log.AddLog(string.Format("[c={0}]{0}:[/c] [c={0}text]{1}[/c]",level.ToString().ToLowerInvariant(),log));
            }
            this.logger.Log(level,log);
        }
        public void Log(string log)
        {
            this.Log(log, LogLevel.Info);
        }
    }
}
