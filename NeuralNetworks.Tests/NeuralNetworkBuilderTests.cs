using System;
using FluentAssertions;
using NeuralNetworks.Tests.TestObjects;
using Xunit;

namespace NeuralNetworks.Tests
{
    public class NeuralNetworkBuilderTests
    {
        [Fact]
        public void Build_ThrowIfNeuralNetworkTypeDoesNotExist()
        {
            var config = NeuralNetworkConfigObjects.GetTwoLayerPerceptronConfig();
            config.NetworkType = "NoSuch";
            Assert.Throws<NeuralNetworkException>(() => NeuralNetworkBuilder.Build(config));
        }

        [Fact]
        public void Build_BuildNeuralNet()
        {
            var config = NeuralNetworkConfigObjects.GetTwoLayerPerceptronConfig();
            var nn = NeuralNetworkBuilder.Build(config);
            Assert.NotNull(nn);
            nn.Should().BeOfType<TwoLayerPerceptron>();
        }

        [Fact]
        public void Build_IfNull_ThrowNullArgumentException()
        {
            var builder = new NeuralNetworkBuilder();
            Assert.Throws<ArgumentNullException>(() => builder.CustomBuild(null));
        }
    }
}
