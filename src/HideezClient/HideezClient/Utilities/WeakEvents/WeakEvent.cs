using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Utilities
{
    internal static class WeakEventHandler
    {
        public static EventHandler Create<THandler>(
            THandler handler, Action<THandler, object, EventArgs> invoker)
            where THandler : class
        {
            var weakEventHandler = new WeakReference<THandler>(handler);

            return (sender, args) =>
            {
                THandler thandler;
                if (weakEventHandler.TryGetTarget(out thandler))
                {
                    invoker(thandler, sender, args);
                }
            };
        }
    }

    internal static class WeakEventHandler<TArgs>
    {
        public static EventHandler<TArgs> Create<THandler>(
            THandler handler, Action<THandler, object, TArgs> invoker)
            where THandler : class
        {
            var weakEventHandler = new WeakReference<THandler>(handler);

            return (sender, args) =>
            {
                THandler thandler;
                if (weakEventHandler.TryGetTarget(out thandler))
                {
                    invoker(thandler, sender, args);
                }
            };
        }
    }
}
