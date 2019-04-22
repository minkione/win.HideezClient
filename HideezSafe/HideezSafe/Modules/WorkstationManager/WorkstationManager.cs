using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Messages;
using HideezSafe.Utilities;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    class WorkstationManager : IWorkstationManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public WorkstationManager(IMessenger messanger)
        {
            // Start listening command messages
            messanger.Register<LockWorkstationMessage>(this, LockPC);
            messanger.Register<ForceShutdownMessage>(this, ForceShutdown);
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

        #endregion Messages handlers
    }
}
