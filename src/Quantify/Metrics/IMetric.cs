namespace Quantify.Metrics
{
    public interface IMetric
    {
        void Accept(IMetricVisitor visitor);
    }
}