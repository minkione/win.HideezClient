using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Modules.SessionStateMonitor;
using HideezClient.Utilities;
using NLog;
using System;
using System.Diagnostics;
using WindowsInput;

namespace HideezClient.Modules
{
    class WorkstationManager : IWorkstationManager
    {
        readonly Logger logger = LogManager.GetCurrentClassLogger();
        readonly IInputSimulator inputSimulator = new InputSimulator();
        readonly ISessionStateMonitor sessionStateMonitor;

        public WorkstationManager(IMessenger messanger, ISessionStateMonitor sessionStateMonitor)
        {
            this.sessionStateMonitor = sessionStateMonitor;

            // Start listening command messages
            messanger.Register<LockWorkstationMessage>(this, LockPC);
            messanger.Register<ForceShutdownMessage>(this, ForceShutdown);
            messanger.Register<ActivateScreenMessage>(this, ActivateScreen);
        }

        public void LockPC()
        {
#if !DEBUG
            Win32Helper.LockWorkStation();
#endif
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
                // Should trigger activation of the screen in credential provider with 0 impact on user
                inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.ESCAPE);
                inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.ESCAPE);
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
                logger.Error(ex);
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
                logger.Error(ex);
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
                logger.Error(ex);
            }
        }
        #endregion Messages handlers
    }
}
