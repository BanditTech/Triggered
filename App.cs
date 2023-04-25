namespace Triggered
{
    using System.Diagnostics;
    internal class App
    {
        public readonly Stopwatch Watch = Stopwatch.StartNew();
        public bool MenuDisplay_Main = true;
        public bool MenuDisplay_Log = true;
        public bool IsRunning = true;
        public int LogicTickDelayInMilliseconds = 10;
        public ExampleAppLog log;

        public App()
        {
            log = new ExampleAppLog();
        }

        public void Log(string message)
        {
            this.log.AddLog(message);
            return;
        }
    }
}
