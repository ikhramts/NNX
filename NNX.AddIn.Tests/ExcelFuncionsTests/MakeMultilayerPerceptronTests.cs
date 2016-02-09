using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NNX.Core;
using Xunit;

namespace NNX.AddIn.Tests.ExcelFuncionsTests
{
    public class MakeMultilayerPerceptronTests : ObjectStoreTestBase
    {
        public MakeMultilayerPerceptronTests()
        {
            var weights = new[]
            {
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1,},
            };

            ObjectStore.Add("weights", weights);
        }

        [Fact]
        public void ShouldMakeMultilayerPerceptron()
        {
            ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, "weights");
            var result = ObjectStore.Get<MultilayerPerceptron>("x");
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnObjectName()
        {
            var name = ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, "weights");
            name.Should().Be("x");
        }

        [Fact]
        public void ShouldSetNumInputs()
        {
            ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, "weights");
            var result = ObjectStore.Get<MultilayerPerceptron>("x");
            result.NumInputs.Should().Be(2);
        }

        [Fact]
        public void ShouldSetNumOutputs()
        {
            ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, "weights");
            var result = ObjectStore.Get<MultilayerPerceptron>("x");
            result.NumOutputs.Should().Be(3);
        }

        [Fact]
        public void ShouldSetHiddenLayerSizes()
        {
            ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, "weights");
            var result = ObjectStore.Get<MultilayerPerceptron>("x");
            result.HiddenLayerSizes.Should().Equal(4, 2);
        }

        [Fact]
        public void ShouldSetWeights()
        {
            ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, "weights");
            var result = ObjectStore.Get<MultilayerPerceptron>("x");
            result.Weights[0][0].Should().Be(1.0);
        }

        [Theory]
        [MemberData("IfWeightsArgDoesNotReferToWeights_Throw_Cases")]
        public void IfWeightsArgDoesNotReferToWeights_Throw(string name, object weights)
        {
            if (weights != null)
                ObjectStore.Add(name, weights);

            Action action = () => ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, name);
            action.ShouldThrow<NNXException>()
                .WithMessage("*Argument Weights should be a weights object created using nnMakeWeights().*");
        }

        public static IEnumerable<object[]> IfWeightsArgDoesNotReferToWeights_Throw_Cases()
        {
            return new[]
            {
                new object[] {"null", null},
                new object[] {"bad", new double[] {1, 2}},
                new object[] {"bad", "bad"},
            };
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        public void IfWeightsHaveWrongNumberOfLayers_Throw(int numLayers)
        {
            var weightsBase = new[]
            {
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1,},
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1,},
            };

            ObjectStore.Add("weights", weightsBase.Take(numLayers).ToArray());

            Action action = () => ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, "weights");
            action.ShouldThrow<NNXException>()
                .WithMessage($"*Argument Weights was expected to have 3 layers; had: {numLayers}.*");

        }

        [Fact]
        public void IfWrongNumberOfWeightsInALayer_Throw()
        {
            var weights = new[]
            {
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,},
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1,},
                new double[] {1, 1, 1, 1, 1, 1, 1, 1, 1,},
            };

            ObjectStore.Add("weights", weights);

            Action action = () => ExcelFunctions.MakeMultilayerPerceptron("x", 2, 3, new[] {4.0, 2.0}, "weights");
            action.ShouldThrow<NNXException>()
                .WithMessage("*Argument Weights was expected to have 10 values in layer 2; had: 9.*");
        }
    }
}
