using FluentAssertions;
using NNX.NeuralNetwork;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class MakeTwoLayerPerceptronConfigTests : ObjectStoreTestBase
    {
        [Fact]
        public void ShouldCreateConfigObject()
        {
            ExcelFunctions.MakeTwoLayerPerceptronConfig("obj", 5);
            var result = ObjectStore.Get<TwoLayerPerceptronConfig>("obj");
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldStoreNumHiddenNodes()
        {
            ExcelFunctions.MakeTwoLayerPerceptronConfig("obj", 5);
            var result = ObjectStore.Get<TwoLayerPerceptronConfig>("obj");
            result.NumHiddenNodes.ShouldBeEquivalentTo(5);
        }

        [InlineData(0)]
        [InlineData(-3)]
        public void IfNumHiddenNodesNotPositive_Throw(int numHiddenNodes)
        {
            Assert.Throws<NNXException>(() => ExcelFunctions.MakeTwoLayerPerceptronConfig("obj", numHiddenNodes));
        }

        [Fact]
        public void ShouldReturnProvidedName()
        {
            var name = ExcelFunctions.MakeTwoLayerPerceptronConfig("obj", 5);
            name.ShouldBeEquivalentTo("obj");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData((string)null)]
        public void IfNameIsNullOrWhitespace_Throw(string badName)
        {
            Assert.Throws<NNXException>(() => ExcelFunctions.MakeTwoLayerPerceptronConfig(badName, 5));
        }

    }
}
