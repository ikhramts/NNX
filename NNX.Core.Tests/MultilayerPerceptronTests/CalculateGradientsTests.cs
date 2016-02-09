using System;
using FluentAssertions;
using NNX.Core.Tests.Assertions;
using Xunit;

namespace NNX.Core.Tests.MultilayerPerceptronTests
{
    public class CalculateGradientsTests
    {
        [Fact]
        public void IfInputLengthDoesNotMatchNumInputs_Throw()
        {
            var nn = SampleInputs.GetSample1HiddenLayerPerceptron();
            Action action = () => nn.CalculateGradients(new[] {0.0}, SampleInputs.GetSampleTargets());
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Argument 'inputs' should have width {nn.NumInputs}; was 1.*");
        }

        [Fact]
        public void IfTargetLengthDoesNotMatchNumOutputs_Throw()
        {
            var nn = SampleInputs.GetSample1HiddenLayerPerceptron();
            Action action = () => nn.CalculateGradients(SampleInputs.GetSampleInputs(), new[] { 0.0 });
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Argument 'targets' should have width {nn.NumOutputs}; was 1.*");
        }

        [Fact]
        public void ShouldCalculateGrads_OneHiddenLayerCase()
        {
            // See MultilayerPerceptronTests.xlsx in this folder for calculations of 
            // expected outputs.
            var nn = SampleInputs.GetSample1HiddenLayerPerceptron();
            var result = nn.CalculateGradients(SampleInputs.GetSampleInputs(), SampleInputs.GetSampleTargets());
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(2);

            var expectedHiddenGrads = new[] { 0.00488574841598145, 0.00977149683196289, 0.0488574841598144, 0.00484248141480251, 0.00968496282960501, 0.0484248141480251, 0.00479817184666825, 0.0095963436933365, 0.0479817184666825 };
            var expectedOutputGrads = new[] { -0.185312455982592, -0.191635920938954, -0.197902455875597, -0.550909419851591, 0.185312455982592, 0.191635920938954, 0.197902455875597, 0.550909419851591 };

            result[1].ShouldApproximatelyEqual(expectedOutputGrads, 1e-12);
            result[0].ShouldApproximatelyEqual(expectedHiddenGrads, 1e-12);
        }

        [Fact]
        public void ShouldCalculateGrads_TwoHiddenLayersCase()
        {
            // See MultilayerPerceptronTests.xlsx in this folder for calculations of 
            // expected outputs.
            var nn = SampleInputs.GetSample2HiddenLayerPerceptron();
            var result = nn.CalculateGradients(SampleInputs.GetSampleInputs(), SampleInputs.GetSampleTargets());

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(3);

            var expectedHidden1Grads = new[] { 0.00197922538436067, 0.00395845076872135, 0.0197922538436067, 0.00200603359153654, 0.00401206718307308, 0.0200603359153654, 0.00203160809301684, 0.00406321618603367, 0.0203160809301684 };
            var expectedHidden2Grads = new[] { 0.00976178390929107, 0.0100948877912437, 0.0104249927460735, 0.0290204923445045, 0.00720465406099808, 0.00745050033849661, 0.00769413326721845, 0.0214184835440782 };
            var expectedOutputGrads = new[] { -0.390542035775992, -0.44184361704256, -0.561729160466126, 0.390542035775992, 0.44184361704256, 0.561729160466125 };

            result[2].ShouldApproximatelyEqual(expectedOutputGrads, 1e-12);
            result[1].ShouldApproximatelyEqual(expectedHidden2Grads, 1e-12);
            result[0].ShouldApproximatelyEqual(expectedHidden1Grads, 1e-12);
        }

        [Fact]
        public void RedeclareForLoopVar()
        {
            var x = 0;

            for (var i = 0; i < 2; i++)
                x = i;


            for (var j = 0; j < 3; j++)
            {
                for (var i = 0; i < 2; i++)
                    x = i;

                var first = true;
                for (var i = 0; i < 2; i++)
                {
                    if (first)
                    {
                        Assert.Equal(i, 0);
                        first = false;
                    }
                }

                Assert.Equal(1, x);
                Assert.False(first);
            }

            for (var i = 0; i < 2; i++)
                x = i;

            var firstAgain = true;
            for (var i = 0; i < 2; i++)
            {
                if (firstAgain)
                {
                    Assert.Equal(i, 0);
                    firstAgain = false;
                }
            }

            Assert.Equal(1, x);
            Assert.False(firstAgain);

        }
    }
}
