using System;
using System.Diagnostics;

namespace Quantify
{
    public interface IClock
    {
        long CurrentTimeNanoseconds();
    }

    public class StopwatchClock : IClock
    {
        private const long UnixEraTicks = 621355968000000000L;

        public long CurrentTimeNanoseconds()
        {
            return (Stopwatch.GetTimestamp() - UnixEraTicks) * 100L;
        }
    }

    public class DateTimeClock : IClock
    {
        private const long UnixEraTicks = 621355968000000000L;

        public long CurrentTimeNanoseconds()
        {
            return (DateTime.UtcNow.Ticks - UnixEraTicks) * 100L;
        }
    }
}