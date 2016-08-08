using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quantify.Metrics;
using Xunit;

namespace Quantify.Tests.Metrics
{
    public abstract class GaugeTests<T>
        where T: struct 
    {
        private readonly Gauge<T> _sut = new Gauge<T>();

        [Fact]
        public void InitialGaugeValueIsZero()
        {
            Assert.Equal(default(T), _sut.Value.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GaugeHoldsValueThatIsSet(int valueIndex)
        {
            var value = ExampleValues[valueIndex];
            _sut.Set(value);
            Assert.Equal(value, _sut.Value.Value);
        }

        protected abstract T[] ExampleValues { get; }
    }

    public class IntGaugeTests : GaugeTests<int>
    {
        protected override int[] ExampleValues => new[] {1, 9, 12};
    }

    public class DecimalGaugeTests : GaugeTests<decimal>
    {
        protected override decimal[] ExampleValues => new[] {1.2m, 157.13m, -0.4m};
    }
}
