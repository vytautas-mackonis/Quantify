using System;
using System.Collections.Generic;

namespace Quantify
{
    internal class ReportScheduler : IDisposable
    {
        private readonly List<PeriodicWorker> _workers = new List<PeriodicWorker>();

        public void Schedule(IReporter reporter, int periodMilliseconds)
        {
            _workers.Add(new PeriodicWorker(periodMilliseconds, periodMilliseconds, () => reporter.Report(MetricsRegistry.ListMetrics())));
        }

        public void Start()
        {
            foreach (var periodicWorker in _workers)
            {
                periodicWorker.Start();
            }
        }

        public void Dispose()
        {
            foreach (var periodicWorker in _workers)
            {
                periodicWorker.Dispose();
            }
        }
    }
}