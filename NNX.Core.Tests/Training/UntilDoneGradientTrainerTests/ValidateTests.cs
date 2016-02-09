using System;
using FluentAssertions;
using NNX.Core.Training;
using Xunit;

namespace NNX.Core.Tests.Training.UntilDoneGradientTrainerTests
{
    public class ValidateTests
    {
        private UntilDoneGradientTrainer _trainer;

        public ValidateTests()
        {
            _trainer = new UntilDoneGradientTrainer
            {
                LearningRate = 0.1,
                NumEpochs = 100,
                ValidationSetFraction = 0.3,
                MaxEpochsWithoutImprovement = 10,
            };
        }

        [Fact]
        public void IfValid_ShouldDoNothing()
        {
            _trainer.Validate();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void IfLearningRateNotPositive_Throw(double bad)
        {
            _trainer.LearningRate = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property LearningRate must be positive; was {bad}*");
        }

        [Fact]
        public void IfMomentumNegative_Throw()
        {
            const double bad = -0.2;
            _trainer.Momentum = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property Momentum cannot be negative; was {bad}*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void IfNumEpochsIsNotPositive_Throw(int badNumEpochs)
        {
            _trainer.NumEpochs = badNumEpochs;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property NumEpochs must be positive; was {badNumEpochs}*");
        }

        [Fact]
        public void IfQuadraticRegularizationNegative_Throw()
        {
            const double bad = -0.1;
            _trainer.QuadraticRegularization = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property QuadraticRegularization cannot be negative; was {bad}*");
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void IfValidationSetFractionNotBetween0And1_Throw(double bad)
        {
            _trainer.ValidationSetFraction = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property ValidationSetFraction must be strictly between 0 and 1; was {bad}*");
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(0)]
        public void IfMaxEpochsWithoutImprovementNotPositive_Throw(int bad)
        {
            _trainer.MaxEpochsWithoutImprovement = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property MaxEpochsWithoutImprovement must not be negative; was {bad}*");

        }

        [Theory]
        [InlineData(-2)]
        [InlineData(0)]
        public void IfEpochsBetweenValidationsNotPositive_Throw(int bad)
        {
            _trainer.EpochsBetweenValidations = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Property EpochsBetweenValidations must be positive; was {bad}*");

        }
    }
}
