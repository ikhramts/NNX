using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeuralNetworks.Tests.TestObjects;
using NeuralNetworks.Training;
using Xunit;

namespace NeuralNetworks.Tests.Training.UntilDoneGradientTrainerTests
{
    public class TrainTests
    {
        private double[] _testOutput = {0.99, 0.01};
        private UntilDoneGradientTrainer _trainer = GetTrainer();

        [Fact]
        public void ShouldStopWhenNoImprovement()
        {
            _trainer.MaxEpochsWithoutImprovement = 1;
            _trainer.NumEpochs = 3;

            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;



        }

        [Fact]
        public void Train_OneEpoch()
        {
            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;

            _trainer.Train(GetTrainingSet(), GetValidationSet(), MockRandom.Get(), nn);

            nn.Should().NotBeNull();
            var weights = nn.Weights;
            weights.Should().NotBeNullOrEmpty();
            weights.Should().HaveCount(2);

            var expected = new[] { new[] { 0.825, 1.65, 2.475 }, new[] { 0.425 } };

            for (var i = 0; i < weights.Length; i++)
            {
                weights[i].Should().HaveCount(expected[i].Length);

                for (var j = 0; j < weights[i].Length; j++)
                    weights[i][j].Should().BeApproximately(expected[i][j], 1e-12);
            }
        }

        [Fact]
        public void Train_TwoEpochsWithMomentum()
        {
            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;

            _trainer.NumEpochs = 2;
            var trainingSet = GetTrainingSet();

            _trainer.Train(trainingSet, nn);

            nn.Should().NotBeNull();
            var weights = nn.Weights;
            weights.Should().NotBeNullOrEmpty();
            weights.Should().HaveCount(2);

            var expected = new[] { new[] { 0.40875, 0.8175, 1.22625 }, new[] { -2.59625 } };

            for (var i = 0; i < weights.Length; i++)
            {
                weights[i].Should().HaveCount(expected[i].Length);

                for (var j = 0; j < weights[i].Length; j++)
                    weights[i][j].Should().BeApproximately(expected[i][j], 1e-12);
            }
        }

        public static InputOutput[] GetTrainingSet()
        {
            return new[]
            {
                new InputOutput{Input = new[] {1.0, 2.0}, Output = new []{0.5}}
            };
        }

        public static Mock<INeuralNetwork> GetMockNeuralNetwork()
        {
            var weights = GetWeights();
            var mock = new Mock<INeuralNetwork>();
            mock.SetupGet(nn => nn.NumInputs).Returns(2);
            mock.SetupGet(nn => nn.NumOutputs).Returns(1);
            mock.SetupGet(nn => nn.Weights).Returns(() => weights);
            mock.Setup(nn => nn.CalculateGradients(It.IsAny<double[]>(), It.IsAny<double[]>()))
                .Returns((double[] i, double[] t) => GetGradients());

            return mock;
        }

        public Mock<INeuralNetwork> GetMockNeuralNetworkWithVaryingFeedForward()
        {
            var mock = GetMockNeuralNetwork();

            // FeedForward() will return a good result on first query, and bad result on all
            // subsequent queries.
            // Note that since this is a mock neural network, FeedForward will be 
            // not be called by any of neural net's internal functions, only by the trainer.

            var good = new FeedForwardResult {Output = new[] {0.99, 0.01}};
            var bad = new FeedForwardResult { Output = new[] { 0.01, 0.99 }};
            var isFirst = true;

            mock.Setup(nn => nn.FeedForward(It.IsAny<double[]>()))
                .Returns((double[] inputs) =>
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        return good;
                    }

                    return bad;
                });

            return mock;
        }

        public static double[][] GetGradients() => new[] { new[] { 0.25, 0.5, 0.75 }, new[] { 2.0 } };
        public static double[][] GetWeights() => new[] { new[] { 1.0, 2.0, 3.0 }, new[] { 1.5 } };
        public static double[][] GetPrevGradients() => new[] { new[] { 0.3, 0.4, 0.1 }, new[] { 0.2 } };

        public static IList<InputOutput> GetValidationSet() => new[]
        {
            new InputOutput {Input = new[] {0.25, 0.5,}, Output = new[] {1.0, 0.0}},
        };

        private static UntilDoneGradientTrainer GetTrainer()
        {
            var trainer = new UntilDoneGradientTrainer
            {
                LearningRate = 0.5,
                Momentum = 2,
                NumEpochs = 1,
                QuadraticRegularization = 0.1,
                ShouldInitializeWeights = false,
                MaxEpochsWithoutImprovement = 100,
                ValidationSetFraction = 0.5
            };

            return trainer;
        }
    }
}
