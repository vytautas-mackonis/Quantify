﻿using System;

namespace Quantify.Metrics
{
    public static class MetricExtensions
    {
        private class CapturingCounterVisitor : IMetricVisitor
        {
            public CounterValue Counter { get; private set; }

            public void Visit(string name, CounterValue metric)
            {
                Counter = metric;
            }

            public void Visit<U>(string name, GaugeValue<U> metric) where U : struct
            {
            }

            public void Visit<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
            }

            public void Visit(string name, MeterValue metric)
            { 
            }

            public void Visit(string name, TimerValue metric)
            {
            }
        }

        private class CapturingGaugeVisitor<T> : IMetricVisitor
            where T : struct
        {
            public GaugeValue<T> Gauge { get; private set; }

            public void Visit(string name, CounterValue metric)
            {
            }

            public void Visit<U>(string name, GaugeValue<U> metric) where U : struct
            {
                if (typeof(T) == typeof(U))
                    Gauge = (GaugeValue<T>)(object)metric;
            }

            public void Visit<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
            }

            public void Visit(string name, MeterValue metric)
            {
            }

            public void Visit(string name, TimerValue metric)
            {
            }
        }

        private class CapturingHistogramVisitor<T> : IMetricVisitor
            where T : struct, IComparable
        {
            public HistogramValue<T> Histogram { get; private set; }

            public void Visit(string name, CounterValue metric)
            {
            }

            public void Visit<U>(string name, GaugeValue<U> metric) where U : struct
            {
            }

            public void Visit<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
                if (typeof(T) == typeof(U))
                    Histogram = (HistogramValue<T>)(object)metric;
            }

            public void Visit(string name, MeterValue metric)
            {
            }

            public void Visit(string name, TimerValue metric)
            {
            }
        }

        private class CapturingMeterVisitor : IMetricVisitor
        {
            public MeterValue Meter { get; private set; }

            public void Visit(string name, CounterValue metric)
            {
            }

            public void Visit<U>(string name, GaugeValue<U> metric) where U : struct
            {
            }

            public void Visit<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
            }

            public void Visit(string name, MeterValue metric)
            {
                Meter = metric;
            }

            public void Visit(string name, TimerValue metric)
            {
            }
        }

        private class CapturingTimerVisitor : IMetricVisitor
        {
            public TimerValue Timer { get; private set; }

            public void Visit(string name, CounterValue metric)
            {
            }

            public void Visit<U>(string name, GaugeValue<U> metric) where U : struct
            {
            }

            public void Visit<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
            }

            public void Visit(string name, MeterValue metric)
            {
            }

            public void Visit(string name, TimerValue metric)
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

        private class NameCapturingMetricVisitor : IMetricVisitor
        {
            public string Name;

            public void Visit(string name, CounterValue metric)
            {
                Name = name;
            }

            public void Visit<U>(string name, GaugeValue<U> metric) where U : struct
            {
                Name = name;
            }

            public void Visit<U>(string name, HistogramValue<U> metric) where U : struct, IComparable
            {
                Name = name;
            }

            public void Visit(string name, MeterValue metric)
            {
                Name = name;
            }

            public void Visit(string name, TimerValue metric)
            {
                Name = name;
            }
        }

        public static string Name(this IMetric metric)
        {
            var visitor = new NameCapturingMetricVisitor();
            metric.Accept(visitor);
            return visitor.Name;
        }
    }
}