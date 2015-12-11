using System;

namespace NNX
{
    public class NNXException : Exception
    {
        public NNXException(string message, Exception innerException = null)
            :base(message, innerException)
        {
        }
    }
}
