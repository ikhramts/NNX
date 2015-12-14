using System;

namespace NeuralNetworks.Utils
{
    public class RandomGenerator : IRandomGenerator
    {
        private Random _random = new Random();

        private int _seed;

        public int Seed
        {
            get { return _seed; }
            set
            {
                _random = new Random(value);
                _seed = value;
            }
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }
    }
}
