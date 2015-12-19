using FluentAssertions;
using NeuralNetworks;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class GetCrossEntropyErrorTests
    {
        private readonly double[] _actuals = {0.2, 0.3, 0.5};
        private readonly double[] _outputs = {0.5, 0.3, 0.2};

        [Fact]
        public void IfArraysAreDifferentSize_Throw()
        {
            var outputs = new [] {0.5, 0.3};
            Assert.Throws<NeuralNetworkException>(() => ExcelFunctions.GetCrossEntropyError(_actuals, outputs));
        }

        [Fact]
        public void ShouldCalculateCrossentropy()
        {
            var expected = 1.30454023362682;
            var result = ExcelFunctions.GetCrossEntropyError(_actuals, _outputs);
            result.Should().BeApproximately(expected, 1e-12);
        }

    }
}
