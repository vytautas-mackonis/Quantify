namespace Quantify
{
    public static class Metrics
    {
        public static MetricsBuilder Configure()
        {
            return new MetricsBuilder();
        }
    }
}