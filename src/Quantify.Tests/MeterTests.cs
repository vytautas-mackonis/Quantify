﻿using System;
using System.Linq;
using Moq;
using Quantify.Sampling;
using Xunit;

namespace Quantify.Tests
{
    public class MeterTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void InvalidNameThrows(string name)
        {
            Assert.Throws<ArgumentException>(() => new Meter(name, Mock.Of<IClock>(), new int[0]));
        }

        [Fact]
        public void NullClockThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Meter("foo", null, new int[0]));
        }

        [Fact]
        public void NullMovingRatesThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Meter("foo", Mock.Of<IClock>(), null));
        }

        [Theory]
        [InlineData(new[] { 60, 300, 900 })]
        [InlineData(new[] { 10, 15 })]
        public void InitialMeterHasZeroValues(int[] rates)
        {
            var sut = new Meter("foo", new StopwatchClock(), rates);
        
            var value = sut.Value();
        
            Assert.Equal(0, value.Count);
            Assert.Equal(0, value.MeanRate);

            var expectedRates = rates.Select(x => new RateValue(x, 0)).ToArray();
            Assert.Equal(expectedRates, value.MovingRates);
        }
        
        [Theory]
        [InlineData(24)]
        [InlineData(39)]
        public void MeterWithASingleValueHasCorrectCount(int count)
        {
            var sut = new Meter("foo", new StopwatchClock(), new [] { 60 });
            sut.Mark(count);
        
            var value = sut.Value();
        
            Assert.Equal(count, value.Count);
        }
        
        [Theory]
        [InlineData(new[] { 7, 8, 12 }, 27)]
        [InlineData(new[] { 11, 2, 18, 1 }, 32)]
        public void MeterWithMultipleValuesHasCorrectCount(int[] markValues, int expectedCount)
        {
            var sut = new Meter("foo", new StopwatchClock(), new[] { 60 });
            foreach (var markValue in markValues)
            {
                sut.Mark(markValue);
            }
        
            var value = sut.Value();
        
            Assert.Equal(expectedCount, value.Count);
        }
        
        [Theory]
        [InlineData(240, 20, 12.0)]
        [InlineData(240, 30, 8.0)]
        [InlineData(250, 20, 12.5)]
        [InlineData(390, 30, 13.0)]
        public void MeterWithASingleValueHasCorrectMeanRate(int count, int seconds, double mean)
        {
            var clock = new FakeClock();
            var sut = new Meter("foo", clock, new[] { 60 });
            clock.AdvanceSeconds(seconds);
            sut.Mark(count);
        
            var value = sut.Value();
        
            Assert.Equal(mean, value.MeanRate);
        }
        
        [Theory]
        [InlineData(new[] { 8, 12, 4 }, new[] { 2, 3, 3 }, 3.0)]
        [InlineData(new[] { 8, 12, 4 }, new[] { 1, 0, 1 }, 12.0)]
        [InlineData(new[] { 12, 13 }, new[] { 2, 0 }, 12.5)]
        public void MeterWithAMultipleValuesHasCorrectMeanRate(int[] meterValues, int[] seconds, double mean)
        {
            var clock = new FakeClock();
            var sut = new Meter("foo", clock, new[] { 60 });
        
            for (var i = 0; i < meterValues.Length; i++)
            {
                clock.AdvanceSeconds(seconds[i]);
                sut.Mark(meterValues[i]);
            }
        
            var value = sut.Value();
        
            Assert.Equal(mean, value.MeanRate);
        }

        [Fact]
        public void MovingRatesAreUpdated()
        {
            var rateIntervals = new[] {60, 300, 900};
            var clock = new FakeClock();
            var sut = new Meter("foo", clock, rateIntervals);
            sut.Mark();
            clock.AdvanceSeconds(10);
            sut.Mark(2);

            var value = sut.Value();
            Assert.Equal(rateIntervals.Length, value.MovingRates.Length);

            var rates = new[] {0.1840, 0.1966, 0.1988};

            for (var i = 0; i < rateIntervals.Length; i++)
            {
                var rate = value.MovingRates[i];
                Assert.Equal(rateIntervals[i], rate.WindowSeconds);
                Assert.InRange(rate.Rate, rates[i] - 0.001, rates[i] + 0.001);
            }
        }
    }
}
