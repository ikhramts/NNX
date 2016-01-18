using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class MakeWeightsTests : ObjectStoreTestBase
    {
        [Fact]
        public void WhenGivenArrayList_ShouldMakeWeights()
        {
            var array1 = new[] {1.1, 1.2};
            var array2 = new[] {2.1, 2.2, 2.3};
            ExcelFunctions.MakeArray("array1", array1.Cast<object>().ToArray());
            ExcelFunctions.MakeArray("array2", array2.Cast<object>().ToArray());
            ExcelFunctions.MakeWeights("weights", new[] {"array1", "array2"});

            var result = ObjectStore.Get<double[][]>("weights");
            result.Should().HaveCount(2);
            result[0].Should().Equal(array1);
            result[1].Should().Equal(array2);
        }

        [Fact]
        public void ShouldReturnObjectName()
        {
            var array1 = new[] { 1.1, 1.2 };
            var array2 = new[] { 2.1, 2.2, 2.3 };
            ExcelFunctions.MakeArray("array1", array1.Cast<object>().ToArray());
            ExcelFunctions.MakeArray("array2", array2.Cast<object>().ToArray());
            var name = ExcelFunctions.MakeWeights("weights", new[] { "array1", "array2" });
            name.Should().Be("weights");
        }

        [Theory]
        [MemberData("WhenWeightsArrayIsNullOrEmpty_Throw_Cases")]
        public void WhenWeightsArrayIsNullOrEmpty_Throw(string[] bad)
        {
            Action action = () => ExcelFunctions.MakeWeights("weights", bad);
            action.ShouldThrow<NNXException>()
                .WithMessage("*Argument WeightArrays cannot be null or empty.*");
        }

        public static IEnumerable<object[]> WhenWeightsArrayIsNullOrEmpty_Throw_Cases()
        {
            return new[]
            {
                new object[] {null},
                new object[] {new string[0]},
            };
        }

        [Fact]
        public void WhenAnyWeightsArrayElementDoesNotPointToArray_Throw()
        {
            var array1 = new[] { 1.1, 1.2 };
            ExcelFunctions.MakeArray("array1", array1.Cast<object>().ToArray());
            Action action = () => ExcelFunctions.MakeWeights("weights", new [] {"array1", "array2"});
            action.ShouldThrow<NNXException>()
                .WithMessage("*Element at index 2 ('array2') does not point to a valid array of numbers.*");
        }

        [Theory]
        [MemberData("WhenAnyWeightsArrayElementIsNotDoubleArray_Throw_Cases")]
        public void WhenAnyWeightsArrayElementIsNotDoubleArray_Throw(object bad)
        {
            var array1 = new[] { 1.1, 1.2 };
            ExcelFunctions.MakeArray("array1", array1.Cast<object>().ToArray());
            ObjectStore.Add("bad", bad);
            Action action = () => ExcelFunctions.MakeWeights("weights", new[] { "array1", "bad" });
            action.ShouldThrow<NNXException>()
                .WithMessage("*Element at index 2 ('bad') does not point to a valid array of numbers.*");
        }

        public static IEnumerable<object[]> WhenAnyWeightsArrayElementIsNotDoubleArray_Throw_Cases()
        {
            return new[]
            {
                new object[] {new string[0]},
                new object[] {new object()},
            };
        }

    }
}
