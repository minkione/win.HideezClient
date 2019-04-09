using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomAction
{
    public class CustomActions
    {
        private interface IParameters
        {
            string HostServerAddress { get; }
            bool InstallDongleDriver { get; }
            bool InstallReaderDriver { get; }
        }

        private class Parameters : IParameters
        {
            public Parameters()
            {
                HostServerAddress = "";

                InstallDongleDriver = false;
                InstallReaderDriver = false;
            }

            public string HostServerAddress { get; set; }
            public bool InstallDongleDriver { get; set; }
            public bool InstallReaderDriver { get; set; }
        }

        [CustomAction]
        public static ActionResult InstallAction(Session session)
        {
            session.Log("(CustomActions.InstallAction) enter.");

            TryParseParameters(session, out IParameters parameters);

            if (!IsValidParameters(parameters))
            {
                session.Log("(CustomActions.InstallAction). Is not valid parameters.");
                return ActionResult.Failure;
            }

            try
            {
                // System.Windows.Forms.MessageBox.Show($"{parameters.HostServerAddress}:{parameters.HostServerPort};{parameters.InstallDongleDriver};{parameters.InstallReaderDriver}");

                // TODO logic custom action
            }
            catch (Exception ex)
            {
                session.Log("(CustomActions.InstallAction). " + ex.ToString());
                return ActionResult.Failure;
            }

            session.Log("(CustomActions.InstallAction) is success.");
            return ActionResult.Success;
        }


        private static bool IsValidParameters(IParameters parameters)
        {
            bool hasAddress = !string.IsNullOrEmpty(parameters.HostServerAddress);

            bool isValidAddress = !hasAddress || Regex.IsMatch(parameters.HostServerAddress, $"^https://.+");

            return isValidAddress;
        }

        private static bool TryParseParameters(Session session, out IParameters outParameters)
        {
            bool success = true;
            var parameters = new Parameters();
            outParameters = parameters;

            try
            {
                parameters.HostServerAddress = session["HOSTSERVERADDRESS"];

                if (byte.TryParse(session["INSTALLDONGLEDRIVER"], out byte instalDongle))
                {
                    parameters.InstallDongleDriver = instalDongle != 0;
                }
                else
                {
                    success = false;
                    session.Log("(CustomActions.TryParseParameters). Error parse INSTALLDONGLEDRIVER.");
                }

                if (byte.TryParse(session["INSTALLREADERDRIVER"], out byte instalReader))
                {
                    parameters.InstallReaderDriver = instalReader != 0;
                }
                else
                {
                    success = false;
                    session.Log("(CustomActions.TryParseParameters). Error parse INSTALLREADERDRIVER.");
                }
            }
            catch (Exception ex)
            {
                success = false;
                session.Log("(CustomActions.TryParseParameters). Error parse parameters. ", ex.ToString());
            }

            return success;
        }
    }
}
