using System.Collections.Generic;

namespace nnet
{
    public interface IConsoleCommand
    {
        /// <summary>
        /// Run the command and return error messages.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        IEnumerable<string> Run(string[] args);

        /// <summary>
        /// The name by which command gets invoked.
        /// </summary>
        string Name { get; }

        string Description { get; }

        string HelpMessage { get; }
    }
}
