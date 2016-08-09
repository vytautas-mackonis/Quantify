using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Quantify.Metrics;
using Quantify.Metrics.Sampling;
using Quantify.Metrics.Time;
using Xunit;

namespace Quantify.Tests.Metrics
{
    public class TimerTests
    {
        [Theory]
        [InlineData(new[] { 60, 300, 900 }, new[] { 0.5, 0.75 })]
        [InlineData(new[] { 60, 300, 900 }, new[] { 0.98, 0.99, 0.999 })]
        [InlineData(new[] { 10, 20 }, new[] { 0.5, 0.75 })]
        public void InitialTimerHasZeroValues(int[] movingRateDurations, double[] percentiles)
        {
            var sut = new Timer(Clock.Default, new ExponentiallyDecayingReservoir<long>(), percentiles, movingRateDurations);

            var value = sut.Value;
            Assert.NotNull(value);
            

            var latencies = value.Latencies;
            Assert.NotNull(latencies);
            Assert.Equal(0, latencies.Count);
            Assert.Equal(0, latencies.LastValue);
            Assert.Equal(0, latencies.Max);
            Assert.Equal(0, latencies.Min);
            Assert.Equal(0.0, latencies.Mean);
            Assert.Equal(0.0, latencies.StdDev);

            var expectedPercentiles = percentiles.Select(x => new PercentileValue<long>(x, 0)).ToArray();
            Assert.Equal(expectedPercentiles, latencies.Percentiles);


            var rate = value.Rate;
            Assert.NotNull(rate);
            Assert.Equal(0, rate.Count);
            Assert.Equal(0, rate.MeanRate);

            var expectedRates = movingRateDurations.Select(x => new RateValue(x, 0)).ToArray();
            Assert.Equal(expectedRates, rate.MovingRates);


            var errorRate = value.ErrorRate;
            Assert.NotNull(errorRate);
            Assert.Equal(0, errorRate.Count);
            Assert.Equal(0, errorRate.MeanRate);

            var expectedErrorRates = movingRateDurations.Select(x => new RateValue(x, 0)).ToArray();
            Assert.Equal(expectedErrorRates, errorRate.MovingRates);


            var currentlyExexuting = value.CurrentlyExecuting;
            Assert.NotNull(currentlyExexuting);
            Assert.Equal(0, currentlyExexuting.Count);
        }

        [Theory]
        [InlineData(new[] { 60 }, new[] { 0.5, 0.8, 0.9 })]
        [InlineData(new[] { 60, 100, 240 }, new[] { 0.5, 0.8, 0.9 })]
        [InlineData(new[] { 60, 100, 240 }, new[] { 0.2, 0.57, 0.99 })]
        public void TimingWillUpdateLatencies(int[] timings, double[] percentiles)
        {
            var clock = new FakeClock();
            var sut = new Timer(clock, new ExponentiallyDecayingReservoir<long>(), percentiles, new int[0]);

            foreach (var timing in timings)
            {
                using (sut.StartTiming())
                {
                    clock.AdvanceNanoSeconds(timing);
                }
            }

            var value = sut.Value;
            var latencies = value.Latencies;
            Assert.Equal(timings.Length, latencies.Count);
            Assert.Equal(timings.Last(), latencies.LastValue);
            Assert.Equal(timings.Max(), latencies.Max);
            Assert.Equal(timings.Min(), latencies.Min);

            var expectedMean = timings.Average();
            Assert.InRange(latencies.Mean, expectedMean - 0.001, expectedMean + 0.001);
            Assert.Equal(Statistics.PopulationStandardDeviation(timings.Select(x => (double)x)), latencies.StdDev);

            var expectedPercentiles = percentiles.Select(x => new PercentileValue<long>(x, Statistics.Percentile(timings, x))).ToArray();
            Assert.Equal(expectedPercentiles, latencies.Percentiles);
        }

        [Theory]
        [InlineData(new[] { 60, 300, 900 }, new[] { 10 }, new[] { 0.0, 0.0, 0.0 } )]
        [InlineData(new[] { 60, 300, 900 }, new[] { 10, 30 }, new[] { 0.184, 0.196, 0.198 })]
        [InlineData(new[] { 10, 20, 30 }, new[] { 10, 30 }, new[] { 0.121, 0.155, 0.169 })]
        public void TimingWillUpdateRate(int[] movingRateDurations, int[] secondsBetweenTimes, double[] expectedMovingRates)
        {
            var clock = new FakeClock();
            var sut = new Timer(clock, new ExponentiallyDecayingReservoir<long>(), new double[0], movingRateDurations);

            foreach (var seconds in secondsBetweenTimes)
            {
                using (sut.StartTiming())
                {
                }

                clock.AdvanceSeconds(seconds);
            }

            var value = sut.Value;

            var rate = value.Rate;
            Assert.Equal(secondsBetweenTimes.Length, rate.Count);
            var expectedMeanRate = secondsBetweenTimes.Length/(double) secondsBetweenTimes.Sum();
            Assert.InRange(rate.MeanRate, expectedMeanRate - 0.001, expectedMeanRate + 0.001);

            Assert.Equal(movingRateDurations.Length, rate.MovingRates.Length);
            for (var i = 0; i < movingRateDurations.Length; i++)
            {
                var movingRate = rate.MovingRates[i];
                var expected = expectedMovingRates[i];
                Assert.Equal(movingRateDurations[i], movingRate.WindowSeconds);
                Assert.InRange(movingRate.Rate, expected - 0.001, expected + 0.001);
            }
        }

        [Theory]
        [InlineData(new[] { 60, 300, 900 }, new[] { 10 }, new[] { 0.0, 0.0, 0.0 })]
        [InlineData(new[] { 60, 300, 900 }, new[] { 10, 30 }, new[] { 0.184, 0.196, 0.198 })]
        [InlineData(new[] { 10, 20, 30 }, new[] { 10, 30 }, new[] { 0.121, 0.155, 0.169 })]
        public void TimingWillUpdateErrorRate(int[] movingRateDurations, int[] secondsBetweenTimes, double[] expectedMovingRates)
        {
            var clock = new FakeClock();
            var sut = new Timer(clock, new ExponentiallyDecayingReservoir<long>(), new double[0], movingRateDurations);

            foreach (var seconds in secondsBetweenTimes)
            {
                using (var context = sut.StartTiming())
                {
                    context.MarkError();
                }

                clock.AdvanceSeconds(seconds);
            }

            var value = sut.Value;

            var rate = value.ErrorRate;
            Assert.Equal(secondsBetweenTimes.Length, rate.Count);
            var expectedMeanRate = secondsBetweenTimes.Length / (double)secondsBetweenTimes.Sum();
            Assert.InRange(rate.MeanRate, expectedMeanRate - 0.001, expectedMeanRate + 0.001);

            Assert.Equal(movingRateDurations.Length, rate.MovingRates.Length);
            for (var i = 0; i < movingRateDurations.Length; i++)
            {
                var movingRate = rate.MovingRates[i];
                var expected = expectedMovingRates[i];
                Assert.Equal(movingRateDurations[i], movingRate.WindowSeconds);
                Assert.InRange(movingRate.Rate, expected - 0.001, expected + 0.001);
            }
        }

        [Fact]
        public void ErrorRateWillNotBeUpdatedWhenNotMarked()
        {
            var sut = new Timer(Clock.Default, new ExponentiallyDecayingReservoir<long>(), new double[0], new int[0]);
            using (sut.StartTiming())
            {
            }

            var value = sut.Value;
            Assert.Equal(0, value.ErrorRate.Count);
        }

        [Fact]
        public void ErrorRateWillOnlyBeUpdatedOnce()
        {
            var sut = new Timer(new FakeClock(), new ExponentiallyDecayingReservoir<long>(), new double[0], new int[0]);
            using (var context = sut.StartTiming())
            {
                context.MarkError();
                context.MarkError();
            }

            var value = sut.Value;

            Assert.Equal(1, value.ErrorRate.Count);
        }

        [Fact]
        public void TimingWillUpdateCurrentlyExecutingCounter()
        {
            var sut = new Timer(Clock.Default, new ExponentiallyDecayingReservoir<long>(), new double[0], new int[0]);

            using (sut.StartTiming())
            {
                Assert.Equal(1, sut.Value.CurrentlyExecuting.Count);
                using (sut.StartTiming())
                {
                    Assert.Equal(2, sut.Value.CurrentlyExecuting.Count);
                }
                Assert.Equal(1, sut.Value.CurrentlyExecuting.Count);
            }
            Assert.Equal(0, sut.Value.CurrentlyExecuting.Count);
        }
    }
}
