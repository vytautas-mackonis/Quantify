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
        private volatile GaugeValue<T> _currentValue = new GaugeValue<T>(default(T));

        public void Set(T value)
        {
            _currentValue = new GaugeValue<T>(value);
        }

        public GaugeValue<T> Value => _currentValue;
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
