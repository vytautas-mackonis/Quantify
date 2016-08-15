using Xunit;

namespace Quantify.Tests
{
    public class RateValueTests
    {
        [Theory]
        [InlineData(3, 1)]
        [InlineData(1, 2)]
        [InlineData(3, 2)]
        public void DifferentIntervalsDoNotEqual(int interval1, int interval2)
        {
            var left = new RateValue(interval1, 1);
            var right = new RateValue(interval2, 1);

            Assert.NotEqual(left, right);
            Assert.NotEqual(right, left);
        }

        [Theory]
        [InlineData(0.5, 0.54)]
        [InlineData(0.7, 0.1)]
        public void DifferentRatesDoNotEqual(double rate1, double rate2)
        {
            var left = new RateValue(1, rate1);
            var right = new RateValue(1, rate2);

            Assert.NotEqual(left, right);
            Assert.NotEqual(right, left);
        }

        [Theory]
        [InlineData(1, 0.5)]
        [InlineData(2, 0.8)]
        public void SameQuantilesAndPercentilesEqual(int interval, double rate)
        {
            var left = new RateValue(interval, rate);
            var right = new RateValue(interval, rate);

            Assert.Equal(left, right);
            Assert.Equal(right, left);
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
        }
    }
}