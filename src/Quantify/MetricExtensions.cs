using System;
using System.Threading.Tasks;

namespace Quantify
{
    public static class MetricExtensions
    {
        private class CapturingCounterVisitor : IMetricVisitor
        {
            public CounterValue Counter { get; private set; }

            public Task VisitAsync(string name, CounterValue metric)
            {
                Counter = metric;
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, GaugeValue<U> metric) where U : struct
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, MeterValue metric)
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, TimerValue metric)
            {
                return Task.CompletedTask;
            }
        }

        private class CapturingGaugeVisitor<T> : IMetricVisitor
            where T : struct
        {
            public GaugeValue<T> Gauge { get; private set; }

            public Task VisitAsync(string name, CounterValue metric)
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, GaugeValue<U> metric) where U : struct
            {
                if (typeof(T) == typeof(U))
                    Gauge = (GaugeValue<T>)(object)metric;
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, MeterValue metric)
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, TimerValue metric)
            {
                return Task.CompletedTask;
            }
        }

        private class CapturingHistogramVisitor<T> : IMetricVisitor
            where T : struct, IComparable
        {
            public HistogramValue<T> Histogram { get; private set; }

            public Task VisitAsync(string name, CounterValue metric)
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, GaugeValue<U> metric) where U : struct
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
                if (typeof(T) == typeof(U))
                    Histogram = (HistogramValue<T>)(object)metric;
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, MeterValue metric)
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, TimerValue metric)
            {
                return Task.CompletedTask;
            }
        }

        private class CapturingMeterVisitor : IMetricVisitor
        {
            public MeterValue Meter { get; private set; }

            public Task VisitAsync(string name, CounterValue metric)
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, GaugeValue<U> metric) where U : struct
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, MeterValue metric)
            {
                Meter = metric;
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, TimerValue metric)
            {
                return Task.CompletedTask;
            }
        }

        private class CapturingTimerVisitor : IMetricVisitor
        {
            public TimerValue Timer { get; private set; }

            public Task VisitAsync(string name, CounterValue metric)
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, GaugeValue<U> metric) where U : struct
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, MeterValue metric)
            {
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, TimerValue metric)
            {
                Timer = metric;
                return Task.CompletedTask;
            }
        }

        public static CounterValue Value(this Counter metric)
        {
            var visitor = new CapturingCounterVisitor();
            metric.AcceptAsync(visitor);
            return visitor.Counter;
        }

        public static GaugeValue<T> Value<T>(this Gauge<T> metric)
            where T: struct 
        {
            var visitor = new CapturingGaugeVisitor<T>();
            metric.AcceptAsync(visitor);
            return visitor.Gauge;
        }

        public static HistogramValue<T> Value<T>(this Histogram<T> metric)
            where T : struct, IComparable
        {
            var visitor = new CapturingHistogramVisitor<T>();
            metric.AcceptAsync(visitor);
            return visitor.Histogram;
        }

        public static MeterValue Value(this Meter metric)
        {
            var visitor = new CapturingMeterVisitor();
            metric.AcceptAsync(visitor);
            return visitor.Meter;
        }

        public static TimerValue Value(this Timer metric)
        {
            var visitor = new CapturingTimerVisitor();
            metric.AcceptAsync(visitor);
            return visitor.Timer;
        }

        private class NameCapturingMetricVisitor : IMetricVisitor
        {
            public string Name;

            public Task VisitAsync(string name, CounterValue metric)
            {
                Name = name;
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, GaugeValue<U> metric) where U : struct
            {
                Name = name;
                return Task.CompletedTask;
            }

            public Task VisitAsync<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
                Name = name;
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, MeterValue metric)
            {
                Name = name;
                return Task.CompletedTask;
            }

            public Task VisitAsync(string name, TimerValue metric)
            {
                Name = name;
                return Task.CompletedTask;
            }
        }

        public static string Name(this IMetric metric)
        {
            var visitor = new NameCapturingMetricVisitor();
            metric.AcceptAsync(visitor);
            return visitor.Name;
        }
    }
}