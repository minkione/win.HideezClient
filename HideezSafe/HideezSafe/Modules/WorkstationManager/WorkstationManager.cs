using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Messages;
using HideezSafe.Utilities;
using NLog;
using System;
using System.Diagnostics;
using WindowsInput;

namespace HideezSafe.Modules
{
    class WorkstationManager : IWorkstationManager
    {
        readonly Logger logger = LogManager.GetCurrentClassLogger();
        readonly IInputSimulator inputSimulator = new InputSimulator();

        public WorkstationManager(IMessenger messanger)
        {
            // Start listening command messages
            messanger.Register<LockWorkstationMessage>(this, LockPC);
            messanger.Register<ForceShutdownMessage>(this, ForceShutdown);
            messanger.Register<ActivateScreenMessage>(this, ActivateScreen);
        }

        public void LockPC()
        {
            Win32Helper.LockWorkStation();
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
            // Should trigger activation of the screen in credential provider with 0 impact on user
            inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.F24);
            inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.F24);
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
