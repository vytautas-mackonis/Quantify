﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Quantify.Sampling;
using Xunit;

namespace Quantify.Tests
{
    public abstract class HistogramTests<T>
        where T: struct, IComparable, IConvertible
    {
        private static IReservoir<T> CreateReservoir(ReservoirType type)
        {
            switch (type)
            {
                case ReservoirType.ExponentiallyDecaying:
                    return new ExponentiallyDecayingReservoir<T>(new StopwatchClock());
                case ReservoirType.SlidingTimeWindow:
                    return new SlidingTimeWindowReservoir<T>(1000000000L * 60 * 5, new StopwatchClock());
                case ReservoirType.SlidingWindow:
                    return new SlidingWindowReservoir<T>(1000);
                case ReservoirType.Uniform:
                    return new UniformReservoir<T>(1000);
                default:
                    throw new ArgumentException("Unknown reservoir type");
            }
        }

        private Histogram<T> CreateHistogram(ReservoirType reservoirType, double[] percentiles)
        {
            return new Histogram<T>("foo", CreateReservoir(reservoirType), percentiles.Select(x => (decimal)x).ToArray());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void InvalidNameThrows(string name)
        {
            Assert.Throws<ArgumentException>(() => new Histogram<T>(name, Mock.Of<IReservoir<T>>(), new decimal[0]));
        }

        [Fact]
        public void NullReservoirThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Histogram<T>("foo", null, new decimal[0]));
        }

        [Fact]
        public void NullPercentilesThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Histogram<T>("foo", Mock.Of<IReservoir<T>>(), null));
        }

        [Theory]
        [InlineData(ReservoirType.ExponentiallyDecaying, new[] { 0.5, 0.75 })]
        [InlineData(ReservoirType.ExponentiallyDecaying, new[] { 0.98, 0.99, 0.999 })]
        [InlineData(ReservoirType.SlidingTimeWindow, new[] { 0.5, 0.75 })]
        [InlineData(ReservoirType.SlidingTimeWindow, new[] { 0.98, 0.99, 0.999 })]
        [InlineData(ReservoirType.SlidingWindow, new[] { 0.5, 0.75 })]
        [InlineData(ReservoirType.SlidingWindow, new[] { 0.98, 0.99, 0.999 })]
        [InlineData(ReservoirType.Uniform, new[] { 0.5, 0.75 })]
        [InlineData(ReservoirType.Uniform, new[] { 0.98, 0.99, 0.999 })]
        public void InitialHistogramHasZeroValues(ReservoirType reservoirType, double[] percentiles)
        {
            var sut = CreateHistogram(reservoirType, percentiles);

            var value = sut.Value();
            Assert.Equal(0, value.Count);
            Assert.Equal(default(T), value.LastValue);
            Assert.Equal(default(T), value.Max);
            Assert.Equal(default(T), value.Min);
            Assert.Equal(0.0, value.Mean);
            Assert.Equal(0.0, value.StdDev);

            var expectedPercentiles = percentiles.Select(x => new PercentileValue<T>((decimal)x, default(T))).ToArray();
            Assert.Equal(expectedPercentiles, value.Percentiles);
        }

        [Theory]
        [InlineData(ReservoirType.ExponentiallyDecaying, new[] { 0.5, 0.75 }, 0)]
        [InlineData(ReservoirType.ExponentiallyDecaying, new[] { 0.98, 0.99, 0.999 }, 1)]
        [InlineData(ReservoirType.SlidingTimeWindow, new[] { 0.5, 0.75 }, 0)]
        [InlineData(ReservoirType.SlidingTimeWindow, new[] { 0.98, 0.99, 0.999 }, 1)]
        [InlineData(ReservoirType.SlidingWindow, new[] { 0.5, 0.75 }, 0)]
        [InlineData(ReservoirType.SlidingWindow, new[] { 0.98, 0.99, 0.999 }, 1)]
        [InlineData(ReservoirType.Uniform, new[] { 0.5, 0.75 }, 0)]
        [InlineData(ReservoirType.Uniform, new[] { 0.98, 0.99, 0.999 }, 1)]
        public void OneValueHistogramHasCorrectValues(ReservoirType reservoirType, double[] percentiles, int sampleIndex)
        {
            var sut = CreateHistogram(reservoirType, percentiles);
            
            var sample = ExampleValues[sampleIndex];
            sut.Mark(sample);

            var value = sut.Value();
            Assert.Equal(1, value.Count);
            Assert.Equal(sample, value.LastValue);
            Assert.Equal(sample, value.Max);
            Assert.Equal(sample, value.Min);
            Assert.Equal(ToDouble(sample), value.Mean);
            Assert.Equal(0, value.StdDev);

            var expectedPercentiles = percentiles.Select(x => new PercentileValue<T>((decimal)x, sample)).ToArray();
            Assert.Equal(expectedPercentiles, value.Percentiles);
        }

        [Theory]
        [InlineData(ReservoirType.ExponentiallyDecaying, StandardDeviationAlgorithm.Population, new[] { 0.8 })]
        [InlineData(ReservoirType.ExponentiallyDecaying, StandardDeviationAlgorithm.Population, new[] { 0.4, 0.5, 0.9 })]
        [InlineData(ReservoirType.SlidingTimeWindow, StandardDeviationAlgorithm.Sample, new[] { 0.5, 0.7 })]
        [InlineData(ReservoirType.SlidingTimeWindow, StandardDeviationAlgorithm.Sample, new[] { 0.4, 0.5, 0.9 })]
        [InlineData(ReservoirType.SlidingWindow, StandardDeviationAlgorithm.Sample, new[] { 0.5, 0.7 })]
        [InlineData(ReservoirType.SlidingWindow, StandardDeviationAlgorithm.Sample, new[] { 0.4, 0.5, 0.9 })]
        [InlineData(ReservoirType.Uniform, StandardDeviationAlgorithm.Sample, new[] { 0.5, 0.7 })]
        [InlineData(ReservoirType.Uniform, StandardDeviationAlgorithm.Sample, new[] { 0.4, 0.5, 0.9 })]
        public void TenValueHistogramHasCorrectValues(ReservoirType reservoirType, StandardDeviationAlgorithm stdDevAlgorithm, double[] percentiles)
        {
            var sut = CreateHistogram(reservoirType, percentiles);

            var samples = ExampleValues.Take(10).ToArray();
            foreach (var sample in samples)
            {
                sut.Mark(sample);
            }

            var value = sut.Value();
            Assert.Equal(10, value.Count);
            Assert.Equal(samples[samples.Length - 1], value.LastValue);
            Assert.Equal(samples.Max(), value.Max);
            Assert.Equal(samples.Min(), value.Min);
            var expectedMean = samples.Select(ToDouble).Average();
            Assert.InRange(value.Mean, expectedMean - 0.001, expectedMean + 0.001);

            var expectedStdDev = StdDev(stdDevAlgorithm, samples);
            var precision = 0.000000000001;
            Assert.InRange(value.StdDev, expectedStdDev - precision, expectedStdDev + precision);

            var expectedPercentiles = percentiles.Select(x => new PercentileValue<T>((decimal)x, Statistics.Percentile(samples, x))).ToArray();
            Assert.Equal(expectedPercentiles, value.Percentiles);
        }

        private static double ToDouble(T value)
        {
            return (double)Convert.ChangeType(value, typeof(double));
        }

        protected abstract T[] ExampleValues { get; }

        private double StdDev(StandardDeviationAlgorithm algorithm, IEnumerable<T> values)
        {
            var doubleValues = values.Select(ToDouble);
            switch (algorithm)
            {
                case StandardDeviationAlgorithm.Population:
                    return Statistics.PopulationStandardDeviation(doubleValues);
                case StandardDeviationAlgorithm.Sample:
                    return Statistics.SampleStandardDeviation(doubleValues);
                default:
                    throw new ArgumentException("Unknown StdDev algorithm");
            }
        }
    }

    public class IntHistogramTests : HistogramTests<int>
    {
        protected override int[] ExampleValues => new[]
        {
            16,
            32,
            23,
            78,
            12,
            48,
            57,
            54,
            1,
            18,
            15
        };
    }


    public class DoubleHistogramTests : HistogramTests<double>
    {
        protected override double[] ExampleValues => new[]
        {
            16.1,
            32.2,
            23.6,
            78.7,
            12.6,
            32.3,
            57.4,
            53.3,
            1.1,
            18.1,
            15.5
        };
    }

    public enum StandardDeviationAlgorithm
    {
        Sample,
        Population
    }

    public enum ReservoirType
    {
        ExponentiallyDecaying,
        SlidingTimeWindow,
        SlidingWindow,
        Uniform
    }
}
