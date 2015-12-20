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

        public double[] Inputs { get; set; }
        public double[] Hidden { get; set; }
        public double[] Outputs { get; set; }

        public double[] HiddenWeights { get; set; }
        public double[] OutputWeights { get; set; }

        public double[][] Weights => new[] {HiddenWeights, OutputWeights};

        public TwoLayerPerceptron(int numInputs, int numHidden, int numOutputs, Random customRand = null)
        {
            InitState(numInputs, numHidden, numOutputs);
            InitializeWeights(customRand);
        }

        public TwoLayerPerceptron(NeuralNetworkConfig config, Random customRand = null)
        {
            if (config.Weights != null && config.Weights.Length != 2)
                throw new NeuralNetworkException("Config expeced to have exactly two sets of weights. Found: " +
                                                 +((int) config.Weights?.Length));

            var numInputs = config.GetSettingInt("NumInputs");
            var numHidden = config.GetSettingInt("NumHidden");
            var numOutputs = config.GetSettingInt("NumOutputs");

            InitState(numInputs, numHidden, numOutputs);

            if (config.Weights == null)
            {
                InitializeWeights(customRand);
                return;
            }

            var newHiddenWeights = config.Weights[0];
            var newOutputWeights = config.Weights[1];

            if (newHiddenWeights.Length != HiddenWeights.Length)
            {
                var message = $"Expected {HiddenWeights.Length} hidden weights; found {newHiddenWeights.Length}.";
                throw new NeuralNetworkException(message);
            }

            if (newOutputWeights.Length != OutputWeights.Length)
            {
                var message = $"Expected {OutputWeights.Length} output weights; found {newOutputWeights.Length}.";
                throw new NeuralNetworkException(message);
            }

            Array.Copy(newHiddenWeights, HiddenWeights, newHiddenWeights.Length);
            Array.Copy(newOutputWeights, OutputWeights, newOutputWeights.Length);
        }

        private void InitState(int numInputs, int numHidden, int numOutputs)
        {
            // Biases will be treated as extra inputs at very last position.
            NumInputs = numInputs;
            NumHidden = numHidden;
            NumOutputs = numOutputs;

            NumHiddenWeights = (NumInputs + 1) * NumHidden;
            NumOutputWeights = (NumHidden + 1) * NumOutputs;

            Inputs = new double[NumInputs + 1];
            Hidden = new double[NumHidden + 1];
            Outputs = new double[NumOutputs];

            Inputs[NumInputs] = 1;
            Hidden[NumHidden] = 1;

            HiddenWeights = new double[NumHiddenWeights];
            OutputWeights = new double[NumOutputWeights];
        }

        public void SetInputs(double[] inputs)
        {
            if (inputs.Length != NumInputs)
                throw new NeuralNetworkException($"Input array length ({inputs.Length}) does not match "+
                                                    $"neural network input length ({NumInputs})");

            Array.Copy(inputs, Inputs, NumInputs);
        }

        public void InitializeWeights(Random customRand = null)
        {
            var random = customRand ?? new Random();

            for (var ij = 0; ij < NumHiddenWeights; ij++)
                HiddenWeights[ij] = random.NextDouble() * 0.2 - 0.1;

            for (var jk = 0; jk < NumOutputWeights; jk++)
                OutputWeights[jk] = random.NextDouble() * 0.2 - 0.1;
        }

        public void FeedForward()
        {
            for (var j = 0; j < NumHidden; j++)
            {
                var preActivation = 0.0;
                var offset = j * (NumInputs + 1);

                for (var i = 0; i <= NumInputs; i++)
                    preActivation += HiddenWeights[offset + i] * Inputs[i];

                Hidden[j] = Math.Tanh(preActivation);
            }

            var sumOfPreOutputs = 0.0;
            var preOutputs = new double[NumOutputs];

            for (var k = 0; k < NumOutputs; k++)
            {
                var preOutput = 0.0;
                var offset = k * (NumHidden + 1);

                for (var j = 0; j < NumHidden + 1; j++)
                    preOutput += OutputWeights[offset + j] * Hidden[j];

                preOutputs[k] = Math.Exp(preOutput);
                sumOfPreOutputs += preOutputs[k];
            }

            for (var k = 0; k < NumOutputs; k++)
                Outputs[k] = preOutputs[k] / sumOfPreOutputs;
        }

        public double[][] CalculateGradients(double[] target)
        {
            var newHiddenWeightGrads = new double[HiddenWeights.Length];
            var newOutputWeightGrads = new double[OutputWeights.Length];
            var preOutputGrads = new double[NumOutputs];
            var preHiddenPreOutputGrads = new double[NumOutputs * NumHidden];

            // Use cross-entropy error.
            for (var k = 0; k < NumOutputs; k++)
            {
                preOutputGrads[k] = Outputs[k] - target[k];
            }

            for (var k = 0; k < NumOutputs; k++)
            {
                var offset = k * (NumHidden + 1);

                for (var j = 0; j < NumHidden + 1; j++)
                    newOutputWeightGrads[offset + j] = preOutputGrads[k] * Hidden[j];
            }

            for (var k = 0; k < NumOutputs; k++)
            {
                for (var j = 0; j < NumHidden; j++)
                    preHiddenPreOutputGrads[k * NumHidden + j] = OutputWeights[k * (NumHidden + 1) + j]
                                                                    * (1 - Hidden[j] * Hidden[j]);
            }

            for (var j = 0; j < NumHidden; j++)
            {
                var offset = j * (NumInputs + 1);

                for (var i = 0; i < NumInputs + 1; i++)
                {
                    var grad = 0.0;

                    for (var k = 0; k < NumOutputs; k++)
                        grad += preOutputGrads[k] * preHiddenPreOutputGrads[k * NumHidden + j];

                    grad *= Inputs[i];

                    newHiddenWeightGrads[offset + i] = grad;
                }
            }

            var result = new[] {newHiddenWeightGrads, newOutputWeightGrads};

            return result;
        }

        public NeuralNetworkConfig GetConfig()
        {
            var config = new NeuralNetworkConfig
            {
                NetworkType = "TwoLayerPerceptron",
                Settings = new Dictionary<string, string>
                {
                    {"NumInputs", NumInputs.ToString()},
                    {"NumHidden", NumHidden.ToString()},
                    {"NumOutputs", NumOutputs.ToString()},
                },
                Weights = Weights.DeepClone()
            };

            return config;
        }
    }
}
