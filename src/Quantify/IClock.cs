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