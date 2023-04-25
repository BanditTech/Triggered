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
        public int LogicTickDelayInMilliseconds = 10;
        public ExampleAppLog log;
        public Logger logger;

        public App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            log = new ExampleAppLog();
            logger = LogManager.GetCurrentClassLogger();
            LogManager.ReconfigExistingLoggers();
        }
    }
}
