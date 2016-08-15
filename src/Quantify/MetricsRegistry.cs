using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quantify.Sampling;

namespace Quantify
{
    public class MetricsRegistry
    {
        private static readonly ConcurrentDictionary<string, IMetric> Metrics = new ConcurrentDictionary<string, IMetric>();

        private static T CreateMetric<T>(string name, Func<string, T> factory)
            where T: IMetric
        {
            if (!MetricsConfiguration.Current.IsInitialized)
                throw new MetricsConfigurationException("Must configure Quantify before creating any metrics. Please call Metrics.Configure() first, finishing with Run().");
            return (T)Metrics.GetOrAdd(name, n => factory(n));
        }

        public static Counter Counter(string name)
        {
            return CreateMetric(name, n => new Counter(n));
        }

        public static Gauge<int> Gauge(string name, Func<int> valueProvider)
        {
            return CreateMetric(name, n => new Gauge<int>(n, valueProvider));
        }

        public static Gauge<long> Gauge(string name, Func<long> valueProvider)
        {
            return CreateMetric(name, n => new Gauge<long>(n, valueProvider));
        }

        public static Gauge<double> Gauge(string name, Func<double> valueProvider)
        {
            return CreateMetric(name, n => new Gauge<double>(n, valueProvider));
        }

        public static Gauge<decimal> Gauge(string name, Func<decimal> valueProvider)
        {
            return CreateMetric(name, n => new Gauge<decimal>(n, valueProvider));
        }

        private static Histogram<T> CreateHistogram<T>(string name)
            where T : struct, IComparable
        {

            return CreateMetric(name, n => new Histogram<T>(
                    n,
                    MetricsConfiguration.Current.ReservoirFactory.Create<T>(MetricsConfiguration.Current.Clock),
                    MetricsConfiguration.Current.Percentiles
                )
            );
        }

        public static Histogram<long> LongHistogram(string name)
        {
            return CreateHistogram<long>(name);
        }

        public static Histogram<double> DoubleHistogram(string name)
        {
            return CreateHistogram<double>(name);
        }

        public static Meter Meter(string name)
        {
            return CreateMetric(name, n => new Meter(n, MetricsConfiguration.Current.Clock, MetricsConfiguration.Current.RateWindows));
        }

        public static Timer Timer(string name)
        {
            return CreateMetric(name, n => new Timer(
                    n,
                    MetricsConfiguration.Current.Clock,
                    MetricsConfiguration.Current.ReservoirFactory.Create<long>(MetricsConfiguration.Current.Clock),
                    MetricsConfiguration.Current.Percentiles,
                    MetricsConfiguration.Current.RateWindows
                )
            );
        }

        public static IEnumerable<IMetric> ListMetrics()
        {
            return Metrics.OrderBy(x => x.Key).Select(x => x.Value);
        }

        internal static void Reset()
        {
            Metrics.Clear();
        }
    }

    public class MetricsConfigurationException : Exception
    {
        public MetricsConfigurationException(string message) : base(message)
        {
        }
    }
}
