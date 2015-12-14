using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class GetWeightsTests : ObjectStoreTestBase
    {
        [Theory]
        [MemberData("ShouldGetWeightsForLayer_Inputs")]
        public void ShouldGetWeightsForLayer(SampleTwoLayerPerceptron input, int layerNum, double[,] expected)
        {
            var name = input.Name;
            ExcelFunctions.MakeTwoLayerPerceptron(name, input.NumHiddenNodes, input.HiddenWeights, input.OutputWeights);
            var result = ExcelFunctions.GetWeights(name, layerNum);
            result.Should().BeAssignableTo<double[,]>();
            Assert.Equal(expected, result);
        }

        // ReSharper disable once UnusedMember.Local
        private static IEnumerable<object[]> ShouldGetWeightsForLayer_Inputs()
        {
            var samplePerceptron = new SampleTwoLayerPerceptron();

            return new[]
            {
                new object[] {samplePerceptron, 1, samplePerceptron.HiddenWeights.ToVertical2DArray()},
                new object[] {samplePerceptron, 2, samplePerceptron.OutputWeights.ToVertical2DArray()},
            };
        }

        [Fact]
        public void IfNeuralNetDoesNotExist_Throw()
        {
            Assert.Throws<NNXException>(() => ExcelFunctions.GetWeights("nosuch", 2));
        }

        [Fact]
        public void IfObjectIsNotNeuralNet_Throw()
        {
            ObjectStore.Add("x", "y");
            Assert.Throws<NNXException>(() => ExcelFunctions.GetWeights("x", 2));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void IfLayerNumNotPositive_Throw(int badLayer)
        {
            var inputs = new SampleTwoLayerPerceptron();
            ExcelFunctions.MakeTwoLayerPerceptron(inputs.Name, inputs.NumHiddenNodes, inputs.HiddenWeights,
                inputs.OutputWeights);
            Assert.Throws<NNXException>(() => ExcelFunctions.GetWeights(inputs.Name, badLayer));
        }

        [Fact]
        public void IfLayerHigherThanNumLayers_Throw()
        {
            var inputs = new SampleTwoLayerPerceptron();
            ExcelFunctions.MakeTwoLayerPerceptron(inputs.Name, inputs.NumHiddenNodes, inputs.HiddenWeights,
                inputs.OutputWeights);
            Assert.Throws<NNXException>(() => ExcelFunctions.GetWeights(inputs.Name, 3));
        }

    }
}
