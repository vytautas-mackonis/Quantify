using System.Threading.Tasks;

namespace Quantify
{
    public interface IMetric
    {
        Task AcceptAsync(IMetricVisitor visitor);
    }
}