﻿using System.Collections.Generic;
using System.ServiceModel;

namespace ServiceLibrary
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICallbacks))]
    public interface IHideezService
    {
        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        bool AttachClient(ServiceClientParameters parameters);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void DetachClient();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        int Ping();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void Shutdown();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        bool GetAdapterState(Adapter addapter);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        BleDeviceDTO[] GetPairedDevices();
    }

    public interface ICallbacks
    {
        [OperationContract(IsOneWay = true)]
        void LockWorkstationRequest();

        [OperationContract(IsOneWay = true)]
        void HESConnectionStateChanged(bool isConnected);

        [OperationContract(IsOneWay = true)]
        void RFIDConnectionStateChanged(bool isConnected);

        [OperationContract(IsOneWay = true)]
        void DongleConnectionStateChanged(bool isConnected);

        [OperationContract(IsOneWay = true)]
        void PairedDevicesCollectionChanged(BleDeviceDTO[] devices);

        [OperationContract(IsOneWay = true)]
        void PairedDevicePropertyChanged(BleDeviceDTO device);
    }

    public enum Adapter
    {
        HES,
        RFID,
        Dongle,
    }
}
