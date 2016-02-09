using System;

namespace NNX.Core
{
    public class NeuralNetworkException : Exception
    {
        public NeuralNetworkException(string message, Exception innerException = null)
            : base(message, innerException)
        {
            
        }
    }
}
