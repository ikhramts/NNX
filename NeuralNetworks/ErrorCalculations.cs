using System;

namespace NeuralNetworks
{
    public static class ErrorCalculations
    {
        public static double CrossEntropyError(double[] output, double[] target)
        {
            if (output.Length != target.Length)
                throw new NeuralNetworkException("Length of 'output' argument (" + output.Length +
                                                ") is different from length of 'target' argument (" +
                                                target.Length + ").");

            var error = 0.0;

            for (var i = 0; i < output.Length; i++)
                error -= target[i] * Math.Log(output[i]);

            return error;
        }

        public static double MeanSquareError(double[] output, double[] target)
        {
            if (output.Length != target.Length)
                throw new NeuralNetworkException("Length of 'output' argument (" + output.Length +
                                                ") is different from length of 'target' argument (" +
                                                target.Length + ").");

            var error = 0.0;

            for (var i = 0; i < output.Length; i++)
                error += (target[i] - output[i]) * (target[i] - output[i]);

            error /= output.Length;

            return error;
        }
    }
}
