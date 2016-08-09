using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quantify.Metrics
{
    public class Gauge<T> : IMetric
        where T: struct
    {
        private readonly Func<T> _valueAccessor;

        public Gauge(Func<T> valueAccessor)
        {
            _valueAccessor = valueAccessor;
        }

        public void Accept(IMetricVisitor visitor)
        {
            visitor.Visit(new GaugeValue<T>(_valueAccessor()));
        }
    }

    public class GaugeValue<T>
    {
        public T Value { get; }

        public GaugeValue(T value)
        {
            Value = value;
        }
    }
}
