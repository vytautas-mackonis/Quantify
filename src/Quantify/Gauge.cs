using System;
using System.Threading.Tasks;

namespace Quantify
{
    public class Gauge<T> : IMetric
        where T: struct, IConvertible
    {
        private readonly string _name;
        private readonly Func<T> _valueAccessor;

        public Gauge(string name, Func<T> valueAccessor)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(name)} must be non-empty.");

            if (valueAccessor == null)
                throw new ArgumentNullException(nameof(valueAccessor));

            _name = name;
            _valueAccessor = valueAccessor;
        }

        public async Task AcceptAsync(IMetricVisitor visitor)
        {
            await visitor.VisitAsync(_name, new GaugeValue<T>(_valueAccessor()));
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
