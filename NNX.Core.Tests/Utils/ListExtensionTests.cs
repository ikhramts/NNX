using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NNX.Core.Tests.Assertions;
using NNX.Core.Tests.TestObjects;
using NNX.Core.Utils;
using Xunit;

namespace NNX.Core.Tests.Utils
{
    public class ListExtensionTests
    {
        [Fact]
        public void Shuffle_ShouldPreserveOriginalList()
        {
            var list = new List<int> {1, 2, 3, 4};
            var expected = new List<int>(list);
            list.Shuffle(RandomProvider.GetRandom(2));

            Assert.Equal(expected, list);
        }

        [Fact]
        public void Shuffle_ShouldShuffle()
        {
            var list = new List<int> { 1, 2, 3, 4 };
            var result = list.Shuffle(RandomProvider.GetRandom(2));

            Assert.NotNull(result);
            Assert.Equal(list, result.OrderBy(i => i).ToList());
            Assert.NotEqual(list, result);
        }

        [Theory]
        [MemberData("Split_ShouldSplit_Cases")]
        public void Split_ShouldSplit(int[] input, double fraction, IList<int> first, IList<int> second)
        {
            var result = input.Split(fraction);
            result.Should().NotBeNull();
            result.First.Should().Equal(first);
            result.Second.Should().Equal(second);
        }

        public static IEnumerable<object[]> Split_ShouldSplit_Cases()
        {
            yield return new object[] { new[] { 1, 2, 3, 4 }, 0.1, new int[] { }, new[] { 1, 2, 3, 4 } };
            yield return new object[] { new[] { 1, 2, 3, 4 }, 0.24, new int[] { }, new[] { 1, 2, 3, 4 } };
            yield return new object[] { new[] { 1, 2, 3, 4 }, 0.25, new[] { 1 }, new[] { 2, 3, 4 } };
            yield return new object[] { new[] { 1, 2, 3, 4 }, 0.3, new[] { 1 }, new[] { 2, 3, 4 } };
            yield return new object[] { new[] { 1, 2, 3, 4 }, 0.49, new[] { 1 }, new[] { 2, 3, 4 } };
            yield return new object[] { new[] { 1, 2, 3, 4 }, 0.5, new[] { 1, 2 }, new[] { 3, 4 } };
            yield return new object[] { new[] { 1, 2, 3, 4 }, 0.55, new[] { 1, 2 }, new[] { 3, 4 } };
            yield return new object[] { new[] { 1, }, 0.5, new int[] { }, new[] { 1 } };
            yield return new object[] { new int[] {  }, 0.5, new int[] { }, new int [] {} };
        }

        [Fact]
        public void Split_IfListIsNull_Throw()
        {
            IList<int> bad = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Action action = () => bad.Split(0.5);
            action.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void Split_IfFractionInFirstNotBetween0And1_Throw(double bad)
        {
            var list = new[] {1, 2, 3, 4};
            Action action = () => list.Split(bad);
            action.ShouldThrow<ArgumentOutOfRangeException>()
                .WithMessage($"*Argument fractionInFirst must be between 0 and 1; was {bad}.*")
                .Where(e => ((double) e.ActualValue) == bad || double.IsNaN(((double)e.ActualValue)));
        }

        [Fact]
        public void AddInPlace_ShouldAdd()
        {
            var left = new[] {new[] {0.1, 0.2}, new[] {0.3, 0.4, 0.5 } };
            var right = new[] {new[] {1.0, 2.0}, new[] {3.0, 4.0, 5.0 } };
            var expected = new[] { new[] { 1.1, 2.2 }, new[] { 3.3, 4.4, 5.5 } };

            left.AddInPlace(right);
            left.Should().NotBeNullOrEmpty();
            left.Should().HaveCount(2);
            left[0].ShouldApproximatelyEqual(expected[0]);
            left[1].ShouldApproximatelyEqual(expected[1]);
        }

        [Fact]
        public void AddInPlace_IfLengthsDontMatch_ThrowArgumentException()
        {
            var target = new[] { new[] { 0.1, 0.2 },  };
            var other = new[] { new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 } };

            Action action = () => target.AddInPlace(other);
            action.ShouldThrow<ArgumentException>()
                .WithMessage("*Argument 'other' should have same length as the target array (1); was 2.*")
                .Where(e => e.ParamName == "other");
        }

        [Fact]
        public void AddInPlace_IfSubLengthsDontMatch_ThrowArgumentExcetpion()
        {
            var target = new[] { new[] { 0.1, 0.2 }, new[] { 0.3 } };
            var other = new[] { new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 } };

            Action action = () => target.AddInPlace(other);
            action.ShouldThrow<ArgumentException>()
                .WithMessage("*Argument 'other' and target array have mismatching sub-array at index 1; " +
                             "target subarray length: 1, other subarray length: 2.*")
                .Where(e => e.ParamName == "other");
        }

        [Fact]
        public void AddInPlace_IfThisIsNull_ThrowArgumentNullException()
        {
            double[][] target = null;
            var other = new[] { new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 } };

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action = () => target.AddInPlace(other);
            action.ShouldThrow<ArgumentNullException>()
                .Where(e => e.ParamName == "target");
        }

        [Fact]
        public void AddInPlace_IfOtherIsNull_ThrowArgumentNullException()
        {
            var target = new[] { new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 } };
            double[][] other = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Action action = () => target.AddInPlace(other);
            action.ShouldThrow<ArgumentNullException>()
                .Where(e => e.ParamName == "other");
        }

        [Fact]
        public void AddInPlace_IfThisIsEmptyAndOtherIsEmpty_ThrowArgumentException()
        {
            var target = new double[0][];
            var other = new double[0][];

            Action action = () => target.AddInPlace(other);
            action.ShouldThrow<ArgumentException>()
                .WithMessage("*Argument 'target' cannot be empty.*")
                .Where(e => e.ParamName == "target");
        }

        [Fact]
        public void MultiplyInPlace_ShouldMultiply()
        {
            var target = new[] { new[] { 0.1, 0.2 }, new[] { 0.3 } };
            var multiplier = 2.0;
            var expected = new[] { new[] { 0.2, 0.4 }, new[] { 0.6 } };

            target.MultiplyInPlace(multiplier);
            target.Should().NotBeNullOrEmpty();
            target.Should().HaveCount(2);
            target[0].ShouldApproximatelyEqual(expected[0]);
            target[1].ShouldApproximatelyEqual(expected[1]);
        }

        [Fact]
        public void MultiplyInPlace_IfThisIsNull_ThrowArgumentNullException()
        {
            var multiplier = 2.0;
            Action action = () => ((double[][]) null).MultiplyInPlace(multiplier);

            action.ShouldThrow<ArgumentNullException>()
                .Where(e => e.ParamName == "target");
        }

        [Fact]
        public void MultiplyInPlace_IfThisIsEmpty_ThrowArgumentException()
        {
            var target = new double[0][];
            const double multiplier = 2.0;
            Action action = () => target.MultiplyInPlace(multiplier);
            action.ShouldThrow<ArgumentException>()
                .WithMessage("*Argument 'target' cannot be empty.*")
                .Where(e => e.ParamName == "target");
        }

        [Fact]
        public void MultiplyInPlace_IfMultiplierIsNaN_ThrowArgumentException()
        {
            var target = new[] { new[] { 0.1, 0.2 }, new[] { 0.3 } };
            const double multiplier = double.NaN;
            Action action = () => target.MultiplyInPlace(multiplier);
            action.ShouldThrow<ArgumentException>()
                .WithMessage("*Argument 'multiplier' cannot be NaN.*")
                .Where(e => e.ParamName == "multiplier");
        }

        [Theory]
        [InlineData(0, 0.5, -0.5)]
        [InlineData(0.5, 1.0, 0.0)]
        [InlineData(0.75, 0, 0.0)]
        [InlineData(0.1, 2, -1.6)]
        public void AddNoise_ShouldAddNoise(double randomValue, double maxNoise, double expectedOffset)
        {
            var rand = MockRandom.Get(randomValue);
            var input = new[] {0.0, 0.2};

            var expected = new[] { 0, 0.2 * (1 + expectedOffset) };
            var result = input.AddRelativeNoise(maxNoise, rand);

            result.ShouldApproximatelyEqual(expected);
        }

    }
}
