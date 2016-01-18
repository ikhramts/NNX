using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeuralNetworks;
using NeuralNetworks.Training;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class TrainMultilayerPerceptronTests : ObjectStoreTestBase
    {
        private readonly object[,] _inputs = { { 1, 2.0 }, { 3, 4.0 } };
        private readonly object[,] _targets = { { 1, 0 }, { 0.0, 1 } };
        private readonly double[] _hiddenLayerSizes = {2, 3};

        private IList<InputOutput> _actualInputs;
        private readonly string _nnName;

        public TrainMultilayerPerceptronTests()
        {
            ExcelFunctions.MakeSimpleGradientTrainer("trainer", 2, 0.1, 0.2, 0.3, 1, 0, 3);
            _nnName = ExcelFunctions.TrainMultilayerPerceptron("nn", "trainer", _inputs, _targets, _hiddenLayerSizes);
        }

        [Fact]
        public void ShouldCreateTwoLayerPerceptron()
        {
            var result = ObjectStore.Get<MultilayerPerceptron>("nn");
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnTwoLayerPerceptronName()
        {
            Assert.Equal("nn", _nnName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void IfNumHiddenNodesIsNotPositive_Throw(int bad)
        {
            _hiddenLayerSizes[0] = bad;
            Action action = 
                () => ExcelFunctions.TrainMultilayerPerceptron("nn", "trainer", _inputs, _targets, _hiddenLayerSizes);
            action.ShouldThrow<NeuralNetworkException>();
        }

        [Fact]
        public void IfHeightOfInputsDoesNotMatchHeightOfTargets_Throw()
        {
            var targets = new object[,] { { 0, 1 } };
            Action action = 
                () => ExcelFunctions.TrainMultilayerPerceptron("nn", "trainer", _inputs, targets, _hiddenLayerSizes);
            action.ShouldThrow<NNXException>()
                .WithMessage(
                    "*Height of Inputs matrix (was 2) should be equal to height of Targets matrix (was 1).*");
        }

        [Fact]
        public void IfTranerConfigDoesNotExist_Throw()
        {
            Action action = 
                () => ExcelFunctions.TrainMultilayerPerceptron("nn", "no such", _inputs, _targets, _hiddenLayerSizes);
            action.ShouldThrow<NNXException>();
        }

        [Fact]
        public void IfTranderConfigIsNotOfTypeTrainerConfig_Throw()
        {
            ObjectStore.Add("bad", "string");
            Action action = 
                () => ExcelFunctions.TrainMultilayerPerceptron("nn", "bad", _inputs, _targets, _hiddenLayerSizes);
            action.ShouldThrow<NNXException>();
        }

        [Fact]
        public void TrainedNetworkShouldHaveCorrectNumHidden()
        {
            var result = ObjectStore.Get<MultilayerPerceptron>("nn");
            var expected = new[] {2, 3};
            result.HiddenLayerSizes.Should().Equal(expected);
        }

        [Fact]
        public void TrainedNeuralNetworkShouldHaveCorrectNumInputs()
        {
            var result = ObjectStore.Get<INeuralNetwork>("nn");
            result.NumInputs.Should().Be(_inputs.GetLength(0));
        }

        [Fact]
        public void TrainedNetworkShouldHaveCorrectNumOutputs()
        {
            var result = ObjectStore.Get<INeuralNetwork>("nn");
            result.NumOutputs.Should().Be(_targets.GetLength(0));
        }

        [Fact]
        public void ShouldInvokeTrainOnce()
        {
            var trainerMock = SetupInspectingMockTrainer();
            ExcelFunctions.TrainMultilayerPerceptron("nn", "trainer", _inputs, _targets, _hiddenLayerSizes);
            trainerMock.Verify(t => t.Train(It.IsAny<IList<InputOutput>>(), It.IsAny<INeuralNetwork>()),
                               Times.Exactly(1));
        }

        [Fact]
        public void ShouldPassInputsAndTargetsToTrain()
        {
            SetupInspectingMockTrainer();

            ExcelFunctions.TrainMultilayerPerceptron("nn", "trainer", _inputs, _targets, _hiddenLayerSizes);

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

            ExcelFunctions.TrainMultilayerPerceptron("nn", "trainer", _inputs, _targets, _hiddenLayerSizes);

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

            ExcelFunctions.TrainMultilayerPerceptron("nn", "trainer", _inputs, _targets, _hiddenLayerSizes);

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

            Action action = 
                () => ExcelFunctions.TrainMultilayerPerceptron("nn", "trainer", _inputs, _targets, _hiddenLayerSizes);
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
                .Callback((IList<InputOutput> l, INeuralNetwork nn) => _actualInputs = l);
            var trainer = trainerMock.Object;
            ObjectStore.Add("trainer", trainer);
            return trainerMock;
        }
    }
}
