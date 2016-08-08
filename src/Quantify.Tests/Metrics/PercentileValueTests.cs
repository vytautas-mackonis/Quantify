using Quantify.Metrics;
using Xunit;

namespace Quantify.Tests.Metrics
{
    public abstract class PercentileValueTests<T>
        where T : struct
    {
        [Theory]
        [InlineData(0.5, 0.54)]
        [InlineData(0.7, 0.1)]
        public void DifferentQuantilesDoNotEqual(double quantile1, double quantile2)
        {
            var left = new PercentileValue<T>(quantile1, default(T));
            var right = new PercentileValue<T>(quantile2, default(T));

            Assert.NotEqual(left, right);
            Assert.NotEqual(right, left);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(1, 2)]
        public void DifferentPercentilesDoNotEqual(int index1, int index2)
        {
            var left = new PercentileValue<T>(1, ExampleValues[index1]);
            var right = new PercentileValue<T>(1, ExampleValues[index2]);

            Assert.NotEqual(left, right);
            Assert.NotEqual(right, left);
        }

        [Theory]
        [InlineData(0, 0.5)]
        [InlineData(1, 0.8)]
        public void SameQuantilesAndPercentilesEqual(int index, double quantile)
        {
            var left = new PercentileValue<T>(quantile, ExampleValues[index]);
            var right = new PercentileValue<T>(quantile, ExampleValues[index]);

            Assert.Equal(left, right);
            Assert.Equal(right, left);
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
        }

        protected abstract T[] ExampleValues { get; }
    }

    public class IntPercentileValueTests : PercentileValueTests<int>
    {
        protected override int[] ExampleValues => new[] { 1, 25, 14 };
    }

    public class DoublePercentileValueTests : PercentileValueTests<double>
    {
        protected override double[] ExampleValues => new[] { 1.8, 25.4, 14.7 };
    }
}