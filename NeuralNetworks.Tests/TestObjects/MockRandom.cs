using Moq;
using NeuralNetworks.Utils;

namespace NeuralNetworks.Tests.TestObjects
{
    public static class MockRandom
    {
        public const double DoubleValue = 0.5;
        public const int IntValue = 0;

        public static Mock<IRandomGenerator> SetUp()
        {
            var mock = GetMock();

            RandomProvider.GetRandom = seed => mock.Object;

            return mock;
        }

        public static void Dispose()
        {
            RandomProvider.GetRandom = RandomProvider.GetDefaultRandom;
        }

        public static IRandomGenerator Get()
        {
            return GetMock().Object;
        }

        public static Mock<IRandomGenerator> GetMock()
        {
            var mock = new Mock<IRandomGenerator>();
            mock.SetupAllProperties();
            mock.Setup(r => r.Next(It.IsAny<int>())).Returns((int i) => IntValue);
            mock.Setup(r => r.NextDouble()).Returns(() => DoubleValue);
            mock.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>())).Returns((int min, int max) => min);
            return mock;
        }
    }
}
