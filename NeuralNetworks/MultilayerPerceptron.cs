using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetworks
{
    public class MultilayerPerceptron : INeuralNetwork
    {
        public int NumInputs { get; }
        public int NumOutputs { get; }
        public IList<int> HiddenLayerSizes { get; private set; }

        public double[][] Weights { get; }

        public MultilayerPerceptron(int numInputs, int numOutputs, IList<int> hiddenLayerSizes)
        {
            if (numInputs <= 0)
                throw new NeuralNetworkException($"Argument {nameof(numInputs)} must be positive; was {numInputs}.");

            if (numOutputs <= 0)
                throw new NeuralNetworkException($"Argument {nameof(numOutputs)} must be positive; was {numOutputs}.");

            if (hiddenLayerSizes == null || !hiddenLayerSizes.Any())
                throw new NeuralNetworkException($"Argument {nameof(hiddenLayerSizes)} cannot be null or empty.");

            if (hiddenLayerSizes.Any(h => h <= 0))
            {
                var badSize = hiddenLayerSizes.First(h => h <= 0);
                var index = hiddenLayerSizes.IndexOf(badSize);
                throw new NeuralNetworkException($"Argument {nameof(hiddenLayerSizes)} must contain only positive " +
                                                $"values; was {badSize} at index {index}.");
            }

            NumInputs = numInputs;
            NumOutputs = numOutputs;
            HiddenLayerSizes = hiddenLayerSizes.ToArray();

            Weights = new double[hiddenLayerSizes.Count + 1][];

            for (var i = 0; i < hiddenLayerSizes.Count + 1; i++)
            {
                if (i == 0)
                    Weights[i] = new double[(numInputs + 1) * hiddenLayerSizes[0]];
                else if (i < hiddenLayerSizes.Count)
                    Weights[i] = new double[(hiddenLayerSizes[i-1] + 1) * hiddenLayerSizes[i]];
                else
                    Weights[i] = new double[(hiddenLayerSizes[hiddenLayerSizes.Count - 1] + 1) * numOutputs];
            }
        }

        public FeedForwardResult FeedForward(double[] input)
        {
            throw new NotImplementedException();
        }

        public double[][] CalculateGradients(double[] input, double[] targets)
        {
            throw new NotImplementedException();
        }

        public static void MakeConfig(int numInputs, int numOutputs, IList<int> hiddenLayerSizes)
        {
            throw new NotImplementedException();
        }
    }
}
