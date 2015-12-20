using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeuralNetworks.Training;
using NeuralNetworks.Utils;
using Xunit;

namespace NeuralNetworks.Tests.Training
{
    public class TrainerTests : IDisposable
    {
        public TrainerTests()
        {
            NeuralNetworkBuilder.Builder = new NeuralNetworkBuilder();
        }

        public void Dispose()
        {
            NeuralNetworkBuilder.Builder = new NeuralNetworkBuilder();
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
            var nnConfig = GetSampleNNConfig();
            var trainer = new Trainer();
            Assert.Throws<NeuralNetworkException>(() => trainer.Train(trainingSet, nnConfig));
        }

        [Fact]
        public void Train_IfTrainingSetDoesNotMatchNetwork_Throw()
        {
            var trainerConfig = GetSampleTrainerConfig();
            var nnConfig = GetSampleNNConfig();
            nnConfig.Settings["NumInputs"] = "3";
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();
            Assert.Throws<NeuralNetworkException>(() => trainer.Train(trainingSet, nnConfig));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Train_IfNumEpochsNotPositive_Throw(int numEpochs)
        {
            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.NumEpochs = numEpochs;
            var nnConfig = GetSampleNNConfig();
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();
            Assert.Throws<NeuralNetworkException>(() => trainer.Train(trainingSet, nnConfig));
        }

        [Fact]
        public void Train_IfMissingNeuralNetworkConfig_Throw()
        {
            var trainerConfig = GetSampleTrainerConfig();
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();

            Action action = () => trainer.Train(trainingSet, null);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Train_ShouldReturnConfiguredNetwork()
        {
            var builder = new NeuralNetworkBuilder();
            var builderMock = new Mock<NeuralNetworkBuilder>();
            builderMock.Setup(b => b.CustomBuild(It.IsAny<NeuralNetworkConfig>()))
                .Returns((NeuralNetworkConfig c) => builder.CustomBuild(c));

            NeuralNetworkBuilder.Builder = builderMock.Object;

            var trainerConfig = GetSampleTrainerConfig();
            var nnConfig = GetSampleNNConfig();
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();

            trainer.Train(trainingSet, nnConfig);
            builderMock.Verify(b => b.CustomBuild(nnConfig), Times.Exactly(1));
        }


        [Fact]
        public void Train_OneEpoch()
        {
            var mockNeuralNet = GetMockNeuralNetwork();

            var builderMock = new Mock<NeuralNetworkBuilder>();
            builderMock.Setup(b => b.CustomBuild(It.IsAny<NeuralNetworkConfig>()))
                .Returns((NeuralNetworkConfig c) => mockNeuralNet.Object);

            NeuralNetworkBuilder.Builder = builderMock.Object;

            var trainerConfig = GetSampleTrainerConfig();
            var nnConfig = GetSampleNNConfig();
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();

            var nn = trainer.Train(trainingSet, nnConfig);

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

            var builderMock = new Mock<NeuralNetworkBuilder>();
            builderMock.Setup(b => b.CustomBuild(It.IsAny<NeuralNetworkConfig>()))
                .Returns((NeuralNetworkConfig c) => mockNeuralNet.Object);

            NeuralNetworkBuilder.Builder = builderMock.Object;

            var trainerConfig = GetSampleTrainerConfig();
            trainerConfig.NumEpochs = 2;
            var nnConfig = GetSampleNNConfig();
            var trainer = new Trainer(trainerConfig);
            var trainingSet = GetTrainingSet();

            var nn = trainer.Train(trainingSet, nnConfig);

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
                QuadraticRegularization = 0.1
            };
        }

        public NeuralNetworkConfig GetSampleNNConfig()
        {
            return new NeuralNetworkConfig
            {
                NetworkType = "TwoLayerPerceptron",
                Settings = new Dictionary<string, string>
                {
                    {"NumInputs", "2"},
                    {"NumHidden", "1"},
                    {"NumOutputs", "1"},
                },
                Weights = new[] {new[] {1.0, 2.0, 3.0}, new[] {1.5, 0.5}}
            };
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
            mock.SetupGet(nn => nn.Outputs).Returns(() => new []{0.25});
            mock.Setup(nn => nn.CalculateGradients(It.IsAny<double[]>()))
                .Returns((double[] t) => GetSampleGradients());

            return mock;
        }

        public static double[][] GetSampleGradients() => new[] {new[] {0.25, 0.5, 0.75}, new[] {2.0}};
        public static double[][] GetSampleWeights() => new[] {new[] {1.0, 2.0, 3.0}, new[] {1.5}};
        public static double[][] GetSamplePrevGradients() => new[] {new[] {0.3, 0.4, 0.1}, new[] {0.2}};

    }
}
