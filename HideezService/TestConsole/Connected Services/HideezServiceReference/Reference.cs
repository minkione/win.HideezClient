﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestConsole.HideezServiceReference {
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
        private TestConsole.HideezServiceReference.ClientType ClientTypeField;
        
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
        public TestConsole.HideezServiceReference.ClientType ClientType {
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
        private string IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool IsConnectedField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string OwnerField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private double ProximityField;
        
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
        public double Proximity {
            get {
                return this.ProximityField;
            }
            set {
                if ((this.ProximityField.Equals(value) != true)) {
                    this.ProximityField = value;
                    this.RaisePropertyChanged("Proximity");
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
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="HideezServiceReference.IHideezService", CallbackContract=typeof(TestConsole.HideezServiceReference.IHideezServiceCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
    public interface IHideezService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/AttachClient", ReplyAction="http://tempuri.org/IHideezService/AttachClientResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/AttachClientHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        bool AttachClient(TestConsole.HideezServiceReference.ServiceClientParameters parameters);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/AttachClient", ReplyAction="http://tempuri.org/IHideezService/AttachClientResponse")]
        System.Threading.Tasks.Task<bool> AttachClientAsync(TestConsole.HideezServiceReference.ServiceClientParameters parameters);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DetachClient", ReplyAction="http://tempuri.org/IHideezService/DetachClientResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/DetachClientHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void DetachClient();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DetachClient", ReplyAction="http://tempuri.org/IHideezService/DetachClientResponse")]
        System.Threading.Tasks.Task DetachClientAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Ping", ReplyAction="http://tempuri.org/IHideezService/PingResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/PingHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        int Ping();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Ping", ReplyAction="http://tempuri.org/IHideezService/PingResponse")]
        System.Threading.Tasks.Task<int> PingAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Shutdown", ReplyAction="http://tempuri.org/IHideezService/ShutdownResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/ShutdownHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void Shutdown();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Shutdown", ReplyAction="http://tempuri.org/IHideezService/ShutdownResponse")]
        System.Threading.Tasks.Task ShutdownAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetAdapterState", ReplyAction="http://tempuri.org/IHideezService/GetAdapterStateResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/GetAdapterStateHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        bool GetAdapterState(TestConsole.HideezServiceReference.Adapter adapter);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetAdapterState", ReplyAction="http://tempuri.org/IHideezService/GetAdapterStateResponse")]
        System.Threading.Tasks.Task<bool> GetAdapterStateAsync(TestConsole.HideezServiceReference.Adapter adapter);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetPairedDevices", ReplyAction="http://tempuri.org/IHideezService/GetPairedDevicesResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/GetPairedDevicesHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        TestConsole.HideezServiceReference.DeviceDTO[] GetPairedDevices();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetPairedDevices", ReplyAction="http://tempuri.org/IHideezService/GetPairedDevicesResponse")]
        System.Threading.Tasks.Task<TestConsole.HideezServiceReference.DeviceDTO[]> GetPairedDevicesAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/EnableMonitoringProximity", ReplyAction="http://tempuri.org/IHideezService/EnableMonitoringProximityResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/EnableMonitoringProximityHideezServiceFaultFaul" +
            "t", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void EnableMonitoringProximity(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/EnableMonitoringProximity", ReplyAction="http://tempuri.org/IHideezService/EnableMonitoringProximityResponse")]
        System.Threading.Tasks.Task EnableMonitoringProximityAsync(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DisableMonitoringProximity", ReplyAction="http://tempuri.org/IHideezService/DisableMonitoringProximityResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/DisableMonitoringProximityHideezServiceFaultFau" +
            "lt", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void DisableMonitoringProximity(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DisableMonitoringProximity", ReplyAction="http://tempuri.org/IHideezService/DisableMonitoringProximityResponse")]
        System.Threading.Tasks.Task DisableMonitoringProximityAsync(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/EnableMonitoringDeviceProperties", ReplyAction="http://tempuri.org/IHideezService/EnableMonitoringDevicePropertiesResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/EnableMonitoringDevicePropertiesHideezServiceFa" +
            "ultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void EnableMonitoringDeviceProperties(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/EnableMonitoringDeviceProperties", ReplyAction="http://tempuri.org/IHideezService/EnableMonitoringDevicePropertiesResponse")]
        System.Threading.Tasks.Task EnableMonitoringDevicePropertiesAsync(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DisableMonitoringDeviceProperties", ReplyAction="http://tempuri.org/IHideezService/DisableMonitoringDevicePropertiesResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/DisableMonitoringDevicePropertiesHideezServiceF" +
            "aultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void DisableMonitoringDeviceProperties(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DisableMonitoringDeviceProperties", ReplyAction="http://tempuri.org/IHideezService/DisableMonitoringDevicePropertiesResponse")]
        System.Threading.Tasks.Task DisableMonitoringDevicePropertiesAsync(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/OnSessionChange", ReplyAction="http://tempuri.org/IHideezService/OnSessionChangeResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/OnSessionChangeHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void OnSessionChange(bool sessionLocked);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/OnSessionChange", ReplyAction="http://tempuri.org/IHideezService/OnSessionChangeResponse")]
        System.Threading.Tasks.Task OnSessionChangeAsync(bool sessionLocked);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/SaveCredential", ReplyAction="http://tempuri.org/IHideezService/SaveCredentialResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/SaveCredentialHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void SaveCredential(string deviceId, string login, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/SaveCredential", ReplyAction="http://tempuri.org/IHideezService/SaveCredentialResponse")]
        System.Threading.Tasks.Task SaveCredentialAsync(string deviceId, string login, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DisconnectDevice", ReplyAction="http://tempuri.org/IHideezService/DisconnectDeviceResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/DisconnectDeviceHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void DisconnectDevice(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DisconnectDevice", ReplyAction="http://tempuri.org/IHideezService/DisconnectDeviceResponse")]
        System.Threading.Tasks.Task DisconnectDeviceAsync(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoveDevice", ReplyAction="http://tempuri.org/IHideezService/RemoveDeviceResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(TestConsole.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/RemoveDeviceHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void RemoveDevice(string deviceId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/RemoveDevice", ReplyAction="http://tempuri.org/IHideezService/RemoveDeviceResponse")]
        System.Threading.Tasks.Task RemoveDeviceAsync(string deviceId);
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
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/PairedDevicesCollectionChanged")]
        void PairedDevicesCollectionChanged(TestConsole.HideezServiceReference.DeviceDTO[] devices);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/PairedDevicePropertyChanged")]
        void PairedDevicePropertyChanged(TestConsole.HideezServiceReference.DeviceDTO device);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/ProximityChanged")]
        void ProximityChanged(string deviceId, double proximity);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IHideezServiceChannel : TestConsole.HideezServiceReference.IHideezService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class HideezServiceClient : System.ServiceModel.DuplexClientBase<TestConsole.HideezServiceReference.IHideezService>, TestConsole.HideezServiceReference.IHideezService {
        
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
        
        public bool AttachClient(TestConsole.HideezServiceReference.ServiceClientParameters parameters) {
            return base.Channel.AttachClient(parameters);
        }
        
        public System.Threading.Tasks.Task<bool> AttachClientAsync(TestConsole.HideezServiceReference.ServiceClientParameters parameters) {
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
        
        public bool GetAdapterState(TestConsole.HideezServiceReference.Adapter adapter) {
            return base.Channel.GetAdapterState(adapter);
        }
        
        public System.Threading.Tasks.Task<bool> GetAdapterStateAsync(TestConsole.HideezServiceReference.Adapter adapter) {
            return base.Channel.GetAdapterStateAsync(adapter);
        }
        
        public TestConsole.HideezServiceReference.DeviceDTO[] GetPairedDevices() {
            return base.Channel.GetPairedDevices();
        }
        
        public System.Threading.Tasks.Task<TestConsole.HideezServiceReference.DeviceDTO[]> GetPairedDevicesAsync() {
            return base.Channel.GetPairedDevicesAsync();
        }
        
        public void EnableMonitoringProximity(string deviceId) {
            base.Channel.EnableMonitoringProximity(deviceId);
        }
        
        public System.Threading.Tasks.Task EnableMonitoringProximityAsync(string deviceId) {
            return base.Channel.EnableMonitoringProximityAsync(deviceId);
        }
        
        public void DisableMonitoringProximity(string deviceId) {
            base.Channel.DisableMonitoringProximity(deviceId);
        }
        
        public System.Threading.Tasks.Task DisableMonitoringProximityAsync(string deviceId) {
            return base.Channel.DisableMonitoringProximityAsync(deviceId);
        }
        
        public void EnableMonitoringDeviceProperties(string deviceId) {
            base.Channel.EnableMonitoringDeviceProperties(deviceId);
        }
        
        public System.Threading.Tasks.Task EnableMonitoringDevicePropertiesAsync(string deviceId) {
            return base.Channel.EnableMonitoringDevicePropertiesAsync(deviceId);
        }
        
        public void DisableMonitoringDeviceProperties(string deviceId) {
            base.Channel.DisableMonitoringDeviceProperties(deviceId);
        }
        
        public System.Threading.Tasks.Task DisableMonitoringDevicePropertiesAsync(string deviceId) {
            return base.Channel.DisableMonitoringDevicePropertiesAsync(deviceId);
        }
        
        public void OnSessionChange(bool sessionLocked) {
            base.Channel.OnSessionChange(sessionLocked);
        }
        
        public System.Threading.Tasks.Task OnSessionChangeAsync(bool sessionLocked) {
            return base.Channel.OnSessionChangeAsync(sessionLocked);
        }
        
        public void SaveCredential(string deviceId, string login, string password) {
            base.Channel.SaveCredential(deviceId, login, password);
        }
        
        public System.Threading.Tasks.Task SaveCredentialAsync(string deviceId, string login, string password) {
            return base.Channel.SaveCredentialAsync(deviceId, login, password);
        }
        
        public void DisconnectDevice(string deviceId) {
            base.Channel.DisconnectDevice(deviceId);
        }
        
        public System.Threading.Tasks.Task DisconnectDeviceAsync(string deviceId) {
            return base.Channel.DisconnectDeviceAsync(deviceId);
        }
        
        public void RemoveDevice(string deviceId) {
            base.Channel.RemoveDevice(deviceId);
        }
        
        public System.Threading.Tasks.Task RemoveDeviceAsync(string deviceId) {
            return base.Channel.RemoveDeviceAsync(deviceId);
        }
    }
}
