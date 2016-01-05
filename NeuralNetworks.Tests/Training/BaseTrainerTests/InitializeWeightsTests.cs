using FluentAssertions;
using Moq;
using NeuralNetworks.Training;
using NeuralNetworks.Utils;
using Xunit;

namespace NeuralNetworks.Tests.Training.BaseTrainerTests
{
    public class InitializeWeightsTests
    {
        public void Dispose()
        {
            RandomProvider.GetRandom = RandomProvider.GetDefaultRandom;
        }

        [Theory]
        [InlineData(1.0, 0.5)]
        [InlineData(0, -0.5)]
        [InlineData(0.5, 0)]
        public void InitializeWeights_ShouldInitWeights(double randResult, double expected)
        {
            var nnMock = GetMockNeuralNetwork();
            var nn = nnMock.Object;

            var randMock = new Mock<IRandomGenerator>();
            randMock.Setup(r => r.NextDouble()).Returns(randResult);
            var rand = randMock.Object;

            BaseTrainer.InitializeWeights(nn, rand);

            nn.Weights.Should().NotBeNull();
            nn.Weights.Should().HaveCount(2);
            nn.Weights[0].Should().Equal(expected, expected, expected);
            nn.Weights[1].Should().Equal(expected);
        }

        public static Mock<INeuralNetwork> GetMockNeuralNetwork()
        {
            var weights = GetSampleWeights();
            var mock = new Mock<INeuralNetwork>();
            mock.SetupGet(nn => nn.NumInputs).Returns(2);
            mock.SetupGet(nn => nn.NumOutputs).Returns(1);
            mock.SetupGet(nn => nn.Weights).Returns(() => weights);

            return mock;
        }

        public static double[][] GetSampleWeights() => new[] { new[] { 1.0, 2.0, 3.0 }, new[] { 1.5 } };
    }
}
