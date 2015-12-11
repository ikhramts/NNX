using System;
using System.IO;
using Moq;
using Xunit;

namespace nnet.Tests
{
    public class ProgramTests
    {
        private readonly StringWriter _stdout = new StringWriter();
        private readonly StringWriter _stderr = new StringWriter();

        public ProgramTests()
        {
            Console.SetOut(_stdout);
            Console.SetError(_stderr);
        }

        [Fact]
        public void TestTest_StdErrStartsEmpty()
        {
            _stderr.Flush();
            var contents = _stderr.ToString();
            Assert.Equal(0, contents.Length);
        }

        [Fact]
        public void TestTest_StdErrAcceptsOutout()
        {
            Console.Error.WriteLine("Error");
            _stderr.Flush();
            var contents = _stderr.ToString();
            Assert.Equal("Error\r\n", contents);
        }

        [Fact]
        public void TestTest_StdOutStartsEmpty()
        {
            _stdout.Flush();
            var contents = _stdout.ToString();
            Assert.Equal(0, contents.Length);
        }

        [Fact]
        public void Main_OnInvalidCommand_ShouldShowError()
        {
            Program.Main(new[] { "nosuch" });
            _stderr.Flush();
            Assert.NotEqual(0, _stderr.ToString().Length);
        }

        [Fact]
        public void Main_OnInvalidCommand_ShouldReturn1()
        {
            var result = Program.Main(new[] { "nosuch" });
            Assert.Equal(1, result);
        }

        [Fact]
        public void Main_OnNoArgs_ShouldShowUsageAndAvailableCommands()
        {
            Program.AvailableCommands = new[] { GetGoodCommand() };

            Program.Main(new string[] {});

            _stderr.Flush();
            var contents = _stderr.ToString();
            Assert.True(contents.Contains("Usage"));
            Assert.True(contents.Contains("testcommand"));
            Assert.True(contents.Contains("help"));
        }

        [Fact]
        public void Main_OnNoArgsShould_Return1()
        {
            var result = Program.Main(new string[] { });
            Assert.Equal(1, result);
        }

        [Fact]
        public void Main_OnGoodCommand_ShouldRunSelectedCommand()
        {
            string[] sentArgs = null;
            var commandMock = new Mock<IConsoleCommand>();
            commandMock
                .Setup(c => c.Run(It.IsAny<string[]>()))
                .Callback((string[] args) => sentArgs = args);
            commandMock.SetupGet(c => c.Name).Returns("testcommand");

            var otherCommand = new Mock<IConsoleCommand>();
            otherCommand.SetupGet(c => c.Name).Returns("otherCommand");

            Program.AvailableCommands = new[] { otherCommand.Object, commandMock.Object };

            Program.Main(new [] { "testcommand", "arg1", "arg2"});

            _stderr.Flush();
            Assert.Equal(0, _stderr.ToString().Length);
            var expected = new[] {"arg1", "arg2"};
            Assert.NotNull(sentArgs);
            Assert.Equal(expected, sentArgs);
        }

        [Fact]
        public void Main_OnCommandSuccess_ShouldReturn0()
        {
            Program.AvailableCommands = new[] { GetGoodCommand() };
            var result = Program.Main(new[] { "testcommand", "arg1", "arg2" });
            Assert.Equal(0, result);
        }

        [Fact]
        public void Main_OnCommandException_ShouldNotThrow()
        {
            Program.AvailableCommands = new[] { GetBadCommand() };
            Program.Main(new[] { "badCommand", "arg1", "arg2" });
        }

        [Fact]
        public void Main_OnCommandException_ShouldShowError()
        {
            Program.AvailableCommands = new[] { GetBadCommand() };
            Program.Main(new[] { "badCommand", "arg1", "arg2" });
            _stderr.Flush();
            Assert.True(_stderr.ToString().Contains("Oops!"));
        }

        [Fact]
        public void Main_OnCommandException_ShouldReturn1()
        {
            Program.AvailableCommands = new[] { GetBadCommand() };
            var result = Program.Main(new[] { "badCommand", "arg1", "arg2" });
            Assert.Equal(1, result);
        }

        [Fact]
        public void Main_OnCommandErrors_ShouldShowErrors()
        {
            Program.AvailableCommands = new[] { GetCommandWithErrors() };
            Program.Main(new[] { "testcommand", "arg1", "arg2" });
            _stderr.Flush();
            var errors = _stderr.ToString();
            Assert.True(errors.Contains("Error1"));
            Assert.True(errors.Contains("Error2"));
        }

        [Fact]
        public void Main_OnCommandErrors_ShouldReturn1()
        {
            Program.AvailableCommands = new[] { GetCommandWithErrors() };
            var result = Program.Main(new[] { "testcommand", "arg1", "arg2" });
            Assert.Equal(1, result);
        }

        [Fact]
        public void Main_OnHelpCommand_ShouldShowUsageAndCommandList()
        {
            Program.AvailableCommands = new[] { GetGoodCommand() };
            Program.Main(new[] { "help" });
            
            _stdout.Flush();
            _stderr.Flush();
            Assert.Equal(0, _stderr.ToString().Length);

            var stdout = _stdout.ToString();
            Assert.True(stdout.Contains("Usage"));
            Assert.True(stdout.Contains("testcommand"));
            Assert.True(stdout.Contains("help"));
        }

        [Fact]
        public void Main_OnHelpForInvalidCommand_ShowError()
        {
            Program.AvailableCommands = new[] { GetGoodCommand() };
            Program.Main(new[] { "help", "nosuch" });

            _stderr.Flush();
            Assert.NotEqual(0, _stderr.ToString().Length);

        }

        [Fact]
        public void Main_OnHelpForInvalidCommand_ShouldReturn1()
        {
            Program.AvailableCommands = new[] { GetGoodCommand() };
            var result = Program.Main(new[] { "help", "nosuch" });
            Assert.Equal(1, result);
        }

        [Fact]
        public void Main_OnHelpCommand_Return0()
        {
            Program.AvailableCommands = new[] { GetGoodCommand() };
            var result = Program.Main(new[] { "help" });
            Assert.Equal(0, result);
        }

        [Fact]
        public void Main_OnHelpCommandWithCommandName_ShouldShowCommandHelp()
        {
            Program.AvailableCommands = new[] { GetGoodCommand() };
            Program.Main(new[] { "help", "testcommand" });

            _stdout.Flush();
            _stderr.Flush();
            Assert.Equal(0, _stderr.ToString().Length);

            var stdout = _stdout.ToString();
            Assert.True(stdout.Contains("A helpful message"));
        }

        [Fact]
        public void Main_OnHelpCommandWithCommandName_Return0()
        {
            Program.AvailableCommands = new[] { GetGoodCommand() };
            var result = Program.Main(new[] { "help", "testcommand" });
            Assert.Equal(0, result);
        }

        private static IConsoleCommand GetGoodCommand()
        {
            var commandMock = new Mock<IConsoleCommand>();
            commandMock.SetupGet(c => c.Name).Returns("testcommand");
            commandMock.SetupGet(c => c.HelpMessage).Returns("A helpful message");
            commandMock.SetupGet(c => c.Description).Returns("Test description");

            return commandMock.Object;
        }

        private static IConsoleCommand GetBadCommand()
        {
            var badCommand = new Mock<IConsoleCommand>();
            badCommand.SetupGet(c => c.Name).Returns("badCommand");
            badCommand.Setup(c => c.Run(It.IsAny<string[]>())).Throws(new Exception("Oops!"));
            return badCommand.Object;
        }

        private static IConsoleCommand GetCommandWithErrors()
        {
            var commandWithErrors = new Mock<IConsoleCommand>();
            commandWithErrors.SetupGet(c => c.Name).Returns("testcommand");
            commandWithErrors
                .Setup(c => c.Run(It.IsAny<string[]>()))
                .Returns((string[] args) => new [] {"Error1", "Error2"});
            return commandWithErrors.Object;
        }
    }
}
