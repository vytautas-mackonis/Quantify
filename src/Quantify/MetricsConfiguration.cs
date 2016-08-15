using System;
using Quantify.Sampling;

namespace Quantify
{
    internal class MetricsConfiguration
    {
        private static volatile MetricsConfiguration _current = new MetricsConfiguration();

        public static MetricsConfiguration Current => _current;

        public decimal[] Percentiles { get; }
        public int[] RateWindows { get; }
        public ISamplingReservoirFactory ReservoirFactory { get; }
        public IClock Clock { get; }
        public bool IsInitialized => Percentiles != null;

        public MetricsConfiguration(decimal[] percentiles, int[] rateWindows, ISamplingReservoirFactory reservoirFactory, IClock clock)
        {
            if (percentiles == null) throw new ArgumentNullException(nameof(percentiles));
            if (rateWindows == null) throw new ArgumentNullException(nameof(rateWindows));
            if (reservoirFactory == null) throw new ArgumentNullException(nameof(reservoirFactory));
            if (clock == null) throw new ArgumentNullException(nameof(clock));

            Percentiles = percentiles;
            RateWindows = rateWindows;
            ReservoirFactory = reservoirFactory;
            Clock = clock;
        }

        private MetricsConfiguration()
        {
            
        }

        public static void InitializeWith(MetricsConfiguration values)
        {
            _current = values;
        }

        public static void Reset()
        {
            _current = new MetricsConfiguration();
        }
    }
}