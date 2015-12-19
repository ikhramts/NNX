using System;
using System.IO;
using Newtonsoft.Json;

namespace NeuralNetworks.Training
{
    public class TrainerConfig : ICloneable
    {
        public double LearningRate { get; set; }
        public double Momentum { get; set; }
        public double QuadraticRegularization { get; set; }
        public double NumEpochs { get; set; }
        public int Seed { get; set; }

        public TrainerConfig()
        {
            LearningRate = 0.01;
            NumEpochs = 1000;
        }

        public void Validate()
        {
            if (LearningRate <= 0)
                throw new NeuralNetworkException($"Property LearningRate must be positive; was {LearningRate}.");

            if (Momentum < 0)
                throw new NeuralNetworkException($"Property Momentum cannot be negative; was {Momentum}.");

            if (QuadraticRegularization < 0)
                throw new NeuralNetworkException(
                    $"Property QuadraticRegularization cannot be negative; was {QuadraticRegularization}.");

            if (NumEpochs <= 0)
                throw new NeuralNetworkException($"Property NumEpochs must be positive; was {NumEpochs}.");
        }

        public static TrainerConfig FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<TrainerConfig>(json);
            }
            catch (Exception ex)
            {
                throw new NeuralNetworkException("Error reading trainer config JSON. ", ex);
            }
            
        }

        public static TrainerConfig FromFile(string configFilePath)
        {
            return FromJson(File.ReadAllText(configFilePath));
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public TrainerConfig Clone()
        {
            return (TrainerConfig)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
