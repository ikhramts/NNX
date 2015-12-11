using System.Collections.Generic;

namespace NeuralNetworks.Tests.TestObjects
{
    public static class NeuralNetworkConfigObjects
    {
        public static NeuralNetworkConfig GetTwoLayerPerceptronConfig()
        {
            var config = new NeuralNetworkConfig
            {
                NetworkType = "TwoLayerPerceptron",
                Settings = new Dictionary<string, string>
                {
                    {"NumInputs", "2"},
                    {"NumHidden", "3"},
                    {"NumOutputs", "4"},
                },
                Weights = new[]
                {
                    new[] {0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5},
                    new[] {0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6, 0.6}
                }
            };

            return config;
        }
    }
}
