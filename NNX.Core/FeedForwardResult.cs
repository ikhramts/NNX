namespace NNX.Core
{
    public class FeedForwardResult
    {
        public double[] InputWithBias { get; set; }
        public double[] Output { get; set; }

        /// <summary>
        /// Includes results from all hidden layers.
        /// </summary>
        public double[][] HiddenLayers { get; set; }
    }
}
