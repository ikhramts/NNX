using FluentAssertions;
using NeuralNetworks;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class MakeTwoLayerPerceptronTests : ObjectStoreTestBase
    {
        [Fact]
        public void ShouldMakeTwoLayerPerceptron()
        {
            var inputs = InvokeMakeTwoLayerPerceptronWithSampleInputs();
            var result = ObjectStore.Get<TwoLayerPerceptron>(inputs.Name);
            Assert.NotNull(result);
        }

        [Fact]
        public void ShouldSetNumHiddenNodesCorrectly()
        {
            var inputs = InvokeMakeTwoLayerPerceptronWithSampleInputs();
            var result = ObjectStore.Get<TwoLayerPerceptron>(inputs.Name);
            Assert.Equal(inputs.NumHiddenNodes, result.NumHidden);
        }

        [Fact]
        public void ShouldReturnObjectName()
        {
            var inputs = GetSampleInputs();
            var name = inputs.Name;
            var result = ExcelFunctions.MakeTwoLayerPerceptron(name, inputs.NumHiddenNodes, inputs.HiddenWeights,
                inputs.OutputWeights);
            Assert.Equal(name, result);
        }

        [Fact]
        public void ShouldSetNumInputsCorrectly()
        {
            var inputs = InvokeMakeTwoLayerPerceptronWithSampleInputs();
            var result = ObjectStore.Get<TwoLayerPerceptron>(inputs.Name);
            var expected = (inputs.HiddenWeights.Length / inputs.NumHiddenNodes) - 1;
            Assert.Equal(expected, result.NumInputs);
        }

        [Fact]
        public void ShouldSetNumOutputsCorrectly()
        {
            var inputs = InvokeMakeTwoLayerPerceptronWithSampleInputs();
            var result = ObjectStore.Get<TwoLayerPerceptron>(inputs.Name);
            var expected = inputs.OutputWeights.Length / (inputs.NumHiddenNodes + 1);
            Assert.Equal(expected, result.NumOutputs);
        }

        [Fact]
        public void ShouldSetInputHiddenWeightsCorrectly()
        {
            var inputs = InvokeMakeTwoLayerPerceptronWithSampleInputs();
            var result = ObjectStore.Get<TwoLayerPerceptron>(inputs.Name);
            result.HiddenWeights.Should().Equal(inputs.HiddenWeights);
        }

        [Fact]
        public void ShouldSetHiddenOutputWeightsCorrectly()
        {
            var inputs = InvokeMakeTwoLayerPerceptronWithSampleInputs();
            var result = ObjectStore.Get<TwoLayerPerceptron>(inputs.Name);
            result.OutputWeights.Should().Equal(inputs.OutputWeights);
        }

        [Fact]
        public void IfNumHiddenWeightsNotMultipleOfHiddenNodes_Throw()
        {
            var inputs = GetSampleInputs();
            var name = inputs.Name;
            Assert.Throws<NNXException>(() => ExcelFunctions.MakeTwoLayerPerceptron(
                                                name, 
                                                inputs.NumHiddenNodes, 
                                                new double[] {1, 2, 3, 4}, 
                                                inputs.OutputWeights));
        }

        [Fact]
        public void IfNumOutputWeightsNotMultipleOfHiddenNodesPlusOne_Throw()
        {
            var inputs = GetSampleInputs();
            var name = inputs.Name;
            Assert.Throws<NNXException>(() => ExcelFunctions.MakeTwoLayerPerceptron(
                                                name,
                                                inputs.NumHiddenNodes,
                                                inputs.HiddenWeights,
                                                new double[] {1, 2, 3, 4, 5}));
        }

        //===========================================================
        private SampleTwoLayerPerceptron InvokeMakeTwoLayerPerceptronWithSampleInputs()
        {
            var inputs = GetSampleInputs();
            ExcelFunctions.MakeTwoLayerPerceptron(inputs.Name, inputs.NumHiddenNodes, inputs.HiddenWeights,
                inputs.OutputWeights);

            return inputs;
        }

        private SampleTwoLayerPerceptron GetSampleInputs()
        {
            return new SampleTwoLayerPerceptron();
        }
    }
}
