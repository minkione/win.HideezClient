﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HideezServiceHost.HideezServiceReference {
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
        private HideezServiceHost.HideezServiceReference.ClientType ClientTypeField;
        
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
        public HideezServiceHost.HideezServiceReference.ClientType ClientType {
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
    [System.Runtime.Serialization.DataContractAttribute(Name="BleDeviceDTO", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
    [System.SerializableAttribute()]
    public partial class BleDeviceDTO : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
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
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="HideezServiceReference.IHideezService", CallbackContract=typeof(HideezServiceHost.HideezServiceReference.IHideezServiceCallback), SessionMode=System.ServiceModel.SessionMode.Required)]
    public interface IHideezService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/AttachClient", ReplyAction="http://tempuri.org/IHideezService/AttachClientResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezServiceHost.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/AttachClientHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        bool AttachClient(HideezServiceHost.HideezServiceReference.ServiceClientParameters parameters);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/AttachClient", ReplyAction="http://tempuri.org/IHideezService/AttachClientResponse")]
        System.Threading.Tasks.Task<bool> AttachClientAsync(HideezServiceHost.HideezServiceReference.ServiceClientParameters parameters);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DetachClient", ReplyAction="http://tempuri.org/IHideezService/DetachClientResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezServiceHost.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/DetachClientHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void DetachClient();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/DetachClient", ReplyAction="http://tempuri.org/IHideezService/DetachClientResponse")]
        System.Threading.Tasks.Task DetachClientAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Ping", ReplyAction="http://tempuri.org/IHideezService/PingResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezServiceHost.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/PingHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        int Ping();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Ping", ReplyAction="http://tempuri.org/IHideezService/PingResponse")]
        System.Threading.Tasks.Task<int> PingAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Shutdown", ReplyAction="http://tempuri.org/IHideezService/ShutdownResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezServiceHost.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/ShutdownHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        void Shutdown();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/Shutdown", ReplyAction="http://tempuri.org/IHideezService/ShutdownResponse")]
        System.Threading.Tasks.Task ShutdownAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetAdapterState", ReplyAction="http://tempuri.org/IHideezService/GetAdapterStateResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezServiceHost.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/GetAdapterStateHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        bool GetAdapterState(HideezServiceHost.HideezServiceReference.Adapter addapter);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetAdapterState", ReplyAction="http://tempuri.org/IHideezService/GetAdapterStateResponse")]
        System.Threading.Tasks.Task<bool> GetAdapterStateAsync(HideezServiceHost.HideezServiceReference.Adapter addapter);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetPairedDevices", ReplyAction="http://tempuri.org/IHideezService/GetPairedDevicesResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(HideezServiceHost.HideezServiceReference.HideezServiceFault), Action="http://tempuri.org/IHideezService/GetPairedDevicesHideezServiceFaultFault", Name="HideezServiceFault", Namespace="http://schemas.datacontract.org/2004/07/ServiceLibrary")]
        HideezServiceHost.HideezServiceReference.BleDeviceDTO[] GetPairedDevices();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IHideezService/GetPairedDevices", ReplyAction="http://tempuri.org/IHideezService/GetPairedDevicesResponse")]
        System.Threading.Tasks.Task<HideezServiceHost.HideezServiceReference.BleDeviceDTO[]> GetPairedDevicesAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IHideezServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/LockWorkstationRequest")]
        void LockWorkstationRequest();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/HESConnectionStateChanged")]
        void HESConnectionStateChanged(bool isConnected);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/RFIDConnectionStateChanged")]
        void RFIDConnectionStateChanged(bool isConnected);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/DongleConnectionStateChanged")]
        void DongleConnectionStateChanged(bool isConnected);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/PairedDevicesCollectionChanged")]
        void PairedDevicesCollectionChanged(HideezServiceHost.HideezServiceReference.BleDeviceDTO[] devices);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IHideezService/PairedDevicePropertyChanged")]
        void PairedDevicePropertyChanged(HideezServiceHost.HideezServiceReference.BleDeviceDTO device);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IHideezServiceChannel : HideezServiceHost.HideezServiceReference.IHideezService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class HideezServiceClient : System.ServiceModel.DuplexClientBase<HideezServiceHost.HideezServiceReference.IHideezService>, HideezServiceHost.HideezServiceReference.IHideezService {
        
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
        
        public bool AttachClient(HideezServiceHost.HideezServiceReference.ServiceClientParameters parameters) {
            return base.Channel.AttachClient(parameters);
        }
        
        public System.Threading.Tasks.Task<bool> AttachClientAsync(HideezServiceHost.HideezServiceReference.ServiceClientParameters parameters) {
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
        
        public bool GetAdapterState(HideezServiceHost.HideezServiceReference.Adapter addapter) {
            return base.Channel.GetAdapterState(addapter);
        }
        
        public System.Threading.Tasks.Task<bool> GetAdapterStateAsync(HideezServiceHost.HideezServiceReference.Adapter addapter) {
            return base.Channel.GetAdapterStateAsync(addapter);
        }
        
        public HideezServiceHost.HideezServiceReference.BleDeviceDTO[] GetPairedDevices() {
            return base.Channel.GetPairedDevices();
        }
        
        public System.Threading.Tasks.Task<HideezServiceHost.HideezServiceReference.BleDeviceDTO[]> GetPairedDevicesAsync() {
            return base.Channel.GetPairedDevicesAsync();
        }
    }
}
