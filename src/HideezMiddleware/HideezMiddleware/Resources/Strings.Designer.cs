﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HideezMiddleware.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("HideezMiddleware.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activation code is to long.
        /// </summary>
        internal static string ConnectionFlow_ActivationCode_Error_CodeToLong {
            get {
                return ResourceManager.GetString("ConnectionFlow.ActivationCode.Error.CodeToLong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Activation code is to short.
        /// </summary>
        internal static string ConnectionFlow_ActivationCode_Error_CodeToShort {
            get {
                return ResourceManager.GetString("ConnectionFlow.ActivationCode.Error.CodeToShort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid activation code. {0} attempts left.
        /// </summary>
        internal static string ConnectionFlow_ActivationCode_Error_InvalidCode {
            get {
                return ResourceManager.GetString("ConnectionFlow.ActivationCode.Error.InvalidCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault got locked due to too many incorrect activation attempts. Please, contact your system administrator..
        /// </summary>
        internal static string ConnectionFlow_ActivationCode_Error_LockedByInvalidAttempts {
            get {
                return ResourceManager.GetString("ConnectionFlow.ActivationCode.Error.LockedByInvalidAttempts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault is locked. Please, contact your system administrator..
        /// </summary>
        internal static string ConnectionFlow_ActivationCode_Error_VaultIsLocked {
            get {
                return ResourceManager.GetString("ConnectionFlow.ActivationCode.Error.VaultIsLocked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault is locked. Please, contact your system administrator. If you already got an activation code, please check your network connection..
        /// </summary>
        internal static string ConnectionFlow_ActivationCode_Error_VaultIsLockedNoNetwork {
            get {
                return ResourceManager.GetString("ConnectionFlow.ActivationCode.Error.VaultIsLockedNoNetwork", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please press the Button on your Hideez Key.
        /// </summary>
        internal static string ConnectionFlow_Button_PressButtonMessage {
            get {
                return ResourceManager.GetString("ConnectionFlow.Button.PressButtonMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connection failed. Make sure to confirm vault connection by pressing the vault button when you see a green light.  ({0}).
        /// </summary>
        internal static string ConnectionFlow_Connection_ConnectionFailed {
            get {
                return ResourceManager.GetString("ConnectionFlow.Connection.ConnectionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connecting vault, please wait..
        /// </summary>
        internal static string ConnectionFlow_Connection_Stage1 {
            get {
                return ResourceManager.GetString("ConnectionFlow.Connection.Stage1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connecting vault. Press the vault button to confirm..
        /// </summary>
        internal static string ConnectionFlow_Connection_Stage1_PressButton {
            get {
                return ResourceManager.GetString("ConnectionFlow.Connection.Stage1.PressButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Retrying vault connecting..
        /// </summary>
        internal static string ConnectionFlow_Connection_Stage2 {
            get {
                return ResourceManager.GetString("ConnectionFlow.Connection.Stage2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LTK error. Retrying. Press the vault button to confirm..
        /// </summary>
        internal static string ConnectionFlow_Connection_Stage2_LtkError_PressButton {
            get {
                return ResourceManager.GetString("ConnectionFlow.Connection.Stage2.LtkError.PressButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Retrying. Press the vault button to confirm..
        /// </summary>
        internal static string ConnectionFlow_Connection_Stage2_PressButton {
            get {
                return ResourceManager.GetString("ConnectionFlow.Connection.Stage2.PressButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to LTK error. Retrying pairing. Press the vault button to confirm..
        /// </summary>
        internal static string ConnectionFlow_Connection_Stage3_LtkError_PressButton {
            get {
                return ResourceManager.GetString("ConnectionFlow.Connection.Stage3.LtkError.PressButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Retrying pairing. Press the vault button to confirm..
        /// </summary>
        internal static string ConnectionFlow_Connection_Stage3_PressButton {
            get {
                return ResourceManager.GetString("ConnectionFlow.Connection.Stage3.PressButton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unexpectedly lost connection to the server. Please, check your network connection and try to connect your vault again..
        /// </summary>
        internal static string ConnectionFlow_Error_UnexpectedlyLostNetworkConnection {
            get {
                return ResourceManager.GetString("ConnectionFlow.Error.UnexpectedlyLostNetworkConnection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unexpected network error occured (code: {0}). Please, check your network connection and try to connect your vault again..
        /// </summary>
        internal static string ConnectionFlow_Error_UnexpectedNetworkError {
            get {
                return ResourceManager.GetString("ConnectionFlow.Error.UnexpectedNetworkError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Your device is in bootloader mode. Please, contact your system administrator to update your firmware..
        /// </summary>
        internal static string ConnectionFlow_Error_VaultInBootloaderMode {
            get {
                return ResourceManager.GetString("ConnectionFlow.Error.VaultInBootloaderMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault is locked. Please, contact your system administrator..
        /// </summary>
        internal static string ConnectionFlow_Error_VaultIsLocked {
            get {
                return ResourceManager.GetString("ConnectionFlow.Error.VaultIsLocked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault is locked. Please, contact your system administrator. If you already got an activation code, please check your network connection..
        /// </summary>
        internal static string ConnectionFlow_Error_VaultIsLockedNoNetwork {
            get {
                return ResourceManager.GetString("ConnectionFlow.Error.VaultIsLockedNoNetwork", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to initialize vault connection &apos;{0}&apos; ({1}). Please try again..
        /// </summary>
        internal static string ConnectionFlow_Initialization_DeviceInitializationError {
            get {
                return ResourceManager.GetString("ConnectionFlow.Initialization.DeviceInitializationError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to initialize vault connection &apos;{0}&apos;. Please try again..
        /// </summary>
        internal static string ConnectionFlow_Initialization_InitializationFailed {
            get {
                return ResourceManager.GetString("ConnectionFlow.Initialization.InitializationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Waiting for the vault initialization....
        /// </summary>
        internal static string ConnectionFlow_Initialization_WaitingInitializationMessage {
            get {
                return ResourceManager.GetString("ConnectionFlow.Initialization.WaitingInitializationMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot download vault license. Please, check your network connection..
        /// </summary>
        internal static string ConnectionFlow_License_Error_CannotDownloadLicense {
            get {
                return ResourceManager.GetString("ConnectionFlow.License.Error.CannotDownloadLicense", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid license received from HES for {0}, (EMPTY_DATA). Please, contact your administrator..
        /// </summary>
        internal static string ConnectionFlow_License_Error_EmptyLicenseData {
            get {
                return ResourceManager.GetString("ConnectionFlow.License.Error.EmptyLicenseData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid license received from HES for {0}, (EMPTY_ID). Please, contact your administrator..
        /// </summary>
        internal static string ConnectionFlow_License_Error_EmptyLicenseId {
            get {
                return ResourceManager.GetString("ConnectionFlow.License.Error.EmptyLicenseId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No licenses available for your vault. Please, contact your system administrator..
        /// </summary>
        internal static string ConnectionFlow_License_Error_NoLicensesAvailable {
            get {
                return ResourceManager.GetString("ConnectionFlow.License.Error.NoLicensesAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Updating vault licenses....
        /// </summary>
        internal static string ConnectionFlow_License_UpdatingLicenseMessage {
            get {
                return ResourceManager.GetString("ConnectionFlow.License.UpdatingLicenseMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Waiting for HES authorization....
        /// </summary>
        internal static string ConnectionFlow_MasterKey_AwaitingHESAuth {
            get {
                return ResourceManager.GetString("ConnectionFlow.MasterKey.AwaitingHESAuth", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault authorization failed unexpectedly. Please, try to connect your vault again. If the error persists, please contact your system administrator..
        /// </summary>
        internal static string ConnectionFlow_MasterKey_Error_AuthFailed {
            get {
                return ResourceManager.GetString("ConnectionFlow.MasterKey.Error.AuthFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Server connection is required for device authorization. Please,check your network connection and try again..
        /// </summary>
        internal static string ConnectionFlow_MasterKey_Error_AuthFailedNoNetwork {
            get {
                return ResourceManager.GetString("ConnectionFlow.MasterKey.Error.AuthFailedNoNetwork", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter the PIN code for your Hideez Key.
        /// </summary>
        internal static string ConnectionFlow_Pin_EnterPinMessage {
            get {
                return ResourceManager.GetString("ConnectionFlow.Pin.EnterPinMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid PIN! {0} attempts left..
        /// </summary>
        internal static string ConnectionFlow_Pin_Error_InvalidPin_ManyAttemptsLeft {
            get {
                return ResourceManager.GetString("ConnectionFlow.Pin.Error.InvalidPin.ManyAttemptsLeft", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid PIN! 1 attempt left..
        /// </summary>
        internal static string ConnectionFlow_Pin_Error_InvalidPin_OneAttemptLeft {
            get {
                return ResourceManager.GetString("ConnectionFlow.Pin.Error.InvalidPin.OneAttemptLeft", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault got locked due to too many incorrect pin attempts. Contact your system administrator..
        /// </summary>
        internal static string ConnectionFlow_Pin_Error_LockedByInvalidAttempts {
            get {
                return ResourceManager.GetString("ConnectionFlow.Pin.Error.LockedByInvalidAttempts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PIN too short.
        /// </summary>
        internal static string ConnectionFlow_Pin_Error_PinToShort {
            get {
                return ResourceManager.GetString("ConnectionFlow.Pin.Error.PinToShort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid PIN.
        /// </summary>
        internal static string ConnectionFlow_Pin_Error_WrongPin {
            get {
                return ResourceManager.GetString("ConnectionFlow.Pin.Error.WrongPin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please create new PIN code for your Hideez Key (minimum {0}).
        /// </summary>
        internal static string ConnectionFlow_Pin_NewPinMessage {
            get {
                return ResourceManager.GetString("ConnectionFlow.Pin.NewPinMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connecting to the HES server....
        /// </summary>
        internal static string ConnectionFlow_RfidConnection_ContactingHesMessage {
            get {
                return ResourceManager.GetString("ConnectionFlow.RfidConnection.ContactingHesMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot connect device. Not connected to the HES..
        /// </summary>
        internal static string ConnectionFlow_RfidConnection_Error_NotConnectedToHes {
            get {
                return ResourceManager.GetString("ConnectionFlow.RfidConnection.Error.NotConnectedToHes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault cannot be initialized. Please, check your network connection..
        /// </summary>
        internal static string ConnectionFlow_StateUpdate_Error_CannotLinkToUser {
            get {
                return ResourceManager.GetString("ConnectionFlow.StateUpdate.Error.CannotLinkToUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault is not assigned to any user. Please, contact your system administrator..
        /// </summary>
        internal static string ConnectionFlow_StateUpdate_Error_NotAssignedToUser {
            get {
                return ResourceManager.GetString("ConnectionFlow.StateUpdate.Error.NotAssignedToUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vault doesn&apos;t have a stored Primary account..
        /// </summary>
        internal static string ConnectionFlow_Unlock_Error_NoCredentials {
            get {
                return ResourceManager.GetString("ConnectionFlow.Unlock.Error.NoCredentials", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reading credentials from the vault....
        /// </summary>
        internal static string ConnectionFlow_Unlock_ReadingCredentials {
            get {
                return ResourceManager.GetString("ConnectionFlow.Unlock.ReadingCredentials", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unlocking the PC....
        /// </summary>
        internal static string ConnectionFlow_Unlock_Unlocking {
            get {
                return ResourceManager.GetString("ConnectionFlow.Unlock.Unlocking", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Uploading new credentials to the vault....
        /// </summary>
        internal static string ConnectionFlow_Update_UploadingCredentials {
            get {
                return ResourceManager.GetString("ConnectionFlow.Update.UploadingCredentials", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User authorization failed unexpectedly. Please, try to connect your vault again. If the error persists, please contact your system administrator..
        /// </summary>
        internal static string ConnectionFlow_UserAuthorization_Error_AuthFailed {
            get {
                return ResourceManager.GetString("ConnectionFlow.UserAuthorization.Error.AuthFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to (Vault serial no: {0}).
        /// </summary>
        internal static string ConnectionFlow_VaultSerialNo {
            get {
                return ResourceManager.GetString("ConnectionFlow.VaultSerialNo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bluetooth not available (state: {0}).
        /// </summary>
        internal static string ServiceComponentStatus_Bluetooth_NotAvailable {
            get {
                return ResourceManager.GetString("ServiceComponentStatus.Bluetooth.NotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HES not connected.
        /// </summary>
        internal static string ServiceComponentStatus_HES_NotConnected {
            get {
                return ResourceManager.GetString("ServiceComponentStatus.HES.NotConnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Workstation not approved on HES.
        /// </summary>
        internal static string ServiceComponentStatus_HES_WorkstationNotApproved {
            get {
                return ResourceManager.GetString("ServiceComponentStatus.HES.WorkstationNotApproved", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ERROR: {0}.
        /// </summary>
        internal static string ServiceComponentStatus_Message_Base {
            get {
                return ResourceManager.GetString("ServiceComponentStatus.Message.Base", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RFID reader not connected.
        /// </summary>
        internal static string ServiceComponentStatus_RFID_ReaderNotConnected {
            get {
                return ResourceManager.GetString("ServiceComponentStatus.RFID.ReaderNotConnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RFID service not connected.
        /// </summary>
        internal static string ServiceComponentStatus_RFID_ServiceNotConnected {
            get {
                return ResourceManager.GetString("ServiceComponentStatus.RFID.ServiceNotConnected", resourceCulture);
            }
        }
    }
}
