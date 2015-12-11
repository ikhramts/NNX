using System;
using System.IO;
using Newtonsoft.Json;

namespace NeuralNetworks.Training
{
    public class TrainerConfig
    {
        public NeuralNetworkConfig NeuralNetworkConfig { get; set; }
        public double LearningRate { get; set; }
        public double Momentum { get; set; }
        public double QuadraticRegularization { get; set; }
        public double NumEpochs { get; set; }

        public TrainerConfig()
        {
            LearningRate = 0.01;
            NumEpochs = 1000;
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
    }
}
