﻿namespace Triggered
{
    using Newtonsoft.Json;
    using NLog;
    using NLog.Config;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Text.Json;

    public static class App
    {
        public static ExampleAppLog logimgui;
        public static Logger logger;
        public static Stopwatch Watch = Stopwatch.StartNew();
        public static LogLevel LogWindowMinimumLogLevel = LogLevel.Debug;
        public static string[] TopGroups;
        public static List<IGroupElement> StashSorterList;

        public static int SelectedGroup = 0;
        public static int LogicTickDelayInMilliseconds = 100;
        public static int SelectedLogLevelIndex = 1;
        public static bool MenuDisplay_Main = true;
        public static bool MenuDisplay_Log = true;
        public static bool MenuDisplay_StashSorter = true;
        public static bool IsVisible = true;
        public static bool IsRunning = true;
        public static bool ShowTransparentViewport = true;
        public static bool fullscreen = true;
        public static bool padding = false;
        public static bool ShowGroupBoxContents = true;
        public static ImFontPtr AlmaMono;
        public static ImFontPtr NotoSans;

        static App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logimgui = new ExampleAppLog();
            logger = LogManager.GetCurrentClassLogger();
            string fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", "AlmaMono-Regular.ttf");
            AlmaMono = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 18.0f);
            fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", "NotoSans-Regular.ttf");
            NotoSans = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 18.0f);
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
