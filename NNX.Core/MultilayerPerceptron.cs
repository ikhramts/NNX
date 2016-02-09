using System;
using System.Collections.Generic;
using System.Linq;

namespace NNX.Core
{
    /// <summary>
    /// Multilayer perceptron with the following features:
    /// * Hidden layer activation functions: tahn
    /// * Output layer activation function: softmax
    /// * Gradient calculation assumes optimization for min cross-entropy error.
    /// </summary>
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
        public FeedForwardResult FeedForward(double[] inputs)
        {
            if (inputs.Length != NumInputs)
                throw new NeuralNetworkException($"Argument 'inputs' should have width {NumInputs}; was {inputs.Length}.");

            // Add bias input node (always = 1).
            var inputsWithBias = new double[NumInputs + 1];
            Array.Copy(inputs, inputsWithBias, inputs.Length);
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

        /// <summary>
        /// Calculates gradients of the error function (cross-entropy) with respect to weights.
        /// </summary>
        public double[][] CalculateGradients(double[] inputs, double[] targets)
        {
            if (targets.Length != NumOutputs)
                throw new NeuralNetworkException($"Argument 'targets' should have width {NumOutputs}; was " +
                                                 $"{targets.Length}.");

            var result = new double[Weights.Length][];

            for (var l = 0; l < Weights.Length; l++)
                result[l] = new double[Weights[l].Length];

            var feedForwardResults = FeedForward(inputs);
            var inputWithBiases = feedForwardResults.InputWithBias;
            var hidden = feedForwardResults.HiddenLayers;
            var outputs = feedForwardResults.Output;

            // The following is a stackexchange-flavored markdown with TeX formulas.
            // To read it, copy-paste it into https://stackedit.io/editor, or paste individual TeX formulas
            // into a LaTeX editor.

            /*
We'll calculate gradients of error function with respect to weights using the following formulas.

Notation:

* $E$: error function (cross-entropy error of targets relative to outputs).
* $n$: number of hidden and output layers.
* $a$: a layer between $0$ (input) and $n$ (output), inclusive.
* $i_a$: $i$-th node in layer $a$.
* $o_{i_n}$, $t_{i_n}$: value of $i$-th output node, and its training target value.
* $h_{i_a}$: value of $i$-th hidden (or input, or output) node in layer $a$ after activation.
* $\bar{h}_{i_a}$: value of $i$-th node in layer $a$ before activation (so $h_{i_a} = \tanh \bar{h}_{i_a}$) .
* $w_{i_{a-1}, i_a}$: weight of $i_{a-1}$-th node in layer $a-1$ in activation of $i_a$-th node in layer $a$.

Note that indexes $i_a$ and $i_{a-1}$ may have different values.

We can calculate the gradient of error function with respect to any weight $w_{i_{a-1}, i_a}$ using
$$
\frac{\partial E}{\partial w_{i_{a-1},i_a}} = h_{i_{a-1}}\frac{\partial E}{\partial \bar{h}_{i_{a}}}
$$
where for layer $n$ (output):
$$
\frac{\partial E}{\partial \bar{h}_{i_{n}}} = o_{i_n} - t_{i_n}
$$
and for any preceding layer $0 < a < n$: 
$$
\frac{\partial E}{\partial \bar{h}_{i_{a}}} = (1-h^2_{i_a})\sum_{j_{a+1}}w_{i_a, j_{a+1}}\frac{\partial E}{\partial \bar{h}_{j_{a+1}}}
$$
            */

            var maxPreActivationGrads = Math.Max(HiddenLayerSizes.Max(), NumOutputs);
            var preActivationGrads = new double[maxPreActivationGrads];
            var prevLayerPreActivationGrads = new double[maxPreActivationGrads];

            // Base setup: error function gradients w/r/t pre-activation output node values.
            for (var i = 0; i < NumOutputs; i++)
                preActivationGrads[i] = outputs[i] - targets[i];

            // Recursive cases: calculate weight gradients and pre-activation hidden node gradients for next layer.
            var nextLayerSize = NumOutputs;
            int thisLayerSize;

            for (var layer = HiddenLayerSizes.Count - 1; layer >= 0; layer--)
            {
                thisLayerSize = HiddenLayerSizes[layer];

                // Calculate weight gradients.
                for (var i = 0; i < nextLayerSize; i++)
                {
                    var offset = (thisLayerSize + 1) * i;
                    for (var j = 0; j < thisLayerSize + 1; j++)
                        result[layer + 1][offset + j] = preActivationGrads[i] * hidden[layer][j];
                }

                // Calculate next layer pre-activation node gradients.
                for (var i = 0; i < thisLayerSize; i++)
                {
                    var sumOfGrads = 0.0;

                    for (var j = 0; j < nextLayerSize; j++)
                        sumOfGrads += Weights[layer + 1][j * (thisLayerSize + 1) + i] * preActivationGrads[j];

                    prevLayerPreActivationGrads[i] = (1 - hidden[layer][i] * hidden[layer][i]) * sumOfGrads;
                }

                var temp = preActivationGrads;
                preActivationGrads = prevLayerPreActivationGrads;
                prevLayerPreActivationGrads = temp;

                nextLayerSize = thisLayerSize;
            }

            // Final case: gradients for weights for first layer.
            thisLayerSize = NumInputs;

            for (var i = 0; i < nextLayerSize; i++)
            {
                var offset = (thisLayerSize + 1) * i;
                for (var j = 0; j < thisLayerSize + 1; j++)
                    result[0][offset + j] = preActivationGrads[i] * inputWithBiases[j];
            }

            return result;
        }
    }
}
