using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quantify.Metrics.Sampling;
using Quantify.Metrics.Time;

namespace Quantify.Metrics
{
    public class Timer
    {
        private readonly IClock _clock;
        private readonly Meter _rateMeter;
        private readonly Meter _errorRateMeter;
        private readonly Histogram<long> _latencyHistogram;
        private readonly Counter _currentlyExecutingCounter;

        public Timer(IClock clock, IReservoir<long> reservoir, double[] percentiles, int[] movingRateWindowSeconds)
        {
            _clock = clock;
            _rateMeter = new Meter(clock, movingRateWindowSeconds);
            _errorRateMeter = new Meter(clock, movingRateWindowSeconds);
            _latencyHistogram = new Histogram<long>(reservoir, percentiles);
            _currentlyExecutingCounter = new Counter();
        }

        public IContext StartTiming()
        {
            return new Context(this);
        }

        public TimerValue Value
            =>
            new TimerValue(_rateMeter.Value, _errorRateMeter.Value, _latencyHistogram.Value,
                _currentlyExecutingCounter.Value);

        public interface IContext : IDisposable
        {
            void MarkError();
        }

        private class Context : IContext
        {
            private readonly Timer _timer;
            private readonly long _startTime;
            private bool _error = false;

            public Context(Timer timer)
            {
                _timer = timer;
                _startTime = timer._clock.CurrentTimeNanoseconds();
                timer._currentlyExecutingCounter.Increment();
            }

            public void Dispose()
            {
                var time = _timer._clock.CurrentTimeNanoseconds() - _startTime;
                _timer._latencyHistogram.Mark(time);
                _timer._rateMeter.Mark();

                if (_error)
                    _timer._errorRateMeter.Mark();
                _timer._currentlyExecutingCounter.Decrement();
            }

            public void MarkError()
            {
                _error = true;
            }
        }
    }

    public static class TimerExtensions
    {
        public static void Time(this Timer timer, Action action)
        {
            var context = timer.StartTiming();
            try
            {
                action();
            }
            catch (Exception)
            {
                context.MarkError();
                throw;
            }
            finally
            {
                context.Dispose();
            }
        }
        
        public static async Task TimeAsync(this Timer timer, Func<Task> action)
        {
            var context = timer.StartTiming();
            try
            {
                await action();
            }
            catch (Exception)
            {
                context.MarkError();
                throw;
            }
            finally
            {
                context.Dispose();
            }
        }
        
        public static T Time<T>(this Timer timer, Func<T> action)
        {
            var context = timer.StartTiming();
            try
            {
                return action();
            }
            catch (Exception)
            {
                context.MarkError();
                throw;
            }
            finally
            {
                context.Dispose();
            }
        }
        
        public static async Task<T> TimeAsync<T>(this Timer timer, Func<Task<T>> action)
        {
            var context = timer.StartTiming();
            try
            {
                return await action();
            }
            catch (Exception)
            {
                context.MarkError();
                throw;
            }
            finally
            {
                context.Dispose();
            }
        }
    }

    public class TimerValue
    {
        public MeterValue Rate { get; }
        public MeterValue ErrorRate { get; }
        public HistogramValue<long> Latencies { get; }
        public CounterValue CurrentlyExecuting { get; }

        public TimerValue(MeterValue rate, MeterValue errorRate, HistogramValue<long> latencies, CounterValue currentlyExecuting)
        {
            Rate = rate;
            ErrorRate = errorRate;
            Latencies = latencies;
            CurrentlyExecuting = currentlyExecuting;
        }
    }
}
