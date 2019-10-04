using System;
using System.ComponentModel;
using System.Windows;

namespace HideezClient.Mvvm
{
    class BindingRaiseevent : IWeakEventListener
    {
        private readonly string path;

        public BindingRaiseevent(object source, string path)
        {
            this.path = path;
            if (source is INotifyPropertyChanged notifyPropertyChanged)
            {
                PropertyChangedEventManager.AddListener(notifyPropertyChanged, this, path);
            }
        }

        public virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            var value = sender.GetType().GetProperty(path)?.GetValue(sender);
            if (value != null)
            {
                ValueChanged?.Invoke(value);
            }

            return true;
        }

        public event Action<object> ValueChanged;
    }
}
