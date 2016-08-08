using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quantify.Metrics;
using Xunit;

namespace Quantify.Tests.Metrics
{
    public class CounterTests
    {
        [Fact]
        public void CounterValueIsZeroInitially()
        {
            var sut = new Counter();
            Assert.Equal(0L, sut.Value.Count);
        }

        [Fact]
        public void CounterValueIsOneAfterSingleIncrement()
        {
            var sut = new Counter();
            sut.Increment();
            Assert.Equal(1L, sut.Value.Count);
        }

        [Theory]
        [InlineData(2L)]
        [InlineData(3L)]
        [InlineData(4L)]
        public void CounterValueIsOneAfterMultipleIncrements(long numIncrements)
        {
            var sut = new Counter();

            for (var i = 0; i < numIncrements; i++)
            {
                sut.Increment();
            }

            Assert.Equal(numIncrements, sut.Value.Count);
        }

        [Theory]
        [InlineData(2L)]
        [InlineData(3L)]
        [InlineData(4L)]
        public void CounterValueIsCorrectAfterSingleIncrementWithValue(long value)
        {
            var sut = new Counter();
            sut.Increment(value);
            Assert.Equal(value, sut.Value.Count);
        }

        [Theory]
        [InlineData(2L, 5, 10L)]
        [InlineData(3L, 4, 12L)]
        public void CounterValueIsCorrectAfterMultipleIncrementsWithValue(long increment, int numIncrements, long expectedResult)
        {
            var sut = new Counter();

            for (var i = 0; i < numIncrements; i++)
            {
                sut.Increment(increment);
            }

            Assert.Equal(expectedResult, sut.Value.Count);
        }

        [Fact]
        public void CounterValueIsCorrectlyDecrementedOnce()
        {
            var sut = new Counter();
            sut.Increment();
            sut.Decrement();
            Assert.Equal(0L, sut.Value.Count);
        }

        [Theory]
        [InlineData(3, 2, 1L)]
        [InlineData(15, 8, 7L)]
        public void CounterValueIsCorrectlyDecrementedMultipleTimes(int counterInitialValue, int timesDecremented, long expectedValue)
        {
            var sut = new Counter();
            sut.Increment(counterInitialValue);

            for (var i = 0; i < timesDecremented; i++)
            {
                sut.Decrement();
            }

            Assert.Equal(expectedValue, sut.Value.Count);
        }

        [Theory]
        [InlineData(3, 2, 1L)]
        [InlineData(15, 8, 7L)]
        public void CounterValueIsCorrectlyDecrementedByValueOnce(int initialValue, int decrement, long expected)
        {
            var sut = new Counter();
            sut.Increment(initialValue);
            sut.Decrement(decrement);
            Assert.Equal(expected, sut.Value.Count);
        }

        [Theory]
        [InlineData(11, 2, 5, 1L)]
        [InlineData(23, 8, 2, 7L)]
        public void CounterValueIsCorrectlyDecrementedByValueMultipleTimes(int counterInitialValue, int timesDecremented, int decrementValue, long expectedValue)
        {
            var sut = new Counter();
            sut.Increment(counterInitialValue);

            for (var i = 0; i < timesDecremented; i++)
            {
                sut.Decrement(decrementValue);
            }

            Assert.Equal(expectedValue, sut.Value.Count);
        }
    }
}
