using System;
using System.Threading;
using System.Threading.Tasks;

namespace McMaster.NETCore.Plugins.Internal
{
    internal class Debouncer : IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly TimeSpan _waitTime;
        private int _counter;

        public Debouncer(TimeSpan waitTime)
        {
            _waitTime = waitTime;
        }

        public void Execute(Action action)
        {
            var current = Interlocked.Increment(ref _counter);

            Task.Delay(_waitTime).ContinueWith(task =>
            {
                // Is this the last task that was queued?
                if (current == _counter && !_cts.IsCancellationRequested)
                {
                    action();
                }

                task.Dispose();
            }, _cts.Token);
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
