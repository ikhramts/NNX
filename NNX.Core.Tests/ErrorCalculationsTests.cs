using Xunit;

namespace NNX.Core.Tests
{
    public class ErrorCalculationsTests
    {
        [Fact]
        public void CrossEntropyError_Calculate()
        {
            var target = new[] {0.05, 0.8, 0.15};
            var output = new[] {0.3, 0.5, 0.2};

            var actual = ErrorCalculations.CrossEntropyError(target, output);
            var expected = 0.856132071529368;

            Assert.Equal(expected, actual, 12);
        }

        [Fact]
        public void CrossEntropyError_ThrowOnLengthMismatch()
        {
            var target = new[] {1.0, 1.0};
            var output = new[] {1.0};

            Assert.Throws<NeuralNetworkException>(() => ErrorCalculations.CrossEntropyError(target, output));
        }

        [Fact]
        public void MeanSquareError_Calculate()
        {
            var target = new[] { 0.05, 0.8, 0.15 };
            var output = new[] { 0.3, 0.5, 0.2 };

            var actual = ErrorCalculations.MeanSquareError(target, output);
            var expected = 0.0516666666666667;

            Assert.Equal(expected, actual, 12);
        }

        [Fact]
        public void MeanSquareError_ThrowOnLengthMismatch()
        {
            var target = new[] { 1.0, 1.0 };
            var output = new[] { 1.0 };

            Assert.Throws<NeuralNetworkException>(() => ErrorCalculations.MeanSquareError(target, output));
        }
    }
}
