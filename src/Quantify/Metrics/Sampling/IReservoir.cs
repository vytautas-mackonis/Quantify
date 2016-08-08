using System;
using System.Linq;
using System.Threading.Tasks;

namespace Quantify.Metrics.Sampling
{
    public interface IReservoir<T>
        where T: struct, IComparable
    {
        int Size { get; }
        void Mark(T value);
        ISampleSet<T> GetSamples();
    }
}
