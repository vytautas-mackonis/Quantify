using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Quantify
{
    public interface IMetricsReporter : IDisposable
    {
        Task Report(IClock clock, IEnumerable<IMetric> metrics);
    }
}