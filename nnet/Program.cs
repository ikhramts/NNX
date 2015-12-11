using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nnet
{
    public class Program
    {
        public static IConsoleCommand[] AvailableCommands = {new TrainCommand()};

        public const string UsageBase = @"
Usage:
        nnet <command> <args>
        nnet help
        nnet help <command>

";

        public static int Main(string[] args)
        {
            var commandsByName = AvailableCommands.ToDictionary(c => c.Name);

            if (args.Length == 0)
            {
                Console.Error.WriteLine(GetUsageMessage());
                return 1;
            }

            var commandName = args[0];

            if (commandName == "help")
            {
                if (args.Length == 1)
                {
                    Console.WriteLine(GetUsageMessage());
                }
                else
                {
                    var targetCommandName = args[1];
                    IConsoleCommand targetCommand;

                    if (!commandsByName.TryGetValue(targetCommandName, out targetCommand))
                    {
                        Console.Error.WriteLine("No such command: " + targetCommandName + "\n");
                        Console.Error.WriteLine(GetAvailableCommands());
                        return 1;
                    }

                    Console.WriteLine(targetCommand.HelpMessage);
                }

                return 0;
            }

            IConsoleCommand command;

            if (!commandsByName.TryGetValue(commandName, out command))
            {
                Console.Error.WriteLine("No such command: " + commandName);
                Console.Error.Write(GetUsageMessage());
                return 1;
            }

            var commandArgs = new string[args.Length - 1];
            Array.Copy(args, 1, commandArgs, 0, commandArgs.Length);

            IEnumerable<string> errors;

            try
            {
                errors = command.Run(commandArgs);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }

            if (errors != null)
            {
                var hasErrors = false;
                foreach (var error in errors)
                {
                    hasErrors = true;
                    Console.Error.WriteLine(error);
                }

                if (hasErrors)
                    return 1;
            }

            return 0;
        }

        public static string GetUsageMessage()
        {
            return UsageBase + GetAvailableCommands();
        }

        public static string GetAvailableCommands()
        {
            var usage = new StringBuilder("Available commands: \n\n");

            foreach (var command in AvailableCommands)
                usage.AppendFormat("  {0}{1}\n", command.Name.PadRight(15), command.Description);

            return usage.ToString();
        }
    }
}
