using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using Newtonsoft.Json;

namespace NeuralNetworks.Training
{
    public class TrainingRun
    {
        public TrainerConfig TrainerConfig { get; set; }

        public string TrainingDataFile { get; set; }
        public string NetworkOutputFile { get; set; }
        public string ResultsOutputFile { get; set; }
        public double TestFraction { get; set; }
        public int Seed { get; set; }

        public void Run()
        {
            if (TrainingDataFile == null)
                throw new NeuralNetworkException("Missing TrainingDataFile property in TrainingRun");

            var contents = File.ReadAllText(TrainingDataFile);
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
            var trainingSetSize = (int)(shuffledSet.Count * TestFraction);
            var trainingSet = shuffledSet.Take(trainingSetSize).ToList();
            var testingSet = shuffledSet.Skip(trainingSetSize).ToList();

            var trainer = new Trainer(TrainerConfig);

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

            // And output the neural network config.
            nn.GetConfig().SaveToFile(NetworkOutputFile);
        }

        public static TrainingRun FromJson(string json)
        {
            return JsonConvert.DeserializeObject<TrainingRun>(json);
        }

        public static TrainingRun FromFile(string configFilePath)
        {
            return FromJson(File.ReadAllText(configFilePath));
        }
    }
}
