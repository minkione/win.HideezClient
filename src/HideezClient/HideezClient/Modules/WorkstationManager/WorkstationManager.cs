using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using HideezClient.Messages;
using HideezClient.Modules.SessionStateMonitor;
using HideezClient.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsInput;

namespace HideezClient.Modules
{
    class WorkstationManager : Logger, IWorkstationManager
    {
        readonly IInputSimulator inputSimulator = new InputSimulator();
        readonly ISessionStateMonitor sessionStateMonitor;

        public WorkstationManager(IMessenger messanger, ISessionStateMonitor sessionStateMonitor, ILog log)
            : base(nameof(WorkstationManager), log)
        {
            this.sessionStateMonitor = sessionStateMonitor;

            // Start listening command messages
            messanger.Register<LockWorkstationMessage>(this, LockPC);
            messanger.Register<ForceShutdownMessage>(this, ForceShutdown);
            messanger.Register<ActivateScreenMessage>(this, ActivateScreen);
        }

        public void LockPC()
        {
            WriteLine($"Calling Win32.LockWorkstation");
            var result = Win32Helper.LockWorkStation();
            WriteLine($"Win32.LockWorkstation result: {result}");
            if (result == false)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void ForceShutdown()
        {
            var process = new ProcessStartInfo("shutdown", "/s /f /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(process);
        }

        public void ActivateScreen()
        {
            if (sessionStateMonitor.CurrentState == SessionState.Locked ||
                sessionStateMonitor.CurrentState == SessionState.Unknown)
            {
                // Should trigger activation of the screen in credential provider with zero impact on user
                //inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.F12);
                inputSimulator.Mouse.RightButtonClick(); // Windows 10 lockui ignores right mouse button for controls interaction
            }
        }

        #region Messages handlers

        private void LockPC(LockWorkstationMessage command)
        {
            try
            {
                LockPC();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        private void ForceShutdown(ForceShutdownMessage command)
        {
            try
            {
                ForceShutdown();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        private void ActivateScreen(ActivateScreenMessage command)
        {
            try
            {
                ActivateScreen();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
        #endregion Messages handlers
    }
}
