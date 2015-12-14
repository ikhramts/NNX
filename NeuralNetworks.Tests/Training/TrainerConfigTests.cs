using System;
using FluentAssertions;
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

        [Fact]
        public void Validate_IfValid_ShouldDoNothing()
        {
            var config = new TrainerConfig {LearningRate = 0.1, NumEpochs = 100};
            config.Validate();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void Validate_IfLearningRateNotPositive_Throw(double badLearnignRate)
        {
            var config = new TrainerConfig { LearningRate = badLearnignRate, NumEpochs = 100 };
            Action action = () => config.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property LearningRate must be positive; was {badLearnignRate}*");
        }

        [Fact]
        public void Validate_IfMomentumNegative_Throw()
        {
            const double badMomentum = -0.2;
            var config = new TrainerConfig { LearningRate = 0.1, NumEpochs = 100, Momentum = badMomentum };
            Action action = () => config.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property Momentum cannot be negative; was {badMomentum}*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void Validate_IfNumEpochsIsNotPositive_Throw(int badNumEpochs)
        {
            var config = new TrainerConfig { LearningRate = 0.1, NumEpochs = badNumEpochs };
            Action action = () => config.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property NumEpochs must be positive; was {badNumEpochs}*");
        }

        [Fact]
        public void Validate_IfQuadraticRegularizationNegative_Throw()
        {
            const double bad = -0.1;
            var config = new TrainerConfig { LearningRate = 0.1, NumEpochs = 100, QuadraticRegularization = bad };
            Action action = () => config.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property QuadraticRegularization cannot be negative; was {bad}*");
        }

    }
}
