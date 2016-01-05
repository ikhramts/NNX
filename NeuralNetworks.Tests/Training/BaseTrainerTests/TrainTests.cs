using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NeuralNetworks.Tests.TestObjects;
using NeuralNetworks.Training;
using NeuralNetworks.Utils;
using Xunit;

namespace NeuralNetworks.Tests.Training.BaseTrainerTests
{
    public class TrainTests : IDisposable
    {
        private readonly Mock<BaseTrainer> _mockTrainer;
        private readonly IList<InputOutput> _trainingSet;
        private readonly INeuralNetwork _nn;

        private IList<InputOutput> _trainingSubSet;
        private IList<InputOutput> _validationSubSet;
        private INeuralNetwork _trainedNeuralNet; 

        public TrainTests()
        {
            _mockTrainer = GetMockTrainer();
            _trainingSet = GetTrainingSet();
            _nn = GetNeuralNetwork();
            MockRandom.SetUp();
        }

        public void Dispose()
        {
            MockRandom.Dispose();
        }

        [Fact]
        public void ShouldValidate()
        {
            _mockTrainer.Object.Train(_trainingSet, _nn);
            _mockTrainer.Verify(t => t.Validate(), Times.Once);
        }

        [Fact]
        public void IfNotValid_Throw()
        {
            _mockTrainer.Setup(t => t.Validate()).Throws(new NeuralNetworkException("x"));
            Action action = () =>_mockTrainer.Object.Train(_trainingSet, _nn);
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage("x");
        }

        [Fact]
        public void IfValidationSetFractionIsZero_ShouldPassFullTrainingSet()
        {
            _mockTrainer.Object.Train(_trainingSet, _nn);
            _trainingSubSet.Should().Equal(_trainingSet);
        }

        [Fact]
        public void IfValidationSetFractionNotZero_ShouldMakeValidationSet()
        {
            _mockTrainer.Protected().Setup<double>("GetValidationSetFraction").Returns(0.25);
            _mockTrainer.Object.Train(_trainingSet, _nn);
            _trainingSubSet.Should().HaveCount(3);
            _validationSubSet.Should().HaveCount(1);
        }

        [Fact]
        public void ShuoldTrainPassedNeuralNet()
        {
            _mockTrainer.Object.Train(_trainingSet, _nn);
            Assert.Same(_nn, _trainedNeuralNet);
        }

        [Fact]
        public void IfInitializeWeights_ShouldInitializeWeights()
        {
            var trainer = _mockTrainer.Object;
            trainer.ShouldInitializeWeights = false;
            _mockTrainer.Object.Train(_trainingSet, _nn);
            var expected = GetWeights();
            _nn.Weights[0].Should().Equal(expected[0]);
            _nn.Weights[1].Should().Equal(expected[1]);
        }

        [Fact]
        public void IfNotInitializeWeights_ShouldNotInitializeWeights()
        {
            var trainer = _mockTrainer.Object;
            trainer.ShouldInitializeWeights = true;
            _mockTrainer.Object.Train(_trainingSet, _nn);

            var expected = MockRandom.DoubleValue - 0.5;

            _nn.Weights[0].Should().Equal(expected, expected, expected);
            _nn.Weights[1].Should().Equal(expected);
        }


        //================= Private Helpers =======================
        private Mock<BaseTrainer> GetMockTrainer()
        {
            var mock = new Mock<BaseTrainer>();
            mock.Setup(t => t.Train(It.IsAny<IList<InputOutput>>(), 
                                    It.IsAny<IList<InputOutput>>(), 
                                    It.IsAny<IRandomGenerator>(),
                                    It.IsAny<INeuralNetwork>()))
                .Callback((IList<InputOutput> trainingSubSet,
                           IList<InputOutput> validationSubSet,
                           IRandomGenerator rand,
                           INeuralNetwork nn) =>
                {
                    _trainingSubSet = trainingSubSet;
                    _validationSubSet = validationSubSet;
                    _trainedNeuralNet = nn;
                });
            mock.Setup(t => t.Validate());
            mock.Protected().Setup<double>("GetValidationSetFraction").Returns(0);

            return mock;
        }

        private IList<InputOutput> GetTrainingSet() =>
            new[]
            {
                new InputOutput {Input = new[] {0.1, 0.2}, Output = new[] {0.0, 1.0}},
                new InputOutput {Input = new[] {0.2, 0.3}, Output = new[] {0.0, 1.0}},
                new InputOutput {Input = new[] {0.4, 0.5}, Output = new[] {1.0, 0.0}},
                new InputOutput {Input = new[] {0.6, 0.7}, Output = new[] {1.0, 0.0}},
            };

        private INeuralNetwork GetNeuralNetwork()
        {
            var mock = new Mock<INeuralNetwork>();
            var weights = GetWeights();
            mock.SetupGet(n => n.Weights).Returns(weights);
            return mock.Object;
        }

        private double[][] GetWeights() => new[] { new[] { 0.1, 0.1, 0.1 }, new[] { 0.2 } };
    }
}
