﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HideezSafe.HideezServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ServiceClientParameters", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
    [System.SerializableAttribute()]
    public partial class ServiceClientParameters : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private HideezSafe.HideezServiceReference.ClientType ClientTypeField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public HideezSafe.HideezServiceReference.ClientType ClientType {
            get {
                return this.ClientTypeField;
            }
            set {
                if ((this.ClientTypeField.Equals(value) != true)) {
                    this.ClientTypeField = value;
                    this.RaisePropertyChanged("ClientType");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClientType", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
    public enum ClientType : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        ServiceHost = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        DesktopClient = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        TestConsole = 2,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
    [System.SerializableAttribute()]
    public partial class HideezServiceFault : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int ErrorCodeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FaultMessageField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ErrorCode {
            get {
                return this.ErrorCodeField;
            }
            set {
                if ((this.ErrorCodeField.Equals(value) != true)) {
                    this.ErrorCodeField = value;
                    this.RaisePropertyChanged("ErrorCode");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FaultMessage {
            get {
                return this.FaultMessageField;
            }
            set {
                if ((object.ReferenceEquals(this.FaultMessageField, value) != true)) {
                    this.FaultMessageField = value;
                    this.RaisePropertyChanged("FaultMessage");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Adapter", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
    public enum Adapter : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        HES = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        RFID = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Dongle = 2,
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DeviceDTO", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
    [System.SerializableAttribute()]
    public partial class DeviceDTO : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int BatteryField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Version BootloaderVersionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Version FirmwareVersionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsBootField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsConnectedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsInitializedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string OwnerField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string SerialNoField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Battery {
            get {
                return this.BatteryField;
            }
            set {
                if ((this.BatteryField.Equals(value) != true)) {
                    this.BatteryField = value;
                    this.RaisePropertyChanged("Battery");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Version BootloaderVersion {
            get {
                return this.BootloaderVersionField;
            }
            set {
                if ((object.ReferenceEquals(this.BootloaderVersionField, value) != true)) {
                    this.BootloaderVersionField = value;
                    this.RaisePropertyChanged("BootloaderVersion");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Version FirmwareVersion {
            get {
                return this.FirmwareVersionField;
            }
            set {
                if ((object.ReferenceEquals(this.FirmwareVersionField, value) != true)) {
                    this.FirmwareVersionField = value;
                    this.RaisePropertyChanged("FirmwareVersion");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Id {
            get {
                return this.IdField;
            }
            set {
                if ((object.ReferenceEquals(this.IdField, value) != true)) {
                    this.IdField = value;
                    this.RaisePropertyChanged("Id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsBoot {
            get {
                return this.IsBootField;
            }
            set {
                if ((this.IsBootField.Equals(value) != true)) {
                    this.IsBootField = value;
                    this.RaisePropertyChanged("IsBoot");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsConnected {
            get {
                return this.IsConnectedField;
            }
            set {
                if ((this.IsConnectedField.Equals(value) != true)) {
                    this.IsConnectedField = value;
                    this.RaisePropertyChanged("IsConnected");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool IsInitialized {
            get {
                return this.IsInitializedField;
            }
            set {
                if ((this.IsInitializedField.Equals(value) != true)) {
                    this.IsInitializedField = value;
                    this.RaisePropertyChanged("IsInitialized");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Owner {
            get {
                return this.OwnerField;
            }
            set {
                if ((object.ReferenceEquals(this.OwnerField, value) != true)) {
                    this.OwnerField = value;
                    this.RaisePropertyChanged("Owner");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string SerialNo {
            get {
                return this.SerialNoField;
            }
            set {
                if ((object.ReferenceEquals(this.SerialNoField, value) != true)) {
                    this.SerialNoField = value;
                    this.RaisePropertyChanged("SerialNo");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="HideezServiceReference.IHideezService", CallbackContract=typeof(HideezSafe.HideezServiceReference.IHideezServiceCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
    public interface IHideezService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/AttachClient", ReplyAction="http://tempuri.org/IHideezService/AttachClientResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/AttachClientHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        bool AttachClient(HideezSafe.HideezServiceReference.ServiceClientParameters parameters);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/AttachClient", ReplyAction="http://tempuri.org/IHideezService/AttachClientResponse")]
        System.Threading.Tasks.Task<bool> AttachClientAsync(HideezSafe.HideezServiceReference.ServiceClientParameters parameters);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DetachClient", ReplyAction="http://tempuri.org/IHideezService/DetachClientResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/DetachClientHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void DetachClient();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DetachClient", ReplyAction="http://tempuri.org/IHideezService/DetachClientResponse")]
        System.Threading.Tasks.Task DetachClientAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Ping", ReplyAction="http://tempuri.org/IHideezService/PingResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/PingHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        int Ping();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Ping", ReplyAction="http://tempuri.org/IHideezService/PingResponse")]
        System.Threading.Tasks.Task<int> PingAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Shutdown", ReplyAction="http://tempuri.org/IHideezService/ShutdownResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/ShutdownHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void Shutdown();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Shutdown", ReplyAction="http://tempuri.org/IHideezService/ShutdownResponse")]
        System.Threading.Tasks.Task ShutdownAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetAdapterState", ReplyAction="http://tempuri.org/IHideezService/GetAdapterStateResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/GetAdapterStateHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        bool GetAdapterState(HideezSafe.HideezServiceReference.Adapter adapter);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetAdapterState", ReplyAction="http://tempuri.org/IHideezService/GetAdapterStateResponse")]
        System.Threading.Tasks.Task<bool> GetAdapterStateAsync(HideezSafe.HideezServiceReference.Adapter adapter);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetDevices", ReplyAction="http://tempuri.org/IHideezService/GetDevicesResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/GetDevicesHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        HideezSafe.HideezServiceReference.DeviceDTO[] GetDevices();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetDevices", ReplyAction="http://tempuri.org/IHideezService/GetDevicesResponse")]
        System.Threading.Tasks.Task<HideezSafe.HideezServiceReference.DeviceDTO[]> GetDevicesAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/OnSessionChange", ReplyAction="http://tempuri.org/IHideezService/OnSessionChangeResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/OnSessionChangeHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void OnSessionChange(bool sessionLocked);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/OnSessionChange", ReplyAction="http://tempuri.org/IHideezService/OnSessionChangeResponse")]
        System.Threading.Tasks.Task OnSessionChangeAsync(bool sessionLocked);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DisconnectDevice", ReplyAction="http://tempuri.org/IHideezService/DisconnectDeviceResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/DisconnectDeviceHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void DisconnectDevice(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DisconnectDevice", ReplyAction="http://tempuri.org/IHideezService/DisconnectDeviceResponse")]
        System.Threading.Tasks.Task DisconnectDeviceAsync(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoveDevice", ReplyAction="http://tempuri.org/IHideezService/RemoveDeviceResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/RemoveDeviceHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void RemoveDevice(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoveDevice", ReplyAction="http://tempuri.org/IHideezService/RemoveDeviceResponse")]
        System.Threading.Tasks.Task RemoveDeviceAsync(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/EstablishRemoteDeviceConnection", ReplyAction="http://tempuri.org/IHideezService/EstablishRemoteDeviceConnectionResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/EstablishRemoteDeviceConnectionHideezServiceFau" +
            "ltFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        string EstablishRemoteDeviceConnection(string serialNo, byte channelNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/EstablishRemoteDeviceConnection", ReplyAction="http://tempuri.org/IHideezService/EstablishRemoteDeviceConnectionResponse")]
        System.Threading.Tasks.Task<string> EstablishRemoteDeviceConnectionAsync(string serialNo, byte channelNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoteConnection_AuthCommand", ReplyAction="http://tempuri.org/IHideezService/RemoteConnection_AuthCommandResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/RemoteConnection_AuthCommandHideezServiceFaultF" +
            "ault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        byte[] RemoteConnection_AuthCommand(string connectionId, byte[] data);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoteConnection_AuthCommand", ReplyAction="http://tempuri.org/IHideezService/RemoteConnection_AuthCommandResponse")]
        System.Threading.Tasks.Task<byte[]> RemoteConnection_AuthCommandAsync(string connectionId, byte[] data);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoteConnection_RemoteCommand", ReplyAction="http://tempuri.org/IHideezService/RemoteConnection_RemoteCommandResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/RemoteConnection_RemoteCommandHideezServiceFaul" +
            "tFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        byte[] RemoteConnection_RemoteCommand(string connectionId, byte[] data);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoteConnection_RemoteCommand", ReplyAction="http://tempuri.org/IHideezService/RemoteConnection_RemoteCommandResponse")]
        System.Threading.Tasks.Task<byte[]> RemoteConnection_RemoteCommandAsync(string connectionId, byte[] data);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoteConnection_ResetChannel", ReplyAction="http://tempuri.org/IHideezService/RemoteConnection_ResetChannelResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezSafe.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/RemoteConnection_ResetChannelHideezServiceFault" +
            "Fault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void RemoteConnection_ResetChannel(string connectionId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoteConnection_ResetChannel", ReplyAction="http://tempuri.org/IHideezService/RemoteConnection_ResetChannelResponse")]
        System.Threading.Tasks.Task RemoteConnection_ResetChannelAsync(string connectionId);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IHideezServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/LockWorkstationRequest")]
        void LockWorkstationRequest();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/ActivateWorkstationScreenRequest")]
        void ActivateWorkstationScreenRequest();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/HESConnectionStateChanged")]
        void HESConnectionStateChanged(bool isConnected);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/RFIDConnectionStateChanged")]
        void RFIDConnectionStateChanged(bool isConnected);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/DongleConnectionStateChanged")]
        void DongleConnectionStateChanged(bool isConnected);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/DevicesCollectionChanged")]
        void DevicesCollectionChanged(HideezSafe.HideezServiceReference.DeviceDTO[] devices);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/DeviceConnectionStateChanged")]
        void DeviceConnectionStateChanged(HideezSafe.HideezServiceReference.DeviceDTO device);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/DeviceInitialized")]
        void DeviceInitialized(HideezSafe.HideezServiceReference.DeviceDTO device);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/RemoteConnection_RssiReceived")]
        void RemoteConnection_RssiReceived(string connectionId, double rssi);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/RemoteConnection_BatteryChanged")]
        void RemoteConnection_BatteryChanged(string connectioNid, int battery);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IHideezServiceChannel : HideezSafe.HideezServiceReference.IHideezService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class HideezServiceClient : System.ServiceModel.DuplexClientBase<HideezSafe.HideezServiceReference.IHideezService>, HideezSafe.HideezServiceReference.IHideezService {
        
        public HideezServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public HideezServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public HideezServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public HideezServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public HideezServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public bool AttachClient(HideezSafe.HideezServiceReference.ServiceClientParameters parameters) {
            return base.Channel.AttachClient(parameters);
        }
        
        public System.Threading.Tasks.Task<bool> AttachClientAsync(HideezSafe.HideezServiceReference.ServiceClientParameters parameters) {
            return base.Channel.AttachClientAsync(parameters);
        }
        
        public void DetachClient() {
            base.Channel.DetachClient();
        }
        
        public System.Threading.Tasks.Task DetachClientAsync() {
            return base.Channel.DetachClientAsync();
        }
        
        public int Ping() {
            return base.Channel.Ping();
        }
        
        public System.Threading.Tasks.Task<int> PingAsync() {
            return base.Channel.PingAsync();
        }
        
        public void Shutdown() {
            base.Channel.Shutdown();
        }
        
        public System.Threading.Tasks.Task ShutdownAsync() {
            return base.Channel.ShutdownAsync();
        }
        
        public bool GetAdapterState(HideezSafe.HideezServiceReference.Adapter adapter) {
            return base.Channel.GetAdapterState(adapter);
        }
        
        public System.Threading.Tasks.Task<bool> GetAdapterStateAsync(HideezSafe.HideezServiceReference.Adapter adapter) {
            return base.Channel.GetAdapterStateAsync(adapter);
        }
        
        public HideezSafe.HideezServiceReference.DeviceDTO[] GetDevices() {
            return base.Channel.GetDevices();
        }
        
        public System.Threading.Tasks.Task<HideezSafe.HideezServiceReference.DeviceDTO[]> GetDevicesAsync() {
            return base.Channel.GetDevicesAsync();
        }
        
        public void OnSessionChange(bool sessionLocked) {
            base.Channel.OnSessionChange(sessionLocked);
        }
        
        public System.Threading.Tasks.Task OnSessionChangeAsync(bool sessionLocked) {
            return base.Channel.OnSessionChangeAsync(sessionLocked);
        }
        
        public void DisconnectDevice(string id) {
            base.Channel.DisconnectDevice(id);
        }
        
        public System.Threading.Tasks.Task DisconnectDeviceAsync(string id) {
            return base.Channel.DisconnectDeviceAsync(id);
        }
        
        public void RemoveDevice(string id) {
            base.Channel.RemoveDevice(id);
        }
        
        public System.Threading.Tasks.Task RemoveDeviceAsync(string id) {
            return base.Channel.RemoveDeviceAsync(id);
        }
        
        public string EstablishRemoteDeviceConnection(string serialNo, byte channelNo) {
            return base.Channel.EstablishRemoteDeviceConnection(serialNo, channelNo);
        }
        
        public System.Threading.Tasks.Task<string> EstablishRemoteDeviceConnectionAsync(string serialNo, byte channelNo) {
            return base.Channel.EstablishRemoteDeviceConnectionAsync(serialNo, channelNo);
        }
        
        public byte[] RemoteConnection_AuthCommand(string connectionId, byte[] data) {
            return base.Channel.RemoteConnection_AuthCommand(connectionId, data);
        }
        
        public System.Threading.Tasks.Task<byte[]> RemoteConnection_AuthCommandAsync(string connectionId, byte[] data) {
            return base.Channel.RemoteConnection_AuthCommandAsync(connectionId, data);
        }
        
        public byte[] RemoteConnection_RemoteCommand(string connectionId, byte[] data) {
            return base.Channel.RemoteConnection_RemoteCommand(connectionId, data);
        }
        
        public System.Threading.Tasks.Task<byte[]> RemoteConnection_RemoteCommandAsync(string connectionId, byte[] data) {
            return base.Channel.RemoteConnection_RemoteCommandAsync(connectionId, data);
        }
        
        public void RemoteConnection_ResetChannel(string connectionId) {
            base.Channel.RemoteConnection_ResetChannel(connectionId);
        }
        
        public System.Threading.Tasks.Task RemoteConnection_ResetChannelAsync(string connectionId) {
            return base.Channel.RemoteConnection_ResetChannelAsync(connectionId);
        }
    }
}
