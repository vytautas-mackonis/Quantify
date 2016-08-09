using System;
using System.Diagnostics;

namespace Quantify.Metrics.Time
{
    public interface IClock
    {
        long CurrentTimeNanoseconds();
    }

    public static class Clock
    {
        public static readonly IClock Default = new StopwatchClock();
    }

    public class StopwatchClock : IClock
    {
        public long CurrentTimeNanoseconds()
        {
            return Stopwatch.GetTimestamp()*100L;
        }
    }

    public class DateTimeClock : IClock
    {
        public long CurrentTimeNanoseconds()
        {
            return DateTime.UtcNow.Ticks*100L;
        }
    }
}