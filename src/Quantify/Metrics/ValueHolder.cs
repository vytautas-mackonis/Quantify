namespace Quantify.Metrics
{
    internal class ValueHolder<T>
    {
        public T Value { get; }

        public ValueHolder(T value)
        {
            Value = value;
        }
    }
}