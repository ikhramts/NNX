using System;
using FluentAssertions;
using Moq;
using NeuralNetworks.Training;
using Xunit;

namespace NeuralNetworks.Tests.Training
{
    public class SimpleGradientTrainerTests
    {
        [Fact]
        public void Train_IfTrainingSetDoesNotMatchNetwork_Throw()
        {
            var trainer = GetSampleTrainer();
            var trainingSet = GetTrainingSet();
            var nn = new TwoLayerPerceptron(3, 1, 1);
            Assert.Throws<NeuralNetworkException>(() => trainer.Train(trainingSet, nn));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Train_IfNumEpochsNotPositive_Throw(int numEpochs)
        {
            var trainer = GetSampleTrainer();
            trainer.NumEpochs = numEpochs;
            var trainingSet = GetTrainingSet();
            Assert.Throws<NeuralNetworkException>(() => trainer.Train(trainingSet, GetSampleNN()));
        }

        [Fact]
        public void Train_IfMissingNeuralNetwork_Throw()
        {
            var trainer = GetSampleTrainer();
            var trainingSet = GetTrainingSet();

            Action action = () => trainer.Train(trainingSet, null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Train_OneEpoch(int batchSize)
        {
            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;

            var trainer = GetSampleTrainer();
            trainer.BatchSize = batchSize;
            var trainingSet = GetTrainingSet();

            trainer.Train(trainingSet, nn);

            nn.Should().NotBeNull();
            var weights = nn.Weights;
            weights.Should().NotBeNullOrEmpty();
            weights.Should().HaveCount(2);

            var expected = new[] {new[] {0.825, 1.65, 2.475}, new[] {0.425}};

            for (var i = 0; i < weights.Length; i++)
            {
                weights[i].Should().HaveCount(expected[i].Length);

                for (var j = 0; j < weights[i].Length; j++)
                    weights[i][j].Should().BeApproximately(expected[i][j], 1e-12);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Train_TwoEpochsWithMomentum(int batchSize)
        {
            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;

            var trainer = GetSampleTrainer();
            trainer.BatchSize = batchSize;
            trainer.NumEpochs = 2;
            var trainingSet = GetTrainingSet();

            trainer.Train(trainingSet, nn);

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
            return new []
            {
                new InputOutput{Input = new[] {1.0, 2.0}, Output = new []{0.5}}
            };
        }

        public INeuralNetwork GetSampleNN()
        {
            var nn = new TwoLayerPerceptron(2, 1, 1);
            nn.Weights[0] = new[] {1.0, 2.0, 3.0};
            nn.Weights[1] = new[] {1.5, 0.5};
            return nn;
        }

        [Fact]
        public void AdjustWeight_ShouldApplyLearningRate()
        {
            var trainer = GetSampleTrainer();
            trainer.Momentum = 0;
            trainer.QuadraticRegularization = 0;

            var nnMock = GetMockNeuralNetwork();
            var nn = nnMock.Object;

            trainer.AdjustWeights(nn, GetSampleGradients(), GetSamplePrevGradients());

            var actualWeights = nn.Weights;
            actualWeights.Should().HaveCount(2);
            actualWeights[0].Should().Equal(0.875, 1.75, 2.625);
            actualWeights[1].Should().Equal(0.5);
        }

        [Fact]
        public void AdjustWeights_ShouldApplyMomentum()
        {
            var trainer = GetSampleTrainer();
            trainer.QuadraticRegularization = 0;

            var nnMock = GetMockNeuralNetwork();
            var nn = nnMock.Object;
            trainer.AdjustWeights(nn, GetSampleGradients(), GetSamplePrevGradients());

            var actualWeights = nn.Weights;
            actualWeights.Should().HaveCount(2);
            actualWeights[0].Should().Equal(0.575, 1.35, 2.525);
            actualWeights[1].Should().HaveCount(1);
            actualWeights[1][0].Should().BeApproximately(0.3, 1e-12);
        }

        [Fact]
        public void AdjustWeights_ShouldApplyQuadraticRegularization()
        {
            var trainer = GetSampleTrainer();
            trainer.Momentum = 0;

            var nnMock = GetMockNeuralNetwork();
            var nn = nnMock.Object;
            trainer.AdjustWeights(nn, GetSampleGradients(), GetSamplePrevGradients());

            var actualWeights = nn.Weights;
            actualWeights.Should().HaveCount(2);
            actualWeights[0].Should().Equal(0.825, 1.65, 2.475);
            actualWeights[1].Should().HaveCount(1);
            actualWeights[1][0].Should().BeApproximately(0.425, 1e-12);
        }

        [Fact]
        public void Validate_IfValid_ShouldDoNothing()
        {
            var config = new SimpleGradientTrainer { LearningRate = 0.1, NumEpochs = 100 };
            config.Validate();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void Validate_IfLearningRateNotPositive_Throw(double badLearnignRate)
        {
            var trainer = new SimpleGradientTrainer { LearningRate = badLearnignRate, NumEpochs = 100 };
            Action action = () => trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property LearningRate must be positive; was {badLearnignRate}*");
        }

        [Fact]
        public void Validate_IfMomentumNegative_Throw()
        {
            const double badMomentum = -0.2;
            var trainer = new SimpleGradientTrainer { LearningRate = 0.1, NumEpochs = 100, Momentum = badMomentum };
            Action action = () => trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property Momentum cannot be negative; was {badMomentum}*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void Validate_IfNumEpochsIsNotPositive_Throw(int badNumEpochs)
        {
            var trainer = new SimpleGradientTrainer { LearningRate = 0.1, NumEpochs = badNumEpochs };
            Action action = () => trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property NumEpochs must be positive; was {badNumEpochs}*");
        }

        [Fact]
        public void Validate_IfQuadraticRegularizationNegative_Throw()
        {
            const double bad = -0.1;
            var trainer = new SimpleGradientTrainer { LearningRate = 0.1, NumEpochs = 100, QuadraticRegularization = bad };
            Action action = () => trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property QuadraticRegularization cannot be negative; was {bad}*");
        }

        public static Mock<INeuralNetwork> GetMockNeuralNetwork()
        {
            var weights = GetSampleWeights();
            var mock = new Mock<INeuralNetwork>();
            mock.SetupGet(nn => nn.NumInputs).Returns(2);
            mock.SetupGet(nn => nn.NumOutputs).Returns(1);
            mock.SetupGet(nn => nn.Weights).Returns(() => weights);
            mock.Setup(nn => nn.CalculateGradients(It.IsAny<double[]>(), It.IsAny<double[]>()))
                .Returns((double[] i, double[] t) => GetSampleGradients());

            return mock;
        }

        public static double[][] GetSampleGradients() => new[] {new[] {0.25, 0.5, 0.75}, new[] {2.0}};
        public static double[][] GetSampleWeights() => new[] {new[] {1.0, 2.0, 3.0}, new[] {1.5}};
        public static double[][] GetSamplePrevGradients() => new[] {new[] {0.3, 0.4, 0.1}, new[] {0.2}};

        private static SimpleGradientTrainer GetSampleTrainer()
        {
            var trainer = new SimpleGradientTrainer
            {
                LearningRate = 0.5,
                Momentum = 2,
                NumEpochs = 1,
                QuadraticRegularization = 0.1,
                ShouldInitializeWeights = false
            };

            return trainer;
        }

    }
}
