using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NeuralNetworks.Tests
{
    public class ListExtensionTests
    {
        [Fact]
        public void Shuffle_ShouldPreserveOriginalList()
        {
            var list = new List<int> {1, 2, 3, 4};
            var expected = new List<int>(list);
            list.Shuffle(new Random(2));

            Assert.Equal(expected, list);
        }

        [Fact]
        public void Shuffle_ShouldShuffle()
        {
            var list = new List<int> { 1, 2, 3, 4 };
            var result = list.Shuffle(new Random(2));

            Assert.NotNull(result);
            Assert.Equal(list, result.OrderBy(i => i).ToList());
            Assert.NotEqual(list, result);
        }

    }
}
