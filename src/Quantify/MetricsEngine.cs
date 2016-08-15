using System;

namespace Quantify
{
    internal class MetricsEngine : IDisposable
    {
        private readonly ReportScheduler _scheduler;

        public MetricsEngine(ReportScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Dispose()
        {
            _scheduler.Dispose();
            MetricsConfiguration.Reset();
            MetricsRegistry.Reset();
        }
    }
}