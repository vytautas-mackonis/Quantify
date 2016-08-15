﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Quantify.IntegrationTests
{
    public class SchedulingTests
    {
        [Theory]
        [InlineData(100, 2)]
        [InlineData(100, 3)]
        [InlineData(200, 2)]
        public void MetricReportingIsScheduledAtInterval(int periodMilliseconds, int timesShouldBeCalled)
        {
            var reporter = new CapturingMetricsReporter();

            using (Metrics.Configure()
                .ReportUsing(reporter, periodMilliseconds)
                .Run())
            {
                Thread.Sleep((periodMilliseconds + 20) * timesShouldBeCalled);
                Assert.Equal(timesShouldBeCalled, reporter.Snapshots.Count);
            }
        }

        [Fact]
        public void MultipleReportersAreSupported()
        {
            var reporter1 = new CapturingMetricsReporter();
            var reporter2 = new CapturingMetricsReporter();

            using (Metrics.Configure()
                .ReportUsing(reporter1, 100)
                .ReportUsing(reporter2, 200)
                .Run())
            {
                Thread.Sleep(250);
                Assert.Equal(2, reporter1.Snapshots.Count);
                Assert.Equal(1, reporter2.Snapshots.Count);
            }
        }

        [Fact]
        public void FreshMetricsRegistrySnapshotIsSuppliedToReporterEachTime()
        {
            var reporter = new CapturingMetricsReporter();

            using (Metrics.Configure()
                .ReportUsing(reporter, 200)
                .Run())
            {
                Thread.Sleep(250);
                var counter = MetricsRegistry.Counter("foo");
                Thread.Sleep(200);


                var snapshots = reporter.Snapshots.ToArray();
                Assert.Equal(0, snapshots[0].Length);
                Assert.Equal(1, snapshots[1].Length);
                Assert.Equal(counter, snapshots[1][0]);
            }
        }

        [Fact]
        public void SchedulingDoesNotContinueAfterMetricsDispose()
        {
            var reporter = new CapturingMetricsReporter();

            using (Metrics.Configure()
                .ReportUsing(reporter, 100)
                .Run())
            {
                Thread.Sleep(120);
            }

            Thread.Sleep(200);

            Assert.Equal(1, reporter.Snapshots.Count);
        }

        [Fact]
        public void NextScheduledTaskDoesNotStartUntilPreviousIsFinished()
        {
            var reporter = new LongRunningCapturingMetricsReporter(100);

            using (Metrics.Configure()
                .ReportUsing(reporter, 100)
                .Run())
            {
                Thread.Sleep(220);
                Assert.Equal(1, reporter.Snapshots.Count);
            }
        }
    }

    public class CapturingMetricsReporter : IReporter
    {
        public readonly ConcurrentQueue<IMetric[]> Snapshots = new ConcurrentQueue<IMetric[]>();

        public Task Report(IEnumerable<IMetric> metrics)
        {
            Snapshots.Enqueue(metrics.ToArray());
            return Task.CompletedTask;
        }
    }

    public class LongRunningCapturingMetricsReporter : IReporter
    {
        public readonly ConcurrentQueue<IMetric[]> Snapshots = new ConcurrentQueue<IMetric[]>();
        private readonly int _sleepMilliseconds;

        public LongRunningCapturingMetricsReporter(int sleepMilliseconds)
        {
            _sleepMilliseconds = sleepMilliseconds;
        }

        public async Task Report(IEnumerable<IMetric> metrics)
        {
            Snapshots.Enqueue(metrics.ToArray());
            await Task.Delay(_sleepMilliseconds).ConfigureAwait(false);
        }
    }
}
