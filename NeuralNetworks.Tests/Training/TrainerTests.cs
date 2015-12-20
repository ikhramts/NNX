using System;
using FluentAssertions;
using Moq;
using NeuralNetworks.Training;
using NeuralNetworks.Utils;
using Xunit;

namespace NeuralNetworks.Tests.Training
{
    public class TrainerTests : IDisposable
    {
        public void Dispose()
        {
            RandomProvider.GetRandom = RandomProvider.GetDefaultRandom;
        }

        [Fact]
        public void ConstructorFromConfig_SetConfig()
        {
            var trainerConfig = GetSampleTrainerConfig();
            var trainer = new Trainer(trainerConfig);
            Assert.Same(trainerConfig, trainer.Config);
        }

        [Fact]
        public void Train_IfMissingTrainerConfig_Throw()
        {
            var trainingSet = GetTrainingSet();
            var nn = Mock.Of<INeuralNetwork>();
            var trainer = new Trainer();
            Assert.Throws<NeuralNetworkException>(() => trainer.Train(trainingSet, nn));
        }

        [Fact]
        public void Train_IfTrainingSetDoesNotMatchNetwork_Throw()
        {
            var trainerConfig = GetSampleTrainerConfig();
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();
            var nn = new TwoLayerPerceptron(3, 1, 1);
            Assert.Throws<NeuralNetworkException>(() => trainer.Train(trainingSet, nn));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Train_IfNumEpochsNotPositive_Throw(int numEpochs)
        {
            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.NumEpochs = numEpochs;
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();
            Assert.Throws<NeuralNetworkException>(() => trainer.Train(trainingSet, GetSampleNN()));
        }

        [Fact]
        public void Train_IfInitializeWeightsIsFalse_DoNotInitializeWeights()
        {
            var randMock = SetupMockRandom();
            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.InitializeWeights = false;
            var trainer = new Trainer(trainerConfig);
            trainer.Train(GetTrainingSet(), GetMockNeuralNetwork().Object);

            randMock.Verify(r => r.NextDouble(), Times.Never);
        }

        [Fact]
        public void Train_IfInitializeWeightsIsTrue_RandomizeInitialWeights()
        {
            var randMock = SetupMockRandom();
            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.InitializeWeights = true;
            var trainer = new Trainer(trainerConfig);
            trainer.Train(GetTrainingSet(), GetMockNeuralNetwork().Object);

            randMock.Verify(r => r.NextDouble(), Times.AtLeastOnce);
        }

        [Fact]
        public void Train_IfMissingNeuralNetwork_Throw()
        {
            var trainerConfig = GetSampleTrainerConfig();
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();

            Action action = () => trainer.Train(trainingSet, null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Train_OneEpoch()
        {
            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;

            var trainerConfig = GetSampleTrainerConfig();
            var trainer = new Trainer(trainerConfig);
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

        [Fact]
        public void Train_TwoEpochsWithMomentum()
        {
            var mockNeuralNet = GetMockNeuralNetwork();
            var nn = mockNeuralNet.Object;


            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.NumEpochs = 2;
            var trainer = new Trainer(trainerConfig);
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

        public static TrainerConfig GetSampleTrainerConfig()
        {
            return new TrainerConfig
            {
                LearningRate = 0.5,
                Momentum = 2,
                NumEpochs = 1,
                QuadraticRegularization = 0.1,
                InitializeWeights = false
            };
        }

        public INeuralNetwork GetSampleNN()
        {
            var nn = new TwoLayerPerceptron(2, 1, 1);
            nn.Weights[0] = new[] {1.0, 2.0, 3.0};
            nn.Weights[1] = new[] {1.5, 0.5};
            return nn;
        }

        [Theory]
        [InlineData(1, 0.1)]
        [InlineData(0, -0.1)]
        [InlineData(0.5, 0)]
        public void InitializeWeights_ShouldInitWeights(double randResult, double expected)
        {
            var nnMock = GetMockNeuralNetwork();
            var nn = nnMock.Object;

            var randMock = new Mock<IRandomGenerator>();
            randMock.Setup(r => r.NextDouble()).Returns(randResult);
            var rand = randMock.Object;

            Trainer.InitializeWeights(nn, rand);

            nn.Weights.Should().NotBeNull();
            nn.Weights.Should().HaveCount(2);
            nn.Weights[0].Should().Equal(expected, expected, expected);
            nn.Weights[1].Should().Equal(expected);
        }

        [Fact]
        public void AdjustWeight_ShouldApplyLearningRate()
        {
            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.Momentum = 0;
            trainerConfig.QuadraticRegularization = 0;
            var trainer = new Trainer(trainerConfig);

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
            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.QuadraticRegularization = 0;
            var trainer = new Trainer(trainerConfig);

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
            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.Momentum = 0;
            var trainer = new Trainer(trainerConfig);

            var nnMock = GetMockNeuralNetwork();
            var nn = nnMock.Object;
            trainer.AdjustWeights(nn, GetSampleGradients(), GetSamplePrevGradients());

            var actualWeights = nn.Weights;
            actualWeights.Should().HaveCount(2);
            actualWeights[0].Should().Equal(0.825, 1.65, 2.475);
            actualWeights[1].Should().HaveCount(1);
            actualWeights[1][0].Should().BeApproximately(0.425, 1e-12);
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

        private Mock<IRandomGenerator> SetupMockRandom()
        {
            var mock = new Mock<IRandomGenerator>();
            mock.SetupAllProperties();
            mock.Setup(r => r.Next(It.IsAny<int>())).Returns((int i) => 0);
            mock.Setup(r => r.NextDouble()).Returns(() => 0.5);
            RandomProvider.GetRandom = seed => mock.Object;
            return mock;
        }

    }
}
