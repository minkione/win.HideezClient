namespace HideezClient.Messages
{
    class ShowPinUiMessage
    {
        public string DeviceId { get; }

        public bool ConfirmPin { get; }

        public bool OldPin { get; }

        public ShowPinUiMessage(string deviceId, bool withConfirm, bool askOldPin)
        {
            DeviceId = deviceId;
            ConfirmPin = withConfirm;
            OldPin = askOldPin;
        }
    }
}
