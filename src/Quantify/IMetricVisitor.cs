using System;
using System.Threading.Tasks;

namespace Quantify
{
    public interface IMetricVisitor
    {
        Task VisitAsync(string name, CounterValue metric);
        Task VisitAsync<T>(string name, GaugeValue<T> metric)
            where T: struct, IConvertible;
        Task VisitAsync<T>(string name, HistogramValue<T> metric)
            where T : struct, IComparable, IConvertible;
        Task VisitAsync(string name, MeterValue metric);
        Task VisitAsync(string name, TimerValue metric);
    }
}