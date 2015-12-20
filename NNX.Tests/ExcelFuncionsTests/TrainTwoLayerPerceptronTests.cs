using System;
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
        private readonly object[,] _inputs = {{1, 2.0}, {3, 4.0}};
        private readonly object[,] _targets = {{1, 0}, {0.0, 1}};
        private int _numHidden = 1;

        private IList<InputOutput> _actualInputs; 

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
            var targets = new object[,] {{0, 1}};
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
            var trainerMock = SetupInspectingMockTrainer();
            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            trainerMock.Verify(t => t.Train(It.IsAny<IList<InputOutput>>(), It.IsAny<INeuralNetwork>()), 
                               Times.Exactly(1));
        }

        [Fact]
        public void ShouldPassInputsAndTargetsToTrain()
        {
            SetupInspectingMockTrainer();

            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);

            _actualInputs.Should().NotBeNull();
            _actualInputs.Count.Should().Be(2);
            _actualInputs[0].Input.Should().Equal(Convert.ToDouble(_inputs[0, 0]), Convert.ToDouble(_inputs[0, 1]));
            _actualInputs[1].Input.Should().Equal(Convert.ToDouble(_inputs[1, 0]), Convert.ToDouble(_inputs[1, 1]));
            _actualInputs[0].Output.Should().Equal(Convert.ToDouble(_targets[0, 0]), Convert.ToDouble(_targets[0, 1]));
            _actualInputs[1].Output.Should().Equal(Convert.ToDouble(_targets[1, 0]), Convert.ToDouble(_targets[1, 1]));
        }

        [Theory]
        [MemberData("GetBadInputValues")]
        public void IfInputHasBadData_SkipPoint(object bad)
        {
            _inputs[0, 1] = bad;
            SetupInspectingMockTrainer();

            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);

            _actualInputs.Should().NotBeNull();
            _actualInputs.Count.Should().Be(1);
            _actualInputs[0].Input.Should().Equal(Convert.ToDouble(_inputs[1, 0]), Convert.ToDouble(_inputs[1, 1]));
            _actualInputs[0].Output.Should().Equal(Convert.ToDouble(_targets[1, 0]), Convert.ToDouble(_targets[1, 1]));
        }

        [Theory]
        [MemberData("GetBadInputValues")]
        public void IfTargetPointHasBadData_SkipPoint(object bad)
        {
            _targets[1, 1] = bad;
            SetupInspectingMockTrainer();

            ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);

            _actualInputs.Should().NotBeNull();
            _actualInputs.Count.Should().Be(1);
            _actualInputs[0].Input.Should().Equal(Convert.ToDouble(_inputs[0, 0]), _inputs[0, 1]);
            _actualInputs[0].Output.Should().Equal(Convert.ToDouble(_targets[0, 0]), Convert.ToDouble(_targets[0, 1]));
        }

        [Theory]
        [MemberData("GetBadInputValues")]
        public void IfAllPointsAreBad_Throw(object bad)
        {
            _targets[1, 1] = bad;
            _inputs[0, 1] = bad;

            Action action = () => ExcelFunctions.TrainTwoLayerPerceptron("nn", "config", _inputs, _targets, _numHidden);
            action.ShouldThrow<NNXException>()
                .WithMessage("*There were no good input/target point pairs.*");
        }

        public static IEnumerable<object[]> GetBadInputValues()
        {
            return new[]
            {
                new object[] {"x"},
                new[] {new object()},
                new object[] {new Exception()}, 
            };
        } 

        private Mock<ITrainer> SetupInspectingMockTrainer()
        {
            var trainerMock = new Mock<ITrainer>();
            trainerMock.SetupAllProperties();
            _actualInputs = null;
            trainerMock.Setup(t => t.Train(It.IsAny<IList<InputOutput>>(), It.IsAny<INeuralNetwork>()))
                .Callback((IList<InputOutput> l, INeuralNetwork nn) => _actualInputs = l)
                .Returns((IList<InputOutput> l, INeuralNetwork nn) => nn);
            var trainer = trainerMock.Object;
            TrainerProvider.GetTrainer = () => trainer;
            return trainerMock;
        }
    }

}
