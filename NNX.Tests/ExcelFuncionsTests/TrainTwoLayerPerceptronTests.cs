using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeuralNetworks;
using NeuralNetworks.Training;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class TrainTwoLayerPerceptronTests : ObjectStoreTestBase
    {
        private readonly double[,] _inputs = {{1, 2}, {3, 4}};
        private readonly double[,] _targets = {{1, 0}, {0, 1}};
        private int _numHidden = 1;

        public TrainTwoLayerPerceptronTests()
        {
            TrainerProvider.GetTrainer = TrainerProvider.GetDefaultTrainer;
            ExcelFunctions.MakeTrainerConfig("config", 2, 0.1, 0.2, 0.3, 3);
        }

        public override void Dispose()
        {
            base.Dispose();
            TrainerProvider.GetTrainer = TrainerProvider.GetDefaultTrainer;
        }

        [Fact]
        public void ShouldCreateTwoLayerPerceptron()
        {
            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            var result = ObjectStore.Get<TwoLayerPerceptron>("nn");
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnTwoLayerPerceptronName()
        {
            var name = ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            Assert.Equal("nn", name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void IfNumHiddenNodesIsNotPositive_Throw(int bad)
        {
            Action action = () => ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, bad);
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Parameter NumHiddenNodes should be positive; was {bad}*");
        }

        [Fact]
        public void IfHeighOfInputsDoesNotMatchHeightOfTargets_Throw()
        {
            var targets = new double[,] {{0, 1}};
            Action action = () => ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, targets, _numHidden);
            action.ShouldThrow<NNXException>()
                .WithMessage(
                    "*Height of Inputs matrix (was 2) should be equal to height of Targets matrix (was 1).*");
        }

        [Fact]
        public void IfTranerConfigDoesNotExist_Throw()
        {
            Action action = () => ExcelFunctions.TrainTwoLayerPerceptron("nn", "no such", _inputs, _targets, _numHidden);
            action.ShouldThrow<NNXException>();
        }

        [Fact]
        public void IfTranderConfigIsNotOfTypeTrainerConfig_Throw()
        {
            ObjectStore.Add("bad", "string");
            Action action = () => ExcelFunctions.TrainTwoLayerPerceptron("nn", "bad", _inputs, _targets, _numHidden);
            action.ShouldThrow<NNXException>();
        }

        [Fact]
        public void TrainedNeuralNetworkShouldHaveCorrectNumInputs()
        {
            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            var result = ObjectStore.Get<TwoLayerPerceptron>("nn");
            result.NumHidden.Should().Be(_numHidden);
        }

        [Fact]
        public void TrainedNetworkShouldHaveCorrectNumHidden()
        {
            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            var result = ObjectStore.Get<TwoLayerPerceptron>("nn");
            result.NumInputs.Should().Be(_inputs.GetLength(0));
        }

        [Fact]
        public void TrainedNetworkShouldHaveCorrectNumOutputs()
        {
            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            var result = ObjectStore.Get<TwoLayerPerceptron>("nn");
            result.NumOutputs.Should().Be(_targets.GetLength(0));
        }

        [Fact]
        public void ShouldInvokeTrainOnce()
        {
            var trainerMock = new Mock<ITrainer>();
            trainerMock.SetupAllProperties();
            trainerMock.Setup(t => t.Train(It.IsAny<IList<InputOutput>>()))
                .Returns((IList < InputOutput > l) => new TwoLayerPerceptron(1, 1, 1));
            var trainer = trainerMock.Object;

            TrainerProvider.GetTrainer = () => trainer;

            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            trainerMock.Verify(t => t.Train(It.IsAny<IList<InputOutput>>()), Times.Exactly(1));
        }

        [Fact]
        public void ShouldPassInputsAndTargetsToTrain()
        {
            var trainerMock = new Mock<ITrainer>();
            trainerMock.SetupAllProperties();
            IList<InputOutput> actualInputs = null;
            trainerMock.Setup(t => t.Train(It.IsAny<IList<InputOutput>>()))
                .Callback((IList<InputOutput> l) => actualInputs = l)
                .Returns((IList<InputOutput> l) => new TwoLayerPerceptron(1, 1, 1));
            var trainer = trainerMock.Object;
            TrainerProvider.GetTrainer = () => trainer;

            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);

            actualInputs.Should().NotBeNull();
            actualInputs.Count.Should().Be(2);
            actualInputs[0].Input.Should().Equal(_inputs[0, 0], _inputs[0, 1]);
            actualInputs[1].Input.Should().Equal(_inputs[1, 0], _inputs[1, 1]);
            actualInputs[0].Output.Should().Equal(_targets[0, 0], _targets[0, 1]);
            actualInputs[1].Output.Should().Equal(_targets[1, 0], _targets[1, 1]);
        }

        [Fact]
        public void ShouldNotAddNeuralNetworkConfigToTrainerConfig()
        {
            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            var trainerConfig = ObjectStore.Get<TrainerConfig>("config");
            trainerConfig.NeuralNetworkConfig.Should().BeNull();
        }
    }

}
