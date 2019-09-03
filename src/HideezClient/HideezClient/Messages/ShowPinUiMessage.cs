namespace HideezClient.Messages
{
    class ShowPinUiMessage
    {
        public string DeviceId { get; set; }

        public bool ConfirmPin { get; set; }

        public bool OldPin { get; set; }

        public ShowPinUiMessage(string deviceId, bool withConfirm, bool askOldPin)
        {
            DeviceId = deviceId;
            ConfirmPin = withConfirm;
            OldPin = askOldPin;
        }
    }
}
