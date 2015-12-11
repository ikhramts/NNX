using Xunit;

namespace nnet.Tests
{
    public class TrainCommandTests
    {
        [Fact]
        public void Construct_FieldsShouldNotBeNull()
        {
            var command = new TrainCommand();
            Assert.NotNull(command.Name);
            Assert.NotNull(command.Description);
            Assert.NotNull(command.HelpMessage);
        }

        [Fact]
        public void Run_OnNoArgs_ReturnError()
        {
            var errors = new TrainCommand().Run(new string[] {});
            Assert.NotNull(errors);
            Assert.NotEmpty(errors);
        }
    }
}
