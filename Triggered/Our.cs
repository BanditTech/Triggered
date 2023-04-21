using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Triggered
{
    class Our
    {
        private static readonly Logger _logger;

        static Our()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public static void Log(string message, string level = "Info")
        {
            switch (level.ToLower())
            {
                case "t":
                case "trace":
                    _logger.Trace(message);
                    break;
                case "d":
                case "debug":
                    _logger.Debug(message);
                    break;
                case "i":
                case "info":
                    _logger.Info(message);
                    break;
                case "w":
                case "warn":
                case "warning":
                    _logger.Warn(message);
                    break;
                case "e":
                case "error":
                    _logger.Error(message);
                    break;
                case "f":
                case "fatal":
                    _logger.Fatal(message);
                    break;
                default:
                    _logger.Error($"Invalid log level '{level}', defaulting to 'Info'");
                    _logger.Info(message);
                    break;
            }
        }
    }
}
