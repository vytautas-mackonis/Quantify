using System;
using System.Collections.Generic;

namespace Quantify
{
    internal class ReportScheduler : IDisposable
    {
        private readonly List<PeriodicWorker> _workers = new List<PeriodicWorker>();
        private readonly List<IMetricsReporter> _reporters = new List<IMetricsReporter>();

        public void Schedule(IMetricsReporter reporter, int periodMilliseconds)
        {
            _reporters.Add(reporter);
            _workers.Add(new PeriodicWorker(periodMilliseconds, periodMilliseconds, () => reporter.Report(MetricsConfiguration.Current.Clock, MetricsRegistry.ListMetrics())));
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

            foreach (var metricsReporter in _reporters)
            {
                metricsReporter.Dispose();
            }
        }
    }
}