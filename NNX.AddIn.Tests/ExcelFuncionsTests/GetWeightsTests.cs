using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NNX.Core;
using Xunit;

namespace NNX.AddIn.Tests.ExcelFuncionsTests
{
    public class GetWeightsTests : ObjectStoreTestBase
    {
        private const string Name = "Percy";

        public GetWeightsTests()
        {
            var mockNN = new Mock<INeuralNetwork>();
            var weights = new[]
            {
                new double[] {1, 2, 3, 4, 5, 6, 7, 8, 9},
                new double[] {1, 2, 3, 6, 5, 6, 7, 8}
            };

            mockNN.SetupGet(n => n.Weights).Returns(weights);
            ObjectStore.Add(Name, mockNN.Object);
        }

        [Theory]
        [MemberData("ShouldGetWeightsForLayer_Inputs")]
        public void ShouldGetWeightsForLayer(int layerNum, double[] expected)
        {
            var result = ExcelFunctions.GetWeights(Name, layerNum);
            result.Should().BeAssignableTo<double[,]>();
            Assert.Equal(expected.ToVertical2DArray(), result);
        }

        // ReSharper disable once UnusedMember.Local
        private static IEnumerable<object[]> ShouldGetWeightsForLayer_Inputs()
        {
            var layer1 = new double[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
            var layer2 = new double[] {1, 2, 3, 6, 5, 6, 7, 8};

            return new[]
            {
                new object[] { 1, layer1},
                new object[] { 2, layer2},
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
            Assert.Throws<NNXException>(() => ExcelFunctions.GetWeights(Name, badLayer));
        }

        [Fact]
        public void IfLayerHigherThanNumLayers_Throw()
        {
            Assert.Throws<NNXException>(() => ExcelFunctions.GetWeights(Name, 3));
        }

    }
}
