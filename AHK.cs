namespace Triggered
{
    using AutoHotkey.Interop;
    using System;

    // AHK Function Wrappers
    internal class AHK : IDisposable
    {
        private AutoHotkeyEngine ahk = AutoHotkeyEngine.Instance;
        public void Dispose()
        {
            ahk.Terminate();
        }
        // https://www.autohotkey.com/docs/v1/lib/Send.htm#keynames
        public void Send(string keys)
        {
            ahk.ExecRaw($"send {keys}");
        }
        public void ControlSend(string control = "", string keys = "", string winTitle = "", string winText = "", string excludeTitle = "", string excludeText = "")
        {
            ahk.ExecRaw($"ControlSend,{control},{keys},{winTitle},{winText},{excludeTitle},{excludeText}");
        }
        public void ControlSendRaw(string control = "", string keys = "", string winTitle = "", string winText = "", string excludeTitle = "", string excludeText = "")
        {
            ahk.ExecRaw($"ControlSendRaw,{control},{keys},{winTitle},{winText},{excludeTitle},{excludeText}");
        }
        public void MsgBox(string message)
        {
            ahk.ExecRaw($"MsgBox,{message}");
        }
        public void Demo()
        {
            ahk.ExecRaw("Run, Notepad,, Min, PID");
            ahk.ExecRaw("WinWait, ahk_pid %PID%");
            ControlSend("Edit1", "This is a line of text in the notepad window.{Enter}");
            ControlSendRaw("Edit1", "Notice that {Enter} is not sent as an Enter keystroke with ControlSendRaw.");
            MsgBox("Press OK to activate the window to see the result.");
            ahk.ExecRaw("WinActivate, ahk_pid %PID%");
        }
    }
}

