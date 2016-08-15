using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Quantify
{
    public interface IMetricsReporter
    {
        Task Report(IEnumerable<IMetric> metrics);
    }
}