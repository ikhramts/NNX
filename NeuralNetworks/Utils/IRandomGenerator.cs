namespace NeuralNetworks.Utils
{
    public interface IRandomGenerator
    {
        int Seed { get; set; }
        double NextDouble();
        int Next(int maxValue);
    }
}
