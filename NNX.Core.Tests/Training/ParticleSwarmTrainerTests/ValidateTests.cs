using System;
using FluentAssertions;
using NNX.Core.Training;
using Xunit;

namespace NNX.Core.Tests.Training.ParticleSwarmTrainerTests
{
    public class ValidateTests
    {
        private ParticleSwarmTrainer _trainer = GetTrainer();

        [Fact]
        public void IfValid_ShouldNotThrow()
        {
            _trainer.Validate();
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(0)]
        public void IfNumParticlesNotPositive_Throw(int bad)
        {
            _trainer.NumParticles = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Parameter NumParticles must be positive; was {bad}.*");
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(0)]
        public void IfNumEpochsNotPositive_Throw(int bad)
        {
            _trainer.NumEpochs = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Parameter NumEpochs must be positive; was {bad}.*");
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(0)]
        public void IfNumThreadsNotPositive_Throw(int bad)
        {
            _trainer.NumThreads = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Parameter NumThreads must be positive; was {bad}.*");
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(0)]
        public void IfBatchSizeNotPositive_Throw(int bad)
        {
            _trainer.BatchSize = bad;
            Action action = () => _trainer.Validate();
            action.ShouldThrow<NeuralNetworkException>()
                .WithMessage($"*Parameter BatchSize must be positive; was {bad}.*");
        }

        public static ParticleSwarmTrainer GetTrainer()
        {
            var trainer = new ParticleSwarmTrainer
            {
                NumParticles = 1000,
                NumEpochs = 100,
                BatchSize = 1,
            };

            return trainer;
        }
    }
}
