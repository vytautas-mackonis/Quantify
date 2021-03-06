﻿using System;
using Xunit;

namespace Quantify.Tests
{
    public abstract class GaugeTests<T>
        where T: struct, IConvertible
    {
        private int _valueIndex = 0;
        private readonly Gauge<T> _sut;

        public GaugeTests()
        {
             _sut = new Gauge<T>("foo", () =>
             {
                 var result = ExampleValues[_valueIndex];
                 _valueIndex++;
                 return result;
             });
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void InvalidNameThrows(string name)
        {
            Assert.Throws<ArgumentException>(() => new Gauge<T>(name, () => default(T)));
        }

        [Fact]
        public void NullValueAccessorThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Gauge<T>("foo", null));
        }

        [Fact]
        public void GaugeReturnsValueFromSuppliedFunction()
        {
            Assert.Equal(ExampleValues[0], _sut.Value().Value);
        }

        [Fact]
        public void GaugeQueriesSuppliedFunctionEveryTime()
        {
            var unused = _sut.Value();
            Assert.Equal(ExampleValues[1], _sut.Value().Value);
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
