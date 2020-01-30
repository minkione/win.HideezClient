using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.Threading
{
    public class SemaphoreQueue
    {
        readonly SemaphoreSlim _semaphore;
        readonly ConcurrentQueue<TaskCompletionSource<bool>> _queue = new ConcurrentQueue<TaskCompletionSource<bool>>();

        public SemaphoreQueue(int initialCount)
        {
            _semaphore = new SemaphoreSlim(initialCount);
        }
        public SemaphoreQueue(int initialCount, int maxCount)
        {
            _semaphore = new SemaphoreSlim(initialCount, maxCount);
        }

        public void Wait()
        {
            WaitAsync().Wait();
        }

        public Task WaitAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            _queue.Enqueue(tcs);
            _semaphore.WaitAsync().ContinueWith(t =>
            {
                if (_queue.TryDequeue(out TaskCompletionSource<bool> popped))
                    popped.SetResult(true);
            });
            return tcs.Task;
        }

        public void Release()
        {
            _semaphore.Release();
        }
    }
}
