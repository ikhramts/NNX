using FluentAssertions;
using NeuralNetworks.Training;
using Xunit;

namespace NeuralNetworks.Tests.Training.UntilDoneGradientTrainerTests
{
    public class GetValidationSetFractionTests
    {
        [Fact]
        public void ShouldReturnValidationSetFraction()
        {
            var trainer = new UntilDoneGradientTrainer {ValidationSetFraction = 0.3};
            trainer.GetValidationSetFraction().Should().Be(0.3);
        }
    }
}
