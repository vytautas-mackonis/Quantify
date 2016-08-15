namespace Quantify
{
    public interface IMetric
    {
        void Accept(IMetricVisitor visitor);
    }
}