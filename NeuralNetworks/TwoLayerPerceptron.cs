using System;
using System.Collections.Generic;

namespace NeuralNetworks
{
    public class TwoLayerPerceptron : INeuralNetwork
    {
        public int NumInputs { get; private set; }
        public int NumHidden { get; private set; }
        public int NumOutputs { get; private set; }

        public int NumHiddenWeights { get; private set; }
        public int NumOutputWeights { get; private set; }

        public double[] HiddenWeights { get; set; }
        public double[] OutputWeights { get; set; }

        public double[][] Weights => new[] {HiddenWeights, OutputWeights};

        public TwoLayerPerceptron(int numInputs, int numHidden, int numOutputs, Random customRand = null)
        {
            InitState(numInputs, numHidden, numOutputs);
            InitializeWeights(customRand);
        }

        private void InitState(int numInputs, int numHidden, int numOutputs)
        {
            // Biases will be treated as extra inputs at very last position.
            NumInputs = numInputs;
            NumHidden = numHidden;
            NumOutputs = numOutputs;

            NumHiddenWeights = (NumInputs + 1) * NumHidden;
            NumOutputWeights = (NumHidden + 1) * NumOutputs;

            HiddenWeights = new double[NumHiddenWeights];
            OutputWeights = new double[NumOutputWeights];
        }


        public void InitializeWeights(Random customRand = null)
        {
            var random = customRand ?? new Random();

            for (var ij = 0; ij < NumHiddenWeights; ij++)
                HiddenWeights[ij] = random.NextDouble() * 0.2 - 0.1;

            for (var jk = 0; jk < NumOutputWeights; jk++)
                OutputWeights[jk] = random.NextDouble() * 0.2 - 0.1;
        }

        public FeedForwardResult FeedForward(double[] input)
        {
            if (input.Length != NumInputs)
                throw new NeuralNetworkException($"Expected input length to be {NumInputs} but got {input.Length}.");

            var result = new FeedForwardResult();
            var inputsWithBias = GetInputsWithBias(input);
            result.InputWithBias = inputsWithBias;

            var hidden = new double[NumHidden + 1];
            hidden[NumHidden] = 1; // Bias.
            result.HiddenLayers = new [] {hidden};

            for (var j = 0; j < NumHidden; j++)
            {
                var preActivation = 0.0;
                var offset = j * (NumInputs + 1);

                for (var i = 0; i <= NumInputs; i++)
                    preActivation += HiddenWeights[offset + i] * inputsWithBias[i];

                hidden[j] = Math.Tanh(preActivation);
            }

            var sumOfPreOutputs = 0.0;
            var preOutputs = new double[NumOutputs];

            for (var k = 0; k < NumOutputs; k++)
            {
                var preOutput = 0.0;
                var offset = k * (NumHidden + 1);

                for (var j = 0; j < NumHidden + 1; j++)
                    preOutput += OutputWeights[offset + j] * hidden[j];

                preOutputs[k] = Math.Exp(preOutput);
                sumOfPreOutputs += preOutputs[k];
            }

            var outputs = new double[NumOutputs];

            for (var k = 0; k < NumOutputs; k++)
                outputs[k] = preOutputs[k] / sumOfPreOutputs;

            result.Output = outputs;

            return result;
        }

        public double[][] CalculateGradients(double[] input, double[] target)
        {
            if (target.Length != NumOutputs)
                throw new NeuralNetworkException($"Expected target length to be {NumOutputs} but got {target.Length}.");

            var feedForwardResult = FeedForward(input);
            var outputs = feedForwardResult.Output;
            var inputsWithBias = feedForwardResult.InputWithBias;
            var hidden = feedForwardResult.HiddenLayers[0];

            var newHiddenWeightGrads = new double[HiddenWeights.Length];
            var newOutputWeightGrads = new double[OutputWeights.Length];
            var preOutputGrads = new double[NumOutputs];
            var preHiddenPreOutputGrads = new double[NumOutputs * NumHidden];

            // Use cross-entropy error.
            for (var k = 0; k < NumOutputs; k++)
            {
                preOutputGrads[k] = outputs[k] - target[k];
            }

            for (var k = 0; k < NumOutputs; k++)
            {
                var offset = k * (NumHidden + 1);

                for (var j = 0; j < NumHidden + 1; j++)
                    newOutputWeightGrads[offset + j] = preOutputGrads[k] * hidden[j];
            }

            for (var k = 0; k < NumOutputs; k++)
            {
                for (var j = 0; j < NumHidden; j++)
                    preHiddenPreOutputGrads[k * NumHidden + j] = OutputWeights[k * (NumHidden + 1) + j]
                                                                    * (1 - hidden[j] * hidden[j]);
            }

            for (var j = 0; j < NumHidden; j++)
            {
                var offset = j * (NumInputs + 1);

                for (var i = 0; i < NumInputs + 1; i++)
                {
                    var grad = 0.0;

                    for (var k = 0; k < NumOutputs; k++)
                        grad += preOutputGrads[k] * preHiddenPreOutputGrads[k * NumHidden + j];

                    grad *= inputsWithBias[i];
                    newHiddenWeightGrads[offset + i] = grad;
                }
            }

            var result = new[] {newHiddenWeightGrads, newOutputWeightGrads};

            return result;
        }

        private double[] GetInputsWithBias(double[] inputs)
        {
            // We need to include bias input (=1) as an additional input at the end of the array.
            var inputsWithBias = new double[NumInputs + 1];
            Array.Copy(inputs, inputsWithBias, inputs.Length);
            inputsWithBias[inputsWithBias.Length - 1] = 1;
            return inputsWithBias;
        }
    }
}
