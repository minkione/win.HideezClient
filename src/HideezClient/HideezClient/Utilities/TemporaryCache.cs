using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Utilities
{
    /// <summary>
    /// Cache that is only available for the limited amount of time.
    /// When cache lifetime is elapsed, the value is cleared next time its accessed.
    /// </summary>
    class TemporaryCache<T> where T : class
    {
        private T value;
        private TimeSpan timeout;
        private DateTime lastModificationTime;

        public T Value
        {
            get
            {
                if (value != null && ((DateTime.Now - lastModificationTime) > timeout))
                    value = null;

                return value;
            }
            set
            {
                lastModificationTime = DateTime.Now;
                this.value = value;
            }
        }

        public TemporaryCache(TimeSpan cacheTimeout)
        {
            timeout = cacheTimeout;
        }

        public TemporaryCache(TimeSpan cacheTimeout, T value)
            : this(cacheTimeout)
        {
            this.value = value;
        }

        public void Clear()
        {
            Value = null;
        }
    }
}