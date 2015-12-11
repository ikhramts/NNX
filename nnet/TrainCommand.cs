using System.Collections.Generic;
using NeuralNetworks.Training;

namespace nnet
{
    public class TrainCommand : IConsoleCommand
    {
        public IEnumerable<string> Run(string[] args)
        {
            if (args.Length == 0)
                return new[] { "Missing argument: path to config file.\n" + HelpMessage};

            var trainer = TrainingRun.FromFile(args[0]);
            trainer.Run();
            return null;
        }

        public string Name => "train";
        public string Description => "Trains a neural network given a provided config.";

        public string HelpMessage => @"
Usage:
        nnet train <configfile.json>
";
    }
}
