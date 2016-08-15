using System;

namespace Quantify.Sampling
{
    public interface IReservoir<T>
        where T: struct, IComparable
    {
        int Size { get; }
        void Mark(T value);
        ISampleSet<T> GetSamples();
    }
}
