namespace NeuralNetworks
{
    public interface INeuralNetwork
    {
        /// <summary>
        /// Indicates the size of expected input array.
        /// </summary>
        int NumInputs { get; }
        int NumOutputs { get; }

        double[] Inputs { get; }

        double[] Outputs { get; }

        void SetInputs(double[] inputs);

        /// <summary>
        /// Provides a reference to the weights for direct updates.
        /// </summary>
        double[][] Weights { get; }

        /// <summary>
        /// Make the neural network calculate outputs based on current weights
        /// and inputs.
        /// </summary>
        void FeedForward();

        /// <summary>
        /// Make the neural network calculate a partial derivative of error function
        /// with respect  to each weight for backpropagation.
        /// </summary>
        /// <returns></returns>
        double[][] CalculateGradients(double[] targets);

        NeuralNetworkConfig GetConfig();
    }
}
