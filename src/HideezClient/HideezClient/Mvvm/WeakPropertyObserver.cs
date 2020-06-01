using System;
using System.ComponentModel;
using System.Windows;

namespace HideezClient.Mvvm
{
    class WeakPropertyObserver : IWeakEventListener
    {
        private readonly string path;

        /// <summary>
        /// For all property set to path string.Empty.
        /// </summary>
        public WeakPropertyObserver(object source, string path)
        {
            this.path = path;
            if (source is INotifyPropertyChanged notifyPropertyChanged)
            {
                PropertyChangedEventManager.AddListener(notifyPropertyChanged, this, path);
            }
        }

        public virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                string propertyName = (e as PropertyChangedEventArgs)?.PropertyName;

                if (!string.IsNullOrEmpty(propertyName))
                {
                    var value = sender.GetType().GetProperty(path)?.GetValue(sender);
                    if (ValueChanged != null)
                    {
                        ValueChanged.Invoke(propertyName, value);
                    }
                }
                return true;
            }

            return false;
        }

        public event Action<string, object> ValueChanged;
    }
}
