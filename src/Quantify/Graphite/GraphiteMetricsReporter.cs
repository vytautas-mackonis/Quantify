using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Quantify.Graphite
{
    public class GraphiteMetricsReporter : IMetricsReporter, IMetricVisitor
    {
        private const long NanosecondsInSecond = 1000000000L;
        private readonly GraphiteConnection _connection;
        private string _timestamp;

        public GraphiteMetricsReporter(string hostname, int port)
        {
            _connection = new GraphiteConnection(hostname, port);
        }

        public async Task Report(IClock clock, IEnumerable<IMetric> metrics)
        {
            _timestamp = (clock.CurrentTimeNanoseconds()/NanosecondsInSecond).ToString(CultureInfo.InvariantCulture);

            foreach (var metric in metrics)
            {
                await metric.AcceptAsync(this);
            }

            await _connection.FlushAsync();
        }

        private async Task SendAsync(string name, string value)
        {
            var sb = new StringBuilder();
            sb.Append(name)
                .Append(' ')
                .Append(value)
                .Append(' ')
                .Append(_timestamp);

            await _connection.SendAsync(sb.ToString());
        }

        private async Task SendAsync(string name, string suffix, string value)
        {
            var sb = new StringBuilder();
            sb.Append(name)
                .Append(suffix)
                .Append(' ')
                .Append(value)
                .Append(' ')
                .Append(_timestamp);

            await _connection.SendAsync(sb.ToString());
        }

        async Task IMetricVisitor.VisitAsync(string name, CounterValue metric)
        {
            await SendAsync(name, metric.Count.ToString(CultureInfo.InvariantCulture));
        }

        async Task IMetricVisitor.VisitAsync<T>(string name, GaugeValue<T> metric)
        {
            await SendAsync(name, metric.Value.ToString(CultureInfo.InvariantCulture));
        }

        async Task IMetricVisitor.VisitAsync<T>(string name, HistogramValue<T> metric)
        {
            await SendAsync(name, ".count", metric.Count.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".last", metric.LastValue.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".min", metric.Min.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".max", metric.Max.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".mean", metric.Mean.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".stdDev", metric.StdDev.ToString(CultureInfo.InvariantCulture));

            foreach (var percentileValue in metric.Percentiles)
            {
                await SendAsync(name, ".p" + percentileValue.Quantile.ToString("0.00###########################", CultureInfo.InvariantCulture).Substring(2), percentileValue.Percentile.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static string FormatRate(int windowSeconds)
        {
            if (windowSeconds%60 == 0)
                return (windowSeconds/60).ToString("\\.rate-#-min", CultureInfo.InvariantCulture);

            return windowSeconds.ToString("\\.rate-#-sec", CultureInfo.InvariantCulture);
        }

        private static string FormatErrorRate(int windowSeconds)
        {
            if (windowSeconds % 60 == 0)
                return (windowSeconds / 60).ToString("\\.error-rate-#-min", CultureInfo.InvariantCulture);

            return windowSeconds.ToString("\\.error-rate-#-sec", CultureInfo.InvariantCulture);
        }

        async Task IMetricVisitor.VisitAsync(string name, MeterValue metric)
        {
            await SendAsync(name, ".total", metric.Count.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".rate-mean", metric.MeanRate.ToString(CultureInfo.InvariantCulture));

            foreach (var rate in metric.MovingRates)
            {
                await SendAsync(name, FormatRate(rate.WindowSeconds), rate.Rate.ToString(CultureInfo.InvariantCulture));
            }
        }

        async Task IMetricVisitor.VisitAsync(string name, TimerValue metric)
        {
            await SendAsync(name, ".count", metric.Latencies.Count.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".activeSessions", metric.CurrentlyExecuting.Count.ToString(CultureInfo.InvariantCulture));

            await SendAsync(name, ".success-count", metric.Rate.Count.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".rate-mean", metric.Rate.MeanRate.ToString(CultureInfo.InvariantCulture));

            foreach (var rate in metric.Rate.MovingRates)
            {
                await SendAsync(name, FormatRate(rate.WindowSeconds), rate.Rate.ToString(CultureInfo.InvariantCulture));
            }

            await SendAsync(name, ".error-count", metric.ErrorRate.Count.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".error-rate-mean", metric.ErrorRate.MeanRate.ToString(CultureInfo.InvariantCulture));

            foreach (var rate in metric.ErrorRate.MovingRates)
            {
                await SendAsync(name, FormatErrorRate(rate.WindowSeconds), rate.Rate.ToString(CultureInfo.InvariantCulture));
            }

            await SendAsync(name, ".duration-last", metric.Latencies.LastValue.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".duration-min", metric.Latencies.Min.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".duration-max", metric.Latencies.Max.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".duration-mean", metric.Latencies.Mean.ToString(CultureInfo.InvariantCulture));
            await SendAsync(name, ".duration-stdDev", metric.Latencies.StdDev.ToString(CultureInfo.InvariantCulture));

            foreach (var percentileValue in metric.Latencies.Percentiles)
            {
                await SendAsync(name, ".duration-p" + percentileValue.Quantile.ToString("0.00###########################", CultureInfo.InvariantCulture).Substring(2), percentileValue.Percentile.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}