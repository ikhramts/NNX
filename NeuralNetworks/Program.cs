using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using NeuralNetworks.Training;

namespace NeuralNetworks
{
    class Program
    {
        private const string Usage = @"
Usage:
    NeuralNetworks <infile.csv>
";

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine(Usage);
                return 1;
            }

            Console.ReadLine();

            var inFile = args[0];
            var contents = File.ReadAllText(inFile);
            var csv = new CsvParser(new StringReader(contents));
            csv.Read();

            var fullSet = new List<InputOutput>();
            var csvLine = 2;

            while (true)
            {
                var record = csv.Read();

                if (record == null || record.Length == 0)
                    break;

                try
                {
                    fullSet.Add(InputOutput.FromCsvRow(record));
                }
                catch (Exception)
                {
                    Console.Error.WriteLine("Error parsing CSV line " + csvLine + ":");
                    throw;
                }

                csvLine++;
            }

            var shuffledSet = fullSet.Shuffle();
            var trainingSetSize = (int) (shuffledSet.Count * 0.7);
            var trainingSet = shuffledSet.Take(trainingSetSize).ToList();
            var testingSet = shuffledSet.Skip(trainingSetSize).ToList();

            var firstInput = trainingSet.First();
            var numInputs = firstInput.Input.Count();
            var numOutputs = firstInput.Output.Count();
            var numHidden = (numInputs + numOutputs)*4;

            var trainer = new Trainer();
            //{
            //    NewNeuralNetwork = () => new TwoLayerPerceptron(numInputs, numHidden, numOutputs),
            //    LearningRate = 0.01,
            //    Momentum = 1,
            //    QuadraticRegularization = 0.001
            //};

            var start = DateTime.Now;
            var nn = trainer.Train(trainingSet);
            var elapsed = DateTime.Now - start;
            var trainError = Trainer.GetError(nn, trainingSet);
            var trainAccuracy = Trainer.GetAccuracy(nn, trainingSet);
            var testError = Trainer.GetError(nn, testingSet);
            var testAccuracy = Trainer.GetAccuracy(nn, testingSet);

            Console.WriteLine("Training complete. Took {0:N} msec", elapsed.TotalMilliseconds);
            Console.WriteLine("Results on training set:");
            Console.WriteLine("    Error: {0:N}", trainError);
            Console.WriteLine("    Accuracy: {0:N}", trainAccuracy);
            Console.WriteLine("Results on test set:");
            Console.WriteLine("    Error: {0:N}", testError);
            Console.WriteLine("    Accuracy: {0:N}", testAccuracy);

            return 0;
        }


    }
}
