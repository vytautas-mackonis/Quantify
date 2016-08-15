using Quantify.Graphite;

namespace Quantify
{
    public static class MetricsBuilderExtensions
    {
        public static MetricsBuilder ReportToGraphite(this MetricsBuilder builder, string hostname, int port, int periodMilliseconds)
        {
            return builder.ReportUsing(new GraphiteMetricsReporter(hostname, port), periodMilliseconds);
        }
    }
}