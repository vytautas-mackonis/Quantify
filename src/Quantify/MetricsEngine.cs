using System;

namespace Quantify
{
    internal class MetricsEngine : IDisposable
    {
        private readonly ReportScheduler _scheduler;
        private readonly MetricsConfiguration _configuration;

        public MetricsEngine(ReportScheduler scheduler, MetricsConfiguration configuration)
        {
            _scheduler = scheduler;
            _configuration = configuration;
        }

        public IDisposable Start()
        {
            MetricsConfiguration.InitializeWith(_configuration);
            _scheduler.Start();
            return this;
        }

        public void Dispose()
        {
            _scheduler.Dispose();
            MetricsConfiguration.Reset();
            MetricsRegistry.Reset();
        }
    }
}