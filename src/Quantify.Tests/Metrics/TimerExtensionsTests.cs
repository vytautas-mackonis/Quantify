using System;
using System.Threading.Tasks;
using Quantify.Metrics;
using Quantify.Metrics.Sampling;
using Quantify.Metrics.Time;
using Xunit;

namespace Quantify.Tests.Metrics
{
    public class TimerExtensionsTests
    {
        private readonly Timer _sut = new Timer(Clock.Default, new ExponentiallyDecayingReservoir<long>(), new double[0], new int[0]);

        [Fact]
        public void TimeCallsSuppliedActionWrappedInStartTiming()
        {
            _sut.Time(() =>
            {
                Assert.Equal(1, _sut.Value.CurrentlyExecuting.Count);
            });

            Assert.Equal(0, _sut.Value.CurrentlyExecuting.Count);
            Assert.Equal(1, _sut.Value.Rate.Count);
        }

        [Fact]
        public void TimePropagatesExceptions()
        {
            var exception = new Exception();

            var thrown = Assert.Throws<Exception>(() => _sut.Time(() => { throw exception; }));
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TimeUpdatesTimerCorrectlyOnException()
        {
            Assert.Throws<Exception>(() => _sut.Time(() => { throw new Exception(); }));

            var value = _sut.Value;
            Assert.Equal(1, value.ErrorRate.Count);
            Assert.Equal(1, value.Rate.Count);
        }

        [Fact]
        public async Task TimeAsyncCallsSuppliedActionWrappedInStartTiming()
        {
            await _sut.TimeAsync(() =>
            {
                Assert.Equal(1, _sut.Value.CurrentlyExecuting.Count);
                return Task.CompletedTask;
            });

            Assert.Equal(0, _sut.Value.CurrentlyExecuting.Count);
            Assert.Equal(1, _sut.Value.Rate.Count);
        }

        [Fact]
        public async Task TimeAsyncPropagatesExceptions()
        {
            var exception = new Exception();

            var thrown = await Assert.ThrowsAsync<Exception>(() => _sut.TimeAsync(() =>
            {
                throw exception;
            }));

            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TimeAsyncUpdatesTimerCorrectlyOnException()
        {
            await Assert.ThrowsAsync<Exception>(() => _sut.TimeAsync(() => { throw new Exception(); }));

            var value = _sut.Value;
            Assert.Equal(1, value.ErrorRate.Count);
            Assert.Equal(1, value.Rate.Count);
        }



        [Fact]
        public void TimeForFunctionCallsSuppliedActionWrappedInStartTiming()
        {
            _sut.Time(() =>
            {
                Assert.Equal(1, _sut.Value.CurrentlyExecuting.Count);
                return true;
            });

            Assert.Equal(0, _sut.Value.CurrentlyExecuting.Count);
            Assert.Equal(1, _sut.Value.Rate.Count);
        }

        [Fact]
        public void TimeForFunctionPropagatesExceptions()
        {
            var exception = new Exception();

            var thrown = Assert.Throws<Exception>(() => _sut.Time(() => {
                if (string.Empty.Length == 0) throw exception;
                return true;
            }));
            Assert.Same(exception, thrown);
        }

        [Fact]
        public void TimeForFunctionUpdatesTimerCorrectlyOnException()
        {
            Assert.Throws<Exception>(() => _sut.Time(() =>
            {
                if (string.Empty.Length == 0) throw new Exception();
                return true;
            }));

            var value = _sut.Value;
            Assert.Equal(1, value.ErrorRate.Count);
            Assert.Equal(1, value.Rate.Count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void TimeForFunctionReturnsCorrectResult(int returned)
        {
            var actual = _sut.Time(() => returned);
            Assert.Equal(returned, actual);
        }

        [Fact]
        public async Task TimeForFunctionAsyncCallsSuppliedActionWrappedInStartTiming()
        {
            await _sut.TimeAsync(() =>
            {
                Assert.Equal(1, _sut.Value.CurrentlyExecuting.Count);
                return Task.FromResult(true);
            });

            Assert.Equal(0, _sut.Value.CurrentlyExecuting.Count);
            Assert.Equal(1, _sut.Value.Rate.Count);
        }

        [Fact]
        public async Task TimeForFunctionAsyncPropagatesExceptions()
        {
            var exception = new Exception();

            var thrown = await Assert.ThrowsAsync<Exception>(() => _sut.TimeAsync(() =>
            {
                if (string.Empty.Length == 0) throw exception;
                return Task.FromResult(true);
            }));

            Assert.Same(exception, thrown);
        }

        [Fact]
        public async Task TimeForFunctionAsyncUpdatesTimerCorrectlyOnException()
        {
            await Assert.ThrowsAsync<Exception>(() => _sut.TimeAsync(() =>
            {
                if (string.Empty.Length == 0) throw new Exception();
                return Task.FromResult(true);
            }));

            var value = _sut.Value;
            Assert.Equal(1, value.ErrorRate.Count);
            Assert.Equal(1, value.Rate.Count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task TimeForFunctionAsyncReturnsCorrectResult(int returned)
        {
            var actual = await _sut.TimeAsync(() => Task.FromResult(returned));
            Assert.Equal(returned, actual);
        }
    }
}