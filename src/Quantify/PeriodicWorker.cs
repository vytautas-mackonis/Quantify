using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quantify.Logging;

namespace Quantify
{
    internal class PeriodicWorker : IDisposable
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(PeriodicWorker));

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly int _delay;
        private readonly int _period;
        private readonly Func<Task> _work;

        public PeriodicWorker(int delay, int period, Func<Task> work)
        {
            _delay = delay;
            _period = period;
            _work = work;
        }

        public Task Start()
        {
            return Task.Run(Run, _cancellationTokenSource.Token);
        }

        private async Task Run()
        {
            if (_delay > 0)
            {
                await Task.Delay(_delay, _cancellationTokenSource.Token);
            }

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await _work().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error while executing periodic report: {ex}");
                }

                await Task.Delay(_period, _cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
