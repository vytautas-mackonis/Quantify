using System;

namespace Quantify
{
    public interface IMetricVisitor
    {
        void Visit(string name, CounterValue metric);
        void Visit<T>(string name, GaugeValue<T> metric)
            where T: struct;
        void Visit<T>(string name, HistogramValue<T> metric)
            where T : struct, IComparable;
        void Visit(string name, MeterValue metric);
        void Visit(string name, TimerValue metric);
    }
}