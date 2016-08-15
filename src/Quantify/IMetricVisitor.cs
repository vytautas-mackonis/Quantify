﻿using System;
using System.Threading.Tasks;

namespace Quantify
{
    public interface IMetricVisitor
    {
        Task VisitAsync(string name, CounterValue metric);
        Task VisitAsync<T>(string name, GaugeValue<T> metric)
            where T: struct;
        Task VisitAsync<T>(string name, HistogramValue<T> metric)
            where T : struct, IComparable;
        Task VisitAsync(string name, MeterValue metric);
        Task VisitAsync(string name, TimerValue metric);
    }
}