using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Quantify.Graphite;
using Quantify.Sampling;
using Xunit;

namespace Quantify.IntegrationTests.Graphite
{
    public class GraphiteMetricsReporterTests : IDisposable
    {
        private readonly GraphiteMock _graphite;
        private readonly int _port;

        public GraphiteMetricsReporterTests()
        {
            _port = 48121;
            _graphite = new GraphiteMock(_port);
            _graphite.Start();
        }

        [Theory]
        [InlineData("foo", 1, 3)]
        [InlineData("bar", 2, 85)]
        public async Task ReportsCounter(string name, int value, int time)
        {
            var metric = new Counter(name);
            metric.Increment(value);

            using (var sut = new GraphiteMetricsReporter("localhost", _port))
                await sut.Report(new FakeClock(() => time), new[] { metric });

            var expected = $"{name} {value} {time}\n";
            await AssertEventually(() => _graphite.ReceivedReports, new[] { expected });
        }

        [Theory] 
        [InlineData("foo", 1, 3)]
        [InlineData("bar", 2, 85)]
        public async Task ReportsGauge(string name, int value, int time)
        {
            var gauge = new Gauge<int>(name, () => value);

            using (var sut = new GraphiteMetricsReporter("localhost", _port))
                await sut.Report(new FakeClock(() => time), new[] {gauge});

            var expected = $"{name} {value} {time}\n";
            await AssertEventually(() => _graphite.ReceivedReports, new[] {expected});
        }

        [Theory]
        [InlineData("foo", new[] { "0.5", "0.78" }, new[] { "50", "78" }, 3)]
        [InlineData("bar", new[] { "0.1", "0.9", "0.999" }, new[] { "10", "90", "999" }, 85)]
        public async Task ReportsHistogram(string name, string[] quantiles, string[] quantileNames, int time)
        {
            var metric = new Histogram<long>(name, new ExponentiallyDecayingReservoir<long>(new StopwatchClock()), quantiles.Select(decimal.Parse).ToArray());

            using (var sut = new GraphiteMetricsReporter("localhost", _port))
                await sut.Report(new FakeClock(() => time), new[] { metric });

            var expected = new List<string>
            {
                $"{name}.count 0 {time}",
                $"{name}.last 0 {time}",
                $"{name}.min 0 {time}",
                $"{name}.max 0 {time}",
                $"{name}.mean 0 {time}",
                $"{name}.stdDev 0 {time}",
            };

            foreach (var quantile in quantileNames)
            {
                expected.Add($"{name}.p{quantile} 0 {time}");
            }

            await AssertEventually(() => _graphite.ReceivedReports, new[] { string.Join("\n", expected) + "\n" });
        }

        [Theory]
        [InlineData("foo", new[] { 60, 300, 900 }, new[] { "1-min", "5-min", "15-min" }, 3)]
        [InlineData("bar", new[] { 20, 120 }, new[] { "20-sec", "2-min" }, 85)]
        public async Task ReportsMeter(string name, int[] rateWindows, string[] windowNames, int time)
        {
            var metric = new Meter(name, new StopwatchClock(), rateWindows);

            using (var sut = new GraphiteMetricsReporter("localhost", _port))
                await sut.Report(new FakeClock(() => time), new[] { metric });

            var expected = new List<string>
            {
                $"{name}.total 0 {time}",
                $"{name}.rate-mean 0 {time}"
            };

            foreach (var window in windowNames)
            {
                expected.Add($"{name}.rate-{window} 0 {time}");
            }

            await AssertEventually(() => _graphite.ReceivedReports, new[] { string.Join("\n", expected) + "\n" });
        }

        [Theory]
        [InlineData("foo", new[] { "0.5", "0.78" }, new[] { "50", "78" }, new[] { 60, 300, 900 }, new[] { "1-min", "5-min", "15-min" }, 3)]
        [InlineData("bar", new[] { "0.1", "0.9", "0.999" }, new[] { "10", "90", "999" }, new[] { 20, 120 }, new[] { "20-sec", "2-min" }, 85)]
        public async Task ReportsTimer(string name, string[] quantiles, string[] quantileNames, int[] rateWindows, string[] windowNames, int time)
        {
            var metric = new Timer(
                name, 
                new StopwatchClock(), 
                new ExponentiallyDecayingReservoir<long>(new StopwatchClock()),
                quantiles.Select(decimal.Parse).ToArray(),
                rateWindows
                );

            using (var sut = new GraphiteMetricsReporter("localhost", _port))
                await sut.Report(new FakeClock(() => time), new[] { metric });

            var expected = new List<string>
            {
                $"{name}.count 0 {time}",
                $"{name}.activeSessions 0 {time}",
                $"{name}.success-count 0 {time}",
                $"{name}.rate-mean 0 {time}",
            };

            foreach (var window in windowNames)
            {
                expected.Add($"{name}.rate-{window} 0 {time}");
            }

            expected.Add($"{name}.error-count 0 {time}");
            expected.Add($"{name}.error-rate-mean 0 {time}");

            foreach (var window in windowNames)
            {
                expected.Add($"{name}.error-rate-{window} 0 {time}");
            }

            expected.Add($"{name}.duration-last 0 {time}");
            expected.Add($"{name}.duration-min 0 {time}");
            expected.Add($"{name}.duration-max 0 {time}");
            expected.Add($"{name}.duration-mean 0 {time}");
            expected.Add($"{name}.duration-stdDev 0 {time}");

            foreach (var quantile in quantileNames)
            {
                expected.Add($"{name}.duration-p{quantile} 0 {time}");
            }

            await AssertEventually(() => _graphite.ReceivedReports, new[] { string.Join("\n", expected) + "\n" });
        }

        [Fact]
        public async Task ReportsMultipleMetrics()
        {
            var counter = new Counter("foo");
            counter.Increment(2);

            var gauge = new Gauge<double>("bar", () => 2.79129);

            using (var sut = new GraphiteMetricsReporter("localhost", _port))
                await sut.Report(new FakeClock(() => 45), new IMetric[] { counter, gauge });

            var expected = $"foo 2 45\nbar 2.79129 45\n";
            await AssertEventually(() => _graphite.ReceivedReports, new[] { expected });
        }

        private async Task AssertEventually(Func<IEnumerable<string>> accessor, IEnumerable<string> expectedValue)
        {
            for (var i = 0; i < 5; i++)
            {
                if (accessor().SequenceEqual(expectedValue))
                    return;

                await Task.Delay(50);
            }

            Assert.Equal(expectedValue, accessor());
        }

        public void Dispose()
        {
            _graphite.Dispose();
        }
    }

    public class FakeClock : IClock
    {
        private readonly Func<long> _value;

        public FakeClock(Func<long> value)
        {
            _value = value;
        }

        public long CurrentTimeNanoseconds()
        {
            return _value()*1000000000L;
        }
    }

    public class GraphiteMock : IDisposable
    {
        private readonly TcpListener _listener;
        private volatile bool _listening;

        public readonly ConcurrentQueue<string> ReceivedReports = new ConcurrentQueue<string>();

        public GraphiteMock(int port)
        {
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
        }

        public void Start()
        {
            _listener.Start();
            _listening = true;
            Task.Run(Listen);
        }

        private async Task HandleClient(TcpClient client)
        {
            using (var reader = new StreamReader(client.GetStream()))
            {
                var line = await reader.ReadToEndAsync();
                ReceivedReports.Enqueue(line);
            }
                
        }

        private async Task Listen()
        {
            while (_listening)
            {
                var client = await _listener.AcceptTcpClientAsync();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                HandleClient(client);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public void Dispose()
        {
            _listening = false;
            _listener.Stop();
        }
    }
}
