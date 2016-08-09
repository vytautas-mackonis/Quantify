using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quantify.Metrics
{
    public class Counter : IMetric
    {
        private long _value = 0;

        public void Increment()
        {
            Interlocked.Increment(ref _value);
        }

        public void Increment(long value)
        {
            Interlocked.Add(ref _value, value);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref _value);
        }

        public void Decrement(long value)
        {
            Interlocked.Add(ref _value, -value);
        }

        public void Accept(IMetricVisitor visitor)
        {
            visitor.Visit(new CounterValue(Volatile.Read(ref _value)));
        }
    }

    public class CounterValue
    {
        public long Count { get; }

        public CounterValue(long count)
        {
            Count = count;
        }
    }

    public interface IMetric
    {
        void Accept(IMetricVisitor visitor);
    }

    public interface IMetricVisitor
    {
        void Visit(CounterValue metric);
        void Visit<T>(GaugeValue<T> metric)
            where T: struct;
        void Visit<T>(HistogramValue<T> metric)
            where T : struct, IComparable;
        void Visit(MeterValue metric);
        void Visit(TimerValue metric);
    }

    public static class MetricExtensions
    {
        private class CapturingCounterVisitor : IMetricVisitor
        {
            public CounterValue Counter { get; private set; }

            public void Visit(CounterValue metric)
            {
                Counter = metric;
            }

            public void Visit<U>(GaugeValue<U> metric) where U : struct
            {
            }

            public void Visit<U>(HistogramValue<U> metric) where U : struct, IComparable
            {
            }

            public void Visit(MeterValue metric)
            { 
            }

            public void Visit(TimerValue metric)
            {
            }
        }

        private class CapturingGaugeVisitor<T> : IMetricVisitor
            where T : struct
        {
            public GaugeValue<T> Gauge { get; private set; }

            public void Visit(CounterValue metric)
            {
            }

            public void Visit<U>(GaugeValue<U> metric) where U : struct
            {
                if (typeof(T) == typeof(U))
                    Gauge = (GaugeValue<T>)(object)metric;
            }

            public void Visit<U>(HistogramValue<U> metric) where U : struct, IComparable
            {
            }

            public void Visit(MeterValue metric)
            {
            }

            public void Visit(TimerValue metric)
            {
            }
        }

        private class CapturingHistogramVisitor<T> : IMetricVisitor
            where T : struct, IComparable
        {
            public HistogramValue<T> Histogram { get; private set; }

            public void Visit(CounterValue metric)
            {
            }

            public void Visit<U>(GaugeValue<U> metric) where U : struct
            {
            }

            public void Visit<U>(HistogramValue<U> metric) where U : struct, IComparable
            {
                if (typeof(T) == typeof(U))
                    Histogram = (HistogramValue<T>)(object)metric;
            }

            public void Visit(MeterValue metric)
            {
            }

            public void Visit(TimerValue metric)
            {
            }
        }

        private class CapturingMeterVisitor : IMetricVisitor
        {
            public MeterValue Meter { get; private set; }

            public void Visit(CounterValue metric)
            {
            }

            public void Visit<U>(GaugeValue<U> metric) where U : struct
            {
            }

            public void Visit<U>(HistogramValue<U> metric) where U : struct, IComparable
            {
            }

            public void Visit(MeterValue metric)
            {
                Meter = metric;
            }

            public void Visit(TimerValue metric)
            {
            }
        }

        public class CapturingTimerVisitor : IMetricVisitor
        {
            public TimerValue Timer { get; private set; }

            public void Visit(CounterValue metric)
            {
            }

            public void Visit<U>(GaugeValue<U> metric) where U : struct
            {
            }

            public void Visit<U>(HistogramValue<U> metric) where U : struct, IComparable
            {
            }

            public void Visit(MeterValue metric)
            {
            }

            public void Visit(TimerValue metric)
            {
                Timer = metric;
            }
        }

        public static CounterValue Value(this Counter metric)
        {
            var visitor = new CapturingCounterVisitor();
            metric.Accept(visitor);
            return visitor.Counter;
        }

        public static GaugeValue<T> Value<T>(this Gauge<T> metric)
            where T: struct 
        {
            var visitor = new CapturingGaugeVisitor<T>();
            metric.Accept(visitor);
            return visitor.Gauge;
        }

        public static HistogramValue<T> Value<T>(this Histogram<T> metric)
            where T : struct, IComparable
        {
            var visitor = new CapturingHistogramVisitor<T>();
            metric.Accept(visitor);
            return visitor.Histogram;
        }

        public static MeterValue Value(this Meter metric)
        {
            var visitor = new CapturingMeterVisitor();
            metric.Accept(visitor);
            return visitor.Meter;
        }

        public static TimerValue Value(this Timer metric)
        {
            var visitor = new CapturingTimerVisitor();
            metric.Accept(visitor);
            return visitor.Timer;
        }
    }
}
