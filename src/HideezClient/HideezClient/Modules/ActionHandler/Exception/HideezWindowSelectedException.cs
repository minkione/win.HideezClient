using System;

namespace HideezClient.Modules.ActionHandler
{
    class HideezWindowSelectedException : Exception
    {
        public HideezWindowSelectedException() : base() { }

        public HideezWindowSelectedException(string message) : base(message) { }
    }
}
