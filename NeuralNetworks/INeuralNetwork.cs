namespace NeuralNetworks
{
    public interface INeuralNetwork
    {
        /// <summary>
        /// Indicates the size of expected input array.
        /// </summary>
        int NumInputs { get; }
        int NumOutputs { get; }

        /// <summary>
        /// Provides a reference to the weights for direct updates.
        /// </summary>
        double[][] Weights { get; }

        /// <summary>
        /// Make the neural network calculate outputs based on current weights
        /// and inputs.
        /// </summary>
        FeedForwardResult FeedForward(double[] input);

        /// <summary>
        /// Make the neural network calculate a partial derivative of error function
        /// with respect  to each weight for backpropagation.
        /// </summary>
        /// <returns></returns>
        double[][] CalculateGradients(double[] input, double[] targets);
    }
}
