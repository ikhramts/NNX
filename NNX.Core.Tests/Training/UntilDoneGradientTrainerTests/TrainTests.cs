using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NNX.Core.Tests.Assertions;
using NNX.Core.Tests.TestObjects;
using NNX.Core.Training;
using Xunit;

namespace NNX.Core.Tests.Training.UntilDoneGradientTrainerTests
{
    public class TrainTests
    {
        private int _stepsOfImprovementRemaning = 10;
        private readonly UntilDoneGradientTrainer _trainer = GetTrainer();

        [Theory]
        [InlineData(1, 1, 1, 4)]
        [InlineData(1, 0, 1, 3)]
        [InlineData(1, 5, 2, 4)]
        [InlineData(3, 3, 2, 4)]
        public void ShouldStopWhenNoImprovement(int lastEpochOfImprovement,
            int maxEpochsWithoutImprovement, 
            int epochsBetweenValidation, 
            int expectedEvaluations)
        {
            _trainer.MaxEpochsWithoutImprovement = maxEpochsWithoutImprovement;
            _trainer.EpochsBetweenValidations = epochsBetweenValidation;
            _trainer.NumEpochs = 100;

            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;

            _stepsOfImprovementRemaning = lastEpochOfImprovement / epochsBetweenValidation + 1;
            _trainer.Train(GetTrainingSet(), GetValidationSet(), MockRandom.Get(), nn);

            mockNeuralNet.Verify(n => n.FeedForward(It.IsAny<double[]>()), Times.Exactly(expectedEvaluations));
        }

        [Fact]
        public void ShouldReturnBestResultWhenStoppedEarly()
        {
            _trainer.MaxEpochsWithoutImprovement = 2;
            _trainer.NumEpochs = 5;

            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;

            _stepsOfImprovementRemaning = 2;
            _trainer.Train(GetTrainingSet(), GetValidationSet(), MockRandom.Get(), nn);

            // Should be same result as training for only 1 epoch.
            var weights = nn.Weights;
            var expected = new[] { new[] { 0.825, 1.65, 2.475 }, new[] { 0.425 } };

            for (var i = 0; i < weights.Length; i++)
            {
                weights[i].ShouldApproximatelyEqual(expected[i]);
            }
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
                weights[i].ShouldApproximatelyEqual(expected[i]);
            }
        }

        [Fact]
        public void Train_TwoEpochsWithMomentum()
        {
            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;

            _trainer.NumEpochs = 2;

            _trainer.Train(GetTrainingSet(), GetValidationSet(), MockRandom.Get(), nn);

            nn.Should().NotBeNull();
            var weights = nn.Weights;
            weights.Should().NotBeNullOrEmpty();
            weights.Should().HaveCount(2);

            var expected = new[] { new[] { 0.40875, 0.8175, 1.22625 }, new[] { -2.59625 } };

            for (var i = 0; i < weights.Length; i++)
            {
                weights[i].ShouldApproximatelyEqual(expected[i]);
            }
        }

        public static InputOutput[] GetTrainingSet()
        {
            return new[]
            {
                new InputOutput{Input = new[] {1.0, 2.0}, Output = new []{0.5}}
            };
        }

        public Mock<INeuralNetwork> GetMockNeuralNetwork()
        {
            var weights = GetWeights();
            var mock = new Mock<INeuralNetwork>();
            mock.SetupGet(nn => nn.NumInputs).Returns(2);
            mock.SetupGet(nn => nn.NumOutputs).Returns(1);
            mock.SetupGet(nn => nn.Weights).Returns(() => weights);
            mock.Setup(nn => nn.CalculateGradients(It.IsAny<double[]>(), It.IsAny<double[]>()))
                .Returns((double[] i, double[] t) => GetGradients());

            // FeedForward() will return increasingly good results for first _stepsOfImprovementRemaning
            // queries, and and bad result on all subsequent queries.
            // This will allow us to test stopping conditions.
            // Note that since this is a mock neural network, FeedForward will be 
            // not be called by any of neural net's internal functions, only by the trainer.
            var good = new[] { 0.99, 0.01 };
            var badResult = new FeedForwardResult { Output = new[] { 0.01, 0.99 } };

            mock.Setup(nn => nn.FeedForward(It.IsAny<double[]>()))
                .Returns((double[] inputs) =>
                {
                    if (_stepsOfImprovementRemaning > 0)
                    {
                        var result = new FeedForwardResult()
                        {
                            Output = new[]
                            {
                                good[0] - ((double) _stepsOfImprovementRemaning) / 100.0,
                                good[1] + ((double) _stepsOfImprovementRemaning) / 100.0,
                            }
                        };

                        _stepsOfImprovementRemaning--;
                        return result;
                    }

                    return badResult;
                });

            return mock;
        }

        public Mock<INeuralNetwork> GetMockNeuralNetworkWithVaryingFeedForward()
        {
            var mock = GetMockNeuralNetwork();

            // FeedForward() will return increasingly good results for first _stepsOfImprovementRemaning
            // queries, and and bad result on all subsequent queries.
            // This will allow us to test stopping conditions.
            // Note that since this is a mock neural network, FeedForward will be 
            // not be called by any of neural net's internal functions, only by the trainer.
            var good = new[] {0.99, 0.01};
            var badResult = new FeedForwardResult { Output = new[] { 0.01, 0.99 }};

            mock.Setup(nn => nn.FeedForward(It.IsAny<double[]>()))
                .Returns((double[] inputs) =>
                {
                    if (_stepsOfImprovementRemaning > 0)
                    {
                        var result = new FeedForwardResult()
                        {
                            Output = new[]
                            {
                                good[0] - ((double) _stepsOfImprovementRemaning) / 100.0,
                                good[1] + ((double) _stepsOfImprovementRemaning) / 100.0,
                            }
                        };

                        _stepsOfImprovementRemaning--;
                        return result;
                    }

                    return badResult;
                });

            return mock;
        }

        public static double[][] GetGradients() => new[] { new[] { 0.25, 0.5, 0.75 }, new[] { 2.0 } };
        public static double[][] GetWeights() => new[] { new[] { 1.0, 2.0, 3.0 }, new[] { 1.5 } };
        public static double[][] GetPrevGradients() => new[] { new[] { 0.3, 0.4, 0.1 }, new[] { 0.2 } };

        public static IList<InputOutput> GetValidationSet() => new[]
        {
            new InputOutput {Input = new[] {0.25, 0.5}, Output = new[] {1.0, 0.0}},
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
