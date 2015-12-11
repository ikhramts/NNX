using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace NeuralNetworks.Tests
{
    public class NeuralNetworkConfigTests
    {
        private NeuralNetworkConfig _config;

        public NeuralNetworkConfigTests()
        {
            _config = new NeuralNetworkConfig
            {
                Settings = new Dictionary<string, string>
                {
                    {"string", "b"},
                    {"int", "1"},
                    {"double", "1.5"}
                }
            };
        }

        [Fact]
        public void GetSettingInt_ShouldThrowIfSettingDoesNotExist()
        {
            Assert.Throws<NeuralNetworkException>(() => _config.GetSettingInt("no such"));
        }

        [Theory]
        [InlineData("string")]
        [InlineData("double")]
        public void GetSettingInt_ShouldThrowIfSettingIsNotInt(string settingName)
        {
            Assert.Throws<NeuralNetworkException>(() => _config.GetSettingInt(settingName));
        }

        [Fact]
        public void GetSettingInt_ShouldSucceedIfSettingIsInt()
        {
            var actual = _config.GetSettingInt("int");
            Assert.Equal(1, actual);
        }

        [Fact]
        public void GetSettingDouble_ShouldThrowIfSettingDoesNotExist()
        {
            Assert.Throws<NeuralNetworkException>(() => _config.GetSettingDouble("no such"));
        }

        [Fact]
        public void GetSettingDouble_ShouldThrowIfSettingIsNotDouble()
        {
            Assert.Throws<NeuralNetworkException>(() => _config.GetSettingDouble("string"));
        }

        [Theory]
        [InlineData("int")]
        [InlineData("double")]
        public void GetSettingDouble_ShouldSucceedIfSettingIsDouble(string settingName)
        {
            var expected = double.Parse(_config.Settings[settingName]);
            var actual = _config.GetSettingDouble(settingName);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(">>Not Json")]
        public void FromJson_ThrowIfNotJson(string notJson)
        {
            Assert.Throws<NeuralNetworkException>(() => NeuralNetworkConfig.FromJson(notJson));
        }

        [Fact]
        public void FromJson_ThrowIfMissingNetworkType()
        {
            const string json = "{'name' : 12}";
            Assert.Throws<NeuralNetworkException>(() => NeuralNetworkConfig.FromJson(json));
        }

        [Fact]
        public void FromJson_IfWeightsAreMissing_SetWeightsToNull()
        {
            const string json = "{'NetworkType': 'DummyNetwork'}";
            var config = NeuralNetworkConfig.FromJson(json);
            Assert.NotNull(config);
            Assert.Null(config.Weights);
        }

        [Fact]
        public void FromJson_DeserializeConfig()
        {
            var json = @"{
    'NetworkType': 'DummyNetwork',
    'Settings': {
        'first': 'firstString',
        'second': 2
    }
}
";
            var config = NeuralNetworkConfig.FromJson(json);
            config.Should().NotBeNull();
            config.NetworkType.Should().BeEquivalentTo("DummyNetwork");
            config.Settings.Should().NotBeNull();
            config.Settings.Should().HaveCount(2);
            config.Settings.Should().ContainKey("first");
            config.Settings.Should().ContainKey("second");

            config.Settings["first"].ShouldBeEquivalentTo("firstString");
            config.Settings["second"].ShouldBeEquivalentTo("2");
        }
    }
}
