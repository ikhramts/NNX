using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetworks
{
    public class MultilayerPerceptron : INeuralNetwork
    {
        public int NumInputs { get; }
        public int NumOutputs { get; }
        public IList<int> HiddenLayerSizes { get; }
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

        /// <summary>
        /// Activation functions: tanh for hidden nodes, softmax for output nodes.
        /// </summary>
        public FeedForwardResult FeedForward(double[] input)
        {
            if (input.Length != NumInputs)
                throw new NeuralNetworkException($"Expected input length to be {NumInputs} but got {input.Length}.");

            // Add bias input node (always = 1).
            var inputsWithBias = new double[NumInputs + 1];
            Array.Copy(input, inputsWithBias, input.Length);
            inputsWithBias[inputsWithBias.Length - 1] = 1;

            // Prepare hidden nodes. Include bias node (always = 1) as the last node in each layer.
            var hidden = new double[HiddenLayerSizes.Count][];

            for (var i = 0; i < HiddenLayerSizes.Count; i++)
            {
                var length = HiddenLayerSizes[i] + 1;
                hidden[i] = new double[length];
                hidden[i][length - 1] = 1;
            }

            // Calculate first hidden layer.
            for (var j = 0; j < HiddenLayerSizes[0]; j++)
            {
                var preActivation = 0.0;
                var offset = j * (NumInputs + 1);

                for (var i = 0; i < NumInputs + 1; i++)
                    preActivation += inputsWithBias[i] * Weights[0][offset + i];

                hidden[0][j] = Math.Tanh(preActivation);
            }

            // Calculate the rest of the hidden layers.
            for (var l = 1; l < HiddenLayerSizes.Count; l++)
            {
                for (var j = 0; j < HiddenLayerSizes[l]; j++)
                {
                    var preActivation = 0.0;
                    var prevLayerSize = HiddenLayerSizes[l - 1] + 1;
                    var offset = j * prevLayerSize;

                    for (var i = 0; i < prevLayerSize; i++)
                        preActivation += hidden[l-1][i] * Weights[l][offset + i];

                    hidden[l][j] = Math.Tanh(preActivation);
                }
            }

            // Calculate the output layer.
            var lastHiddenLayer = hidden[HiddenLayerSizes.Count - 1];
            var lastHiddenLayerSize = lastHiddenLayer.Length;
            var outputWeights = Weights[Weights.Length - 1];
            var sumOfPreOutputs = 0.0;
            var preOutputs = new double[NumOutputs];

            for (var k = 0; k < NumOutputs; k++)
            {
                var preOutput = 0.0;
                var offset = k * lastHiddenLayerSize;

                for (var j = 0; j < lastHiddenLayerSize; j++)
                    preOutput += outputWeights[offset + j] * lastHiddenLayer[j];

                preOutputs[k] = Math.Exp(preOutput);
                sumOfPreOutputs += preOutputs[k];
            }

            var outputs = new double[NumOutputs];

            for (var k = 0; k < NumOutputs; k++)
                outputs[k] = preOutputs[k] / sumOfPreOutputs;

            var result = new FeedForwardResult();
            result.InputWithBias = inputsWithBias;
            result.HiddenLayers = hidden;
            result.Output = outputs;

            return result;
        }

        public double[][] CalculateGradients(double[] input, double[] targets)
        {
            throw new NotImplementedException();
        }
    }
}
