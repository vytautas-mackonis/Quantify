using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quantify.Metrics
{
    public class Timer
    {
        public IDisposable StartTiming()
        {
            return null;
        }
    }

    public static class TimerExtensions
    {
        public static void Time(this Timer timer, Action action)
        {

        }

        public static Task TimeAsync(this Timer timer, Func<Task> action)
        {
            return null;
        }

        public static T Time<T>(this Timer timer, Func<T> action)
        {
            return default(T);
        }

        public static Task<T> TimeAsync<T>(this Timer timer, Func<Task<T>> action)
        {
            return null;
        }
    }

    public class TimerValue
    {
        public readonly MeterValue Rate;
        public readonly HistogramValue<long> Histogram;
        public readonly long ActiveSessions;
    }
}
