using System;

namespace NeuralNetworks
{
    public class NeuralNetworkBuilder
    {
        public static NeuralNetworkBuilder Builder { get; set; }

        static NeuralNetworkBuilder()
        {
            Builder = new NeuralNetworkBuilder();
        }

        public virtual INeuralNetwork CustomBuild(NeuralNetworkConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            switch (config.NetworkType)
            {
                case "TwoLayerPerceptron":
                    return new TwoLayerPerceptron(config);

                default:
                    throw new NeuralNetworkException("Unrecognized NetworkType: " + config.NetworkType);
            }
        }

        public static INeuralNetwork Build(NeuralNetworkConfig config)
        {
            return Builder.CustomBuild(config);
        }

        public static INeuralNetwork FromFile(string configFilePath)
        {
            return Build(NeuralNetworkConfig.FromFile(configFilePath));
        }
    }
}
