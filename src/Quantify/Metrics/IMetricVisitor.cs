using System;

namespace Quantify.Metrics
{
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
}