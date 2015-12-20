using System;
using FluentAssertions;
using Xunit;

namespace NeuralNetworks.Tests
{
    public class TwoLayerPerceptronTests
    {
        private const int Precision = 12;

        [Fact]
        public void ConstructorShouldCreateCorrectArrays()
        {
            var nn = new TwoLayerPerceptron(1, 2, 4);
            Assert.Equal(1, nn.NumInputs);
            Assert.Equal(2, nn.NumHidden);
            Assert.Equal(4, nn.NumOutputs);

            Assert.Equal(4, nn.HiddenWeights.Length);
            Assert.Equal(12, nn.OutputWeights.Length);
        }

        [Fact]
        public void ConstructorFromConfig_ShouldConstructCorrectly()
        {
            var config = GetSampleConfig();
            var nn = new TwoLayerPerceptron(config);
            Assert.Equal(config.GetSettingInt("NumInputs"), nn.NumInputs);
            Assert.Equal(config.GetSettingInt("NumHidden"), nn.NumHidden);
            Assert.Equal(config.GetSettingInt("NumOutputs"), nn.NumOutputs);

            foreach (var weight in nn.HiddenWeights)
                Assert.Equal(0.5, weight);

            foreach (var weight in nn.OutputWeights)
                Assert.Equal(0.6, weight);
        }

        [Fact]
        public void ConstructorFromConfig_ThrowIfMissingNumInputs()
        {
            var config = GetSampleConfig();
            config.Settings.Remove("NumInputs");
            Assert.Throws<NeuralNetworkException>(() => new TwoLayerPerceptron(config));
        }

        [Fact]
        public void ConstructorFromConfig_ThrowIfMissingNumHidden()
        {
            var config = GetSampleConfig();
            config.Settings.Remove("NumHidden");
            Assert.Throws<NeuralNetworkException>(() => new TwoLayerPerceptron(config));
        }

        [Fact]
        public void ConstructorFromConfig_ThrowIfMissingNumOutputs()
        {
            var config = GetSampleConfig();
            config.Settings.Remove("NumOutputs");
            Assert.Throws<NeuralNetworkException>(() => new TwoLayerPerceptron(config));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void ConstructorFromConfig_ThrowIfNumWeightSubsetsIsWrong(int numSubsets)
        {
            var config = GetSampleConfig();
            var weights = new double[numSubsets][];
            Array.Copy(config.Weights, weights, Math.Min(config.Weights.Length, weights.Length));

            for (var i = 2; i < weights.Length; i++)
                weights[i] = new[] {1.0};

            config.Weights = weights;
            Assert.Throws<NeuralNetworkException>(() => new TwoLayerPerceptron(config));
        }

        [Fact]
        public void ConstructorFromConfig_InitializeRandomWeightsIfWeightsAreNull()
        {
            var config = GetSampleConfig();
            config.Weights = null;
            var nn = new TwoLayerPerceptron(config);

            Assert.NotNull(nn);
            Assert.NotNull(nn.Weights);
            Assert.NotNull(nn.HiddenWeights);
            Assert.NotNull(nn.OutputWeights);

            var hiddenWeightsNotAllZero = false;
            foreach (var weight in nn.HiddenWeights)
                hiddenWeightsNotAllZero |= weight != 0;

            Assert.True(hiddenWeightsNotAllZero);

            var outputWeightsNotAllZero = false;
            foreach (var weight in nn.OutputWeights)
                outputWeightsNotAllZero |= weight != 0;

            Assert.True(outputWeightsNotAllZero);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, -1)]
        [InlineData(1, 1)]
        [InlineData(1, -1)]
        public void ConstructorFromConfig_ThrowIfWeightSubsetSizeIsWrong(int incorrectSubset, int itemsToAdd)
        {
            var config = GetSampleConfig();
            var victimSubset = config.Weights[incorrectSubset];
            config.Weights[incorrectSubset] = new double[victimSubset.Length + itemsToAdd];

            Assert.Throws<NeuralNetworkException>(() => new TwoLayerPerceptron(config));
        }

        [Fact]
        public void FeedForward_ShouldMakeOutput()
        {
            var nn = GetNeuralNetwork();

            var expectedOutput = new double[3];
            expectedOutput[0] = 0.18280661189982;
            expectedOutput[1] = 0.305765321819331;
            expectedOutput[2] = 0.511428066280849;

            var result = nn.FeedForward(GetSampleInputs());

            Assert.Equal(expectedOutput.Length, result.Output.Length);

            for (var k = 0; k < expectedOutput.Length; k++)
                Assert.Equal(expectedOutput[k], result.Output[k], Precision);
        }

        [Fact]
        public void FeedForward_ShouldProduceHiddenNodeValues()
        {
            var nn = GetNeuralNetwork();
            var expectedHidden = new double[7];
            expectedHidden[0] = 0.664036770267849;
            expectedHidden[1] = 0.675069874838608;
            expectedHidden[2] = 0.685809062229095;
            expectedHidden[3] = 0.696257672686682;
            expectedHidden[4] = 0.706419320397235;
            expectedHidden[5] = 0.716297870199025;
            expectedHidden[6] = 1;

            var result = nn.FeedForward(GetSampleInputs());

            result.HiddenLayers.Should().NotBeNullOrEmpty();
            result.HiddenLayers.Should().HaveCount(1);

            var actualHidden = result.HiddenLayers[0];

            actualHidden.Should().HaveCount(expectedHidden.Length);

            for (var k = 0; k < expectedHidden.Length; k++)
                Assert.Equal(expectedHidden[k], actualHidden[k], Precision);

        }

        [Fact]
        public void FeedForward_IfInputLengthDoesNotMatchNumInputs_Throw()
        {
            var nn = GetNeuralNetwork();
            Action action = () => nn.FeedForward(new[] {0.0});
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Expected input length to be {nn.NumInputs} but got 1.*");
        }

        [Fact]
        public void CalculateGrads_ShouldCalculateGrads()
        {
            var expectedOutputWeightGrads = new[]						
            {						
                0.121390312149565, 	0.123407236614882, 	0.125370431076294, 	0.127280506153106, 	0.129138122542392, 	0.130943986762141, 	0.18280661189982, 
                0.203039416760818, 	0.206412957530562, 	0.209696628619093, 	0.212891451358221, 	0.215998530840654, 	0.219019048799906, 	0.305765321819331, 
                -0.324429728910383, 	-0.329820194145444, 	-0.335067059695386, 	-0.340171957511328, 	-0.345136653383046, 	-0.349963035562047, 	-0.488571933719151
            };

            var expectedHiddenWeightGrads = new[]						
            {						
                -0.00375337645432844, 	-0.00750675290865689, 	-0.0112601293629853, 	-0.0150135058173138, 	-0.0375337645432844, 		
                -0.00365418360662175, 	-0.00730836721324349, 	-0.0109625508198652, 	-0.014616734426487, 	-0.0365418360662175, 		
                -0.00355606341857728, 	-0.00711212683715456, 	-0.0106681902557318, 	-0.0142242536743091, 	-0.0355606341857728, 		
                -0.00345911181176317, 	-0.00691822362352634, 	-0.0103773354352895, 	-0.0138364472470527, 	-0.0345911181176317, 		
                -0.00336341680728173, 	-0.00672683361456347, 	-0.0100902504218452, 	-0.0134536672291269, 	-0.0336341680728173, 		
                -0.00326905869764401, 	-0.00653811739528801, 	-0.00980717609293202, 	-0.013076234790576, 	-0.0326905869764401
           };

            var nn = GetNeuralNetwork();
            var result = nn.CalculateGradients(GetSampleInputs(), GetSampleTargets());

            Assert.NotNull(result);
            Assert.Equal(2, result.Length);

            var resultHiddenGrads = result[0];
            var resultOutputGrads = result[1];
            Assert.Equal(expectedOutputWeightGrads.Length, resultOutputGrads.Length);
            Assert.Equal(expectedHiddenWeightGrads.Length, resultHiddenGrads.Length);

            for (var jk = 0; jk < expectedOutputWeightGrads.Length; jk++)
                Assert.Equal(expectedOutputWeightGrads[jk], resultOutputGrads[jk], Precision);

            for (var ij = 0; ij < expectedHiddenWeightGrads.Length; ij++)
                Assert.Equal(expectedHiddenWeightGrads[ij], resultHiddenGrads[ij], Precision);
        }

        [Fact]
        public void CalculateGrads_IfInputLengthNotEqualNumInputs_Throw()
        {
            var nn = GetNeuralNetwork();
            Action action = () => nn.CalculateGradients(new[] { 0.0 }, GetSampleTargets());
            action.ShouldThrow<NeuralNetworkException>();
        }

        [Fact]
        public void CalculateGrads_IfTargetLengthNotEqualNumOutputs_Throw()
        {
            var nn = GetNeuralNetwork();
            Action action = () => nn.CalculateGradients(GetSampleInputs(), new[] { 0.0 });
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Expected target length to be {nn.NumOutputs} but got 1.*");

        }

        private static TwoLayerPerceptron GetNeuralNetwork()
        {
            // Setup generated from NeuralNetworksTests.xlsx in this folder.
            var nn = new TwoLayerPerceptron(4, 6, 3);

            nn.HiddenWeights[0] = 0.1; nn.HiddenWeights[1] = 0.2; nn.HiddenWeights[2] = 0.3; nn.HiddenWeights[3] = 0.4; nn.HiddenWeights[4] = 0.5;
            nn.HiddenWeights[5] = 0.11; nn.HiddenWeights[6] = 0.21; nn.HiddenWeights[7] = 0.31; nn.HiddenWeights[8] = 0.41; nn.HiddenWeights[9] = 0.51;
            nn.HiddenWeights[10] = 0.12; nn.HiddenWeights[11] = 0.22; nn.HiddenWeights[12] = 0.32; nn.HiddenWeights[13] = 0.42; nn.HiddenWeights[14] = 0.52;
            nn.HiddenWeights[15] = 0.13; nn.HiddenWeights[16] = 0.23; nn.HiddenWeights[17] = 0.33; nn.HiddenWeights[18] = 0.43; nn.HiddenWeights[19] = 0.53;
            nn.HiddenWeights[20] = 0.14; nn.HiddenWeights[21] = 0.24; nn.HiddenWeights[22] = 0.34; nn.HiddenWeights[23] = 0.44; nn.HiddenWeights[24] = 0.54;
            nn.HiddenWeights[25] = 0.15; nn.HiddenWeights[26] = 0.25; nn.HiddenWeights[27] = 0.35; nn.HiddenWeights[28] = 0.45; nn.HiddenWeights[29] = 0.55;

            nn.OutputWeights[0] = 1.1; nn.OutputWeights[1] = 1.11; nn.OutputWeights[2] = 1.12; nn.OutputWeights[3] = 1.13; nn.OutputWeights[4] = 1.14; nn.OutputWeights[5] = 1.15; nn.OutputWeights[6] = 1.16;
            nn.OutputWeights[7] = 1.2; nn.OutputWeights[8] = 1.21; nn.OutputWeights[9] = 1.22; nn.OutputWeights[10] = 1.23; nn.OutputWeights[11] = 1.24; nn.OutputWeights[12] = 1.25; nn.OutputWeights[13] = 1.26;
            nn.OutputWeights[14] = 1.3; nn.OutputWeights[15] = 1.31; nn.OutputWeights[16] = 1.32; nn.OutputWeights[17] = 1.33; nn.OutputWeights[18] = 1.34; nn.OutputWeights[19] = 1.35; nn.OutputWeights[20] = 1.36;

            return nn;
        }

        private static double[] GetSampleInputs() => new[] {0.1, 0.2, 0.3, 0.4};
        private static double[] GetSampleTargets() => new[] { 0.0, 0.0, 1.0 };

        [Fact]
        public void GetConfig()
        {
            var nn = new TwoLayerPerceptron(2, 3, 4);
            for (var ij = 0; ij < nn.HiddenWeights.Length; ij++)
                nn.HiddenWeights[ij] = 0.5;

            for (var jk = 0; jk < nn.OutputWeights.Length; jk++)
                nn.OutputWeights[jk] = 0.6;


            var config = nn.GetConfig();
            var expected = GetSampleConfig();
            
            Assert.Equal(expected.NetworkType, config.NetworkType);
            Assert.Equal(expected.GetSettingInt("NumInputs"), config.GetSettingInt("NumInputs"));
            Assert.Equal(expected.GetSettingInt("NumHidden"), config.GetSettingInt("NumHidden"));
            Assert.Equal(expected.GetSettingInt("NumOutputs"), config.GetSettingInt("NumOutputs"));

            Assert.Equal(expected.Weights[0], config.Weights[0]);
            Assert.Equal(expected.Weights[1], config.Weights[1]);
        }

        public static NeuralNetworkConfig GetSampleConfig()
        {
            return TestObjects.NeuralNetworkConfigObjects.GetTwoLayerPerceptronConfig();
        }
    }
}
