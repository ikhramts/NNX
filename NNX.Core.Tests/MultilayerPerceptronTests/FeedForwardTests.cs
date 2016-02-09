using System;
using FluentAssertions;
using NNX.Core.Tests.Assertions;
using Xunit;

namespace NNX.Core.Tests.MultilayerPerceptronTests
{
    public class FeedForwardTests
    {
        [Fact]
        public void ShouldCalculateCorrectly_OneHiddenLayer()
        {
            var nn = SampleInputs.GetSample1HiddenLayerPerceptron();
            var result = nn.FeedForward(SampleInputs.GetSampleInputs());
            result.HiddenLayers.Should().HaveCount(1);

            var expectedHidden = new[] { 0.336375544336332, 0.347853774202261, 0.359228665810271, 1.0 };
            result.HiddenLayers[0].ShouldApproximatelyEqual(expectedHidden, 1e-12);

            var expectedOutput = new[] { 0.449090580148409, 0.550909419851591 };
            result.Output.ShouldApproximatelyEqual(expectedOutput, 1e-12);
        }

        [Fact]
        public void ShouldCalculateCorrectly_TwoHiddenLayers()
        {
            var nn = SampleInputs.GetSample2HiddenLayerPerceptron();
            var result = nn.FeedForward(SampleInputs.GetSampleInputs());
            result.HiddenLayers.Should().HaveCount(2);

            var expectedHidden1 = new[] { 0.336375544336332, 0.347853774202261, 0.359228665810271, 1.0 };
            result.HiddenLayers[0].ShouldApproximatelyEqual(expectedHidden1, 1e-12);

            var expectedHidden2 = new[] { 0.695249709756778, 0.78657767504168, 1 };
            result.HiddenLayers[1].ShouldApproximatelyEqual(expectedHidden2, 1e-12);

            var expectedOutput = new[] { 0.438270839533874, 0.561729160466125 };
            result.Output.ShouldApproximatelyEqual(expectedOutput, 1e-12);
        }

        [Fact]
        public void ShouldIncludeInputsWithBias()
        {
            var inputs = SampleInputs.GetSampleInputs();
            var nn = SampleInputs.GetSample1HiddenLayerPerceptron();
            var result = nn.FeedForward(inputs);

            var expected = new double[inputs.Length + 1];
            Array.Copy(inputs, expected, inputs.Length);
            expected[inputs.Length] = 1;
            result.InputWithBias.Should().Equal(expected);
        }

        [Fact]
        public void FeedForward_IfInputLengthDoesNotMatchNumInputs_Throw()
        {
            var nn = SampleInputs.GetSample1HiddenLayerPerceptron();
            Action action = () => nn.FeedForward(new[] { 0.0 });
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Argument 'inputs' should have width {nn.NumInputs}; was 1.*");
        }


    }
}
