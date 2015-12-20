using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace NeuralNetworks.Tests.MultilayerPerceptronTests
{
    public class DirectConstructorTests
    {
        [Fact]
        public void ShouldInitializeNumInputs()
        {
            var nn = new MultilayerPerceptron(1, 2, new[] {3});
            Assert.Equal(1, nn.NumInputs);
        }

        [Fact]
        public void ShouldInitializeNumOutputs()
        {
            var nn = new MultilayerPerceptron(1, 2, new[] { 3 });
            Assert.Equal(2, nn.NumOutputs);
        }

        [Fact]
        public void ShouldInitializeHiddenLayerSizes()
        {
            var hidden = new[] {2, 3, 2};
            var nn = new MultilayerPerceptron(1, 2, hidden);
            nn.HiddenLayerSizes.Should().Equal(hidden);
        }

        [Theory]
        [MemberData("ShouldInitializeWeight_Cases")]
        public void ShouldInitializeWeights(int numInputs, int numOutputs, int[] numHidden, int[] expectedWeightSizes)
        {
            var nn = new MultilayerPerceptron(numInputs, numOutputs, numHidden);
            nn.Weights.Should().NotBeNullOrEmpty();
            nn.Weights.Should().HaveCount(expectedWeightSizes.Length);

            for (int i = 0; i < nn.Weights.Length; i++)
            {
                nn.Weights[i].Should().HaveCount(expectedWeightSizes[i], $"Difference in layer {i}.");
            }
        }

        public static IEnumerable<object[]> ShouldInitializeWeight_Cases()
        {
            yield return new object[] { 2, 3, new[] { 2 }, new[] { 6, 9 } };
            yield return new object[] { 1, 2, new[] { 2, 3 }, new[] { 4, 9, 8 } };
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(0)]
        public void IfNumInputsNotPositive_Throw(int bad)
        {
            Action action = () => new MultilayerPerceptron(bad, 2, new[] { 3 });
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Argument numInputs must be positive; was {bad}.*");
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(0)]
        public void IfNumOutputsNotPositive_Throw(int bad)
        {
            Action action = () => new MultilayerPerceptron(1, bad, new[] { 3 });
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Argument numOutputs must be positive; was {bad}.*");
        }

        [Theory]
        [MemberData("IfAnyHiddenLayerSizeNotPositive_Throw_Cases")]
        public void IfAnyHiddenLayerSizeNotPositive_Throw(int[] numHidden, int badLayer)
        {
            Action action = () => new MultilayerPerceptron(1, 2, numHidden);
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage(
                    $"*Argument hiddenLayerSizes must contain only positive values; was {numHidden[badLayer]} " +
                    $"at index {badLayer}.*");
        }

        public static IEnumerable<object> IfAnyHiddenLayerSizeNotPositive_Throw_Cases()
        {
            yield return new object[] { new[] { -1, 2 }, 0 };
            yield return new object[] { new[] { 0, 2 }, 0 };
            yield return new object[] { new[] { 1, -2 }, 1 };
            yield return new object[] { new[] { 1, 0 }, 1 };
            yield return new object[] { new[] { 0, -1 }, 0 };
        }

        [Theory]
        [MemberData("IfHiddenLayerSizesIsNullOrEmpty_Throw_Cases")]
        public void IfHiddenLayerSizesIsNullOrEmpty_Throw(int[] numHidden)
        {
            Action action = () => new MultilayerPerceptron(1, 2, numHidden);
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage("*Argument hiddenLayerSizes cannot be null or empty.*");
        }

        public static IEnumerable<object> IfHiddenLayerSizesIsNullOrEmpty_Throw_Cases()
        {
            return new[]
            {
                new object[] {null},
                new object[] {new int[] {}}
            };
        }


    }
}
