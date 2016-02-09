using System;

namespace NNX.Core.Utils
{
    /// <summary>
    /// Thin wrapper over System.Random.
    /// </summary>
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

        public double NextDouble() => _random.NextDouble();

        public int Next(int maxValue) => _random.Next(maxValue);

        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
    }
}
