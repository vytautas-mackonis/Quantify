using System;
using Quantify.Metrics.Time;

namespace Quantify.Tests
{
    public class FakeClock : IClock
    {
        public long CurrentTime = DateTime.Now.Ticks * 100L;

        public long CurrentTimeNanoseconds()
        {
            return CurrentTime;
        }

        public void AdvanceSeconds(int seconds)
        {
            AdvanceNanoSeconds(seconds * 1000000000L);
        }

        public void AdvanceNanoSeconds(long ns)
        {
            CurrentTime += ns;
        }
    }
}