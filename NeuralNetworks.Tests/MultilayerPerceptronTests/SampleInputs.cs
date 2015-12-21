namespace NeuralNetworks.Tests.MultilayerPerceptronTests
{
    public static class SampleInputs
    {
        public static MultilayerPerceptron GetSample1HiddenLayerPerceptron()
        {
            var nn = new MultilayerPerceptron(2, 2, new [] {3});
            nn.Weights[0] = new[] { 0.1, 0.2, 0.3, 0.11, 0.21, 0.31, 0.12, 0.22, 0.32 };
            nn.Weights[1] = new[] { 0.4, 0.41, 0.42, 0.43, 0.5, 0.51, 0.52, 0.53 };
            return nn;
        }

        public static MultilayerPerceptron GetSample2HiddenLayerPerceptron()
        {
            var nn = new MultilayerPerceptron(2, 2, new[] { 3, 2 });
            nn.Weights[0] = new[] { 0.1, 0.2, 0.3, 0.11, 0.21, 0.31, 0.12, 0.22, 0.32 };
            nn.Weights[1] = new[] { 0.4, 0.41, 0.42, 0.43, 0.5, 0.51, 0.52, 0.53 };
            nn.Weights[2] = new[] { 0.61, 0.62, 0.63, 0.71, 0.72, 0.73 };
            return nn;
        }

        public static double[] GetSampleInputs() => new[] {0.1, 0.2};
        public static double[] GetSampleTargets() => new[] {1.0, 0.0};
    }
}
