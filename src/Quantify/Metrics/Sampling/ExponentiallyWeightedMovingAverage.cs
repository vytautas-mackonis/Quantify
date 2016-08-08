using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quantify.Metrics.Sampling
{
    public class ExponentiallyWeightedMovingAverage
    {
        private const int INTERVAL = 5;
        private const double SECONDS_PER_MINUTE = 60.0;
        private const int ONE_MINUTE = 1;
        private const int FIVE_MINUTES = 5;
        private const int FIFTEEN_MINUTES = 15;
        private static readonly double M1_ALPHA = 1 - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / ONE_MINUTE);
        private static readonly double M5_ALPHA = 1 - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / FIVE_MINUTES);
        private static readonly double M15_ALPHA = 1 - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / FIFTEEN_MINUTES);

        private volatile bool initialized = false;
        private volatile ValueHolder<double> rate = new ValueHolder<double>(0.0);

        private readonly AtomicLong uncounted = new AtomicLong(0);
        private readonly double alpha, interval;

        /**
         * Creates a new EWMA which is equivalent to the UNIX one minute load average and which expects
         * to be ticked every 5 seconds.
         *
         * @return a one-minute EWMA
         */
        public static ExponentiallyWeightedMovingAverage oneMinuteEWMA()
        {
            return new ExponentiallyWeightedMovingAverage(M1_ALPHA, INTERVAL);
        }

        /**
         * Creates a new EWMA which is equivalent to the UNIX five minute load average and which expects
         * to be ticked every 5 seconds.
         *
         * @return a five-minute EWMA
         */
        public static ExponentiallyWeightedMovingAverage fiveMinuteEWMA()
        {
            return new ExponentiallyWeightedMovingAverage(M5_ALPHA, INTERVAL);
        }

        /**
         * Creates a new EWMA which is equivalent to the UNIX fifteen minute load average and which
         * expects to be ticked every 5 seconds.
         *
         * @return a fifteen-minute EWMA
         */
        public static ExponentiallyWeightedMovingAverage fifteenMinuteEWMA()
        {
            return new ExponentiallyWeightedMovingAverage(M15_ALPHA, INTERVAL);
        }

        /**
         * Create a new EWMA with a specific smoothing constant.
         *
         * @param alpha        the smoothing constant
         * @param interval     the expected tick interval
         * @param intervalUnit the time unit of the tick interval
         */
        public ExponentiallyWeightedMovingAverage(double alpha, long interval)
        {
            this.interval = interval*1000000000L;
            this.alpha = alpha;
        }

        /**
         * Update the moving average with a new value.
         *
         * @param n the new value
         */
        public void update(long n)
        {
            uncounted.Add(n);
        }

        /**
         * Mark the passage of time and decay the current rate accordingly.
         */
        public void tick()
        {
            long count = uncounted.GetAndReset();
            double instantRate = count / interval;
            if (initialized)
            {
                rate = new ValueHolder<double>(rate.Value + (alpha * (instantRate - rate.Value)));
            }
            else
            {
                rate = new ValueHolder<double>(instantRate);
                initialized = true;
            }
        }

        /**
         * Returns the rate in the given units of time.
         *
         * @param rateUnit the unit of time
         * @return the rate
         */
        public double getRate()
        {
            return rate.Value;
        }
    }
}
