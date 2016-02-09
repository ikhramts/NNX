using System;

namespace NNX.AddIn
{
    public class NNXException : Exception
    {
        public NNXException(string message, Exception innerException = null)
            :base(message, innerException)
        {
        }
    }
}
