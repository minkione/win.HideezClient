using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;

namespace CustomAction
{
    public class CustomActions
    {
        private interface IParameters
        {
            string HostServerAddress { get; }
            bool InstallDongleDriver { get; }
            string CsrDriverInstallerPath { get; }
        }

        private class Parameters : IParameters
        {
            public Parameters()
            {
                HostServerAddress = "";
                InstallDongleDriver = false;
                CsrDriverInstallerPath = "";
            }

            public string HostServerAddress { get; set; }
            public bool InstallDongleDriver { get; set; }
            public string CsrDriverInstallerPath { get; set; }
        }

        [CustomAction]
        public static ActionResult SetupCustomComponentsAction(Session session)
        {
            session.Log("(CustomActions.InstallAction) enter.");

            if (!AreParametersSet(session))
            {
                session.Log("(CustomAction.InstallAction) Parameters are empty. Skip custom action.");
                return ActionResult.Success;
            }

            if (!TryParseParameters(session, out IParameters parameters))
            {
                session.Log("(CustomActions.InstallAction). One or more parameters could not be parsed");
                return ActionResult.Failure;
            }

            if (!IsValidParameters(session, parameters))
            {
                session.Log("(CustomActions.InstallAction). One or more parameters could not be validated");
                return ActionResult.Failure;
            }

            try
            {
                session.Log("(CustomActions.InstallAction) starting DPinst process");
                session.Log("(CustomActions.InstallAction) path to process {0}", parameters.CsrDriverInstallerPath);
                var installationProcess = Process.Start(parameters.CsrDriverInstallerPath, "/q /sw");
                var installTimeoutSeconds = 20;
                // True is returned if the process closed by itself before specified timeout
                var timedOut = !installationProcess.WaitForExit(installTimeoutSeconds * 1000); 

                if (timedOut)
                {
                    session.Log("(CustomActions.InstallAction) driver installation timed out after 20 seconds");
                    installationProcess.Kill();
                    session.Log("(CustomActions.InstallAction) terminated DPInst process");
                    return ActionResult.Failure;
                }
            }
            catch (Exception ex)
            {
                session.Log("(CustomActions.InstallAction). " + ex.ToString());
                return ActionResult.Failure;
            }

            session.Log("(CustomActions.InstallAction) was finished successfully.");
            return ActionResult.Success;
        }


        private static bool IsValidParameters(Session session, IParameters parameters)
        {
            bool hasAddress = !string.IsNullOrEmpty(parameters.HostServerAddress);
            session.Log("(CustomActions.IsValidParameters) Host address specified = {0}", hasAddress);

            bool isValidAddress = !hasAddress || Regex.IsMatch(parameters.HostServerAddress, $"^(http|https)://.+");
            session.Log("(CustomActions.IsValidParameters) HES address validated = {0}", isValidAddress);

            bool isValidDriverPath = !string.IsNullOrEmpty(parameters.CsrDriverInstallerPath) && File.Exists(parameters.CsrDriverInstallerPath);
            session.Log("(CustomActions.IsValidParameters) Driver installer path validated = {0}", isValidDriverPath);

            bool validated = isValidAddress && isValidDriverPath;

            return validated;
        }

        private static bool AreParametersSet(Session session)
        {
            var containsKeys = session.CustomActionData.ContainsKey("HesAddress") 
                && session.CustomActionData.ContainsKey("InstallDongleDriver");

            if (containsKeys)
            {
                session.Log("(CustomActions.AreParametersSet) Keys detected");
                var containsValues = !string.IsNullOrEmpty(session.CustomActionData["HesAddress"]) 
                    || !string.IsNullOrEmpty(session.CustomActionData["InstallDongleDriver"]);
                if (containsValues)
                {
                    session.Log("(CustomActions.AreParametersSet) At least one key contains value");
                    // At least one parameter is set
                    return true;
                }
                else
                {
                    session.Log("(CustomActions.AreParametersSet) Keys values are null or empty");
                    return false;
                }
            }
            else
            {
                session.Log("(CustomActions.AreParametersSet) Keys not found");
                return false;
            }

        }

        private static bool TryParseParameters(Session session, out IParameters outParameters)
        {
            bool success = true;
            var parameters = new Parameters();
            outParameters = parameters;

            try
            {
                try
                {
                    // Address where Enterprise Server is located
                    parameters.HostServerAddress = session.CustomActionData["HesAddress"];
                    session.Log("(CustomActions.TryParseParameters) HES address parsed");
                }
                catch (Exception ex)
                {
                    success = false;
                    session.Log("(CustomActions.TryParseParameters) Couldn't parse HES address");
                    session.Log("Exception message: {0}", ex.Message);
                }

                // Determines if driver installation is enabled
                if (byte.TryParse(session.CustomActionData["InstallDongleDriver"], out byte instalDongle))
                {
                    parameters.InstallDongleDriver = instalDongle != 0;
                    session.Log("(CustomActions.TryParseParameters) CSR driver option parsed");
                }
                else
                {
                    success = false;
                    session.Log("(CustomActions.TryParseParameters) Couldn't parse CSR driver option");
                }

                try
                {
                    // Path to the DPinst for CSR driver installation
                    parameters.CsrDriverInstallerPath = session.CustomActionData["CsrDriverPath"];
                    session.Log("(CustomActions.TryParseParameters) CSR driver installer path parsed");
                }
                catch (Exception ex)
                {
                    success = false;
                    session.Log("(CustomActions.TryParseParameters) Couldn't parse CSR driver installer path");
                    session.Log("Exception message: {0}", ex.Message);
                }
            }
            catch (Exception ex)
            {
                success = false;
                session.Log("(CustomActions.TryParseParameters). Couldn't parse parameters. ", ex.ToString());
            }

            return success;
        }
    }
}
