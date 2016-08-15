using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Quantify.Sampling;
using Xunit;

namespace Quantify.Tests
{
    public class GraphiteReporterTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void InvalidHostnameThrows(string name)
        {
            Assert.Throws<ArgumentException>(() => Metrics.Configure().ReportToGraphite(name, 80, 100));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(65536)]
        [InlineData(151851)]
        public void InvalidPortThrows(int port)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Metrics.Configure().ReportToGraphite("localhost", port, 100));
        }
    }
}
