using System;
using System.ComponentModel;

namespace HideezClient.Mvvm
{
    class BindingRaiseevent
    {
        private readonly string path;

        public BindingRaiseevent(object source, string path)
        {
            this.path = path;
            if(source is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += PropertyChanged_PropertyChanged;
            }
        }

        private void PropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == path)
            {
                var value = sender.GetType().GetProperty(path)?.GetValue(sender);
                if(value != null)
                {
                    ValueChanged?.Invoke(value);
                }
            }
        }

        public event Action<object> ValueChanged;
    }
}
