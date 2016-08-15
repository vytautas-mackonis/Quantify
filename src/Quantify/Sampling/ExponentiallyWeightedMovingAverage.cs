using System;

namespace Quantify.Sampling
{
    internal class ExponentiallyWeightedMovingAverage
    {
        private const int Interval = 5;

        private volatile bool _initialized = false;
        private volatile ValueHolder<double> _rate = new ValueHolder<double>(0.0);

        private readonly AtomicLong _uncounted = new AtomicLong(0);
        private readonly double _alpha, _interval;

        private static double AlphaFor(int windowSeconds, int tickIntervalSeconds)
        {
            return 1 - Math.Exp(-tickIntervalSeconds / (double)windowSeconds);
        }

        public ExponentiallyWeightedMovingAverage(int windowIntervalSeconds)
            : this(windowIntervalSeconds, Interval)
        {

        }

        public ExponentiallyWeightedMovingAverage(int windowIntervalSeconds, int tickIntervalSeconds)
            : this(AlphaFor(windowIntervalSeconds, tickIntervalSeconds), tickIntervalSeconds)
        {

        }

        public ExponentiallyWeightedMovingAverage(double alpha, int tickIntervalSeconds)
        {
            this._interval = tickIntervalSeconds*1000000000L;
            this._alpha = alpha;
        }

        public void Update(long n)
        {
            _uncounted.Add(n);
        }

        public void Tick()
        {
            long count = _uncounted.GetAndReset();
            double instantRate = count / _interval;
            if (_initialized)
            {
                _rate = new ValueHolder<double>(_rate.Value + (_alpha * (instantRate - _rate.Value)));
            }
            else
            {
                _rate = new ValueHolder<double>(instantRate);
                _initialized = true;
            }
        }

        public double GetRate()
        {
            return _rate.Value;
        }
    }
}
