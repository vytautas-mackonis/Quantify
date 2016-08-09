using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quantify.Metrics
{
    public class Gauge<T>
        where T: struct
    {
        private readonly Func<T> _valueAccessor;

        public Gauge(Func<T> valueAccessor)
        {
            _valueAccessor = valueAccessor;
        }

        public GaugeValue<T> Value => new GaugeValue<T>(_valueAccessor());
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
