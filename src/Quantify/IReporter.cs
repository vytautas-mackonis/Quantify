using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quantify
{
    public interface IReporter
    {
        Task Report(IEnumerable<IMetric> metrics);
    }
}