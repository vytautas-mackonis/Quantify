using System;

namespace Quantify.Sampling
{
    public interface ISamplingReservoirFactory
    {
        IReservoir<T> Create<T>(IClock clock)
            where T : struct, IComparable;
    }
}