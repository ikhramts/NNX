using FluentAssertions;
using NeuralNetworks;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class GetMeanSquareErrorTests
    {
        private readonly double[] _actuals = { 0.2, 0.3, 0.5 };
        private readonly double[] _outputs = { 0.5, 0.3, 0.2 };

        [Fact]
        public void IfArraysAreDifferentSize_Throw()
        {
            var outputs = new[] { 0.5, 0.3 };
            Assert.Throws<NeuralNetworkException>(() => ExcelFunctions.GetMeanSquareError(_actuals, outputs));
        }

        [Fact]
        public void ShouldCalculateCrossentropy()
        {
            var expected = 0.06;
            var result = ExcelFunctions.GetMeanSquareError(_actuals, _outputs);
            result.Should().BeApproximately(expected, 1e-12);
        }
    }
}
