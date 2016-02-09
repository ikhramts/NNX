using FluentAssertions;
using NNX.Core.Training;
using Xunit;

namespace NNX.Core.Tests.Training.UntilDoneGradientTrainerTests
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
