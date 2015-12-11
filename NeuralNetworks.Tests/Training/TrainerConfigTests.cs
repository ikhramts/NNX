using NeuralNetworks.Training;
using Newtonsoft.Json;
using Xunit;

namespace NeuralNetworks.Tests.Training
{
    public class TrainerConfigTests
    {
        [Fact]
        public void FromJson_Convert()
        {
            var json = "{'LearningRate': 2}";
            var config = TrainerConfig.FromJson(json);
            Assert.Equal(2, config.LearningRate);
        }

        [Theory]
        [InlineData("{'LearningRate': 'two'}")]
        [InlineData("'LearningRate': 2}")]
        public void FromJson_OnBadJson_ShouldWrapInNeuralNetworkException(string badJson)
        {
            var caughtException = false;
            try
            {
                TrainerConfig.FromJson(badJson);
            }
            catch (NeuralNetworkException ex)
            {
                caughtException = true;
                Assert.NotNull(ex.InnerException);
                Assert.True(ex.InnerException is JsonSerializationException);
            }

            Assert.True(caughtException);
        }

        [Fact]
        public void ToJson_ShouldConvert()
        {
            var config = new TrainerConfig { LearningRate = 2 };
            var json = config.ToJson();
            Assert.NotNull(json);
            Assert.NotEqual(0, json.Length);
        }
    }
}
