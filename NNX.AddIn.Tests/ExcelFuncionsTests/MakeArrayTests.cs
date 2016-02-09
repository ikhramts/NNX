using System;
using System.Collections.Generic;
using ExcelDna.Integration;
using FluentAssertions;
using Xunit;

namespace NNX.AddIn.Tests.ExcelFuncionsTests
{
    public class MakeArrayTests : ObjectStoreTestBase
    {
        private const string Name = "array";

        [Fact]
        public void WhenGivenString_ShouldMakeStringArray()
        {
            var array = new object[] {"one", "two", "three"};
            ExcelFunctions.MakeArray(Name, array);
            var result = ObjectStore.Get<string[]>(Name);
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(3);
            result[0].Should().Be((string)array[0]);
            result[1].Should().Be((string)array[1]);
            result[2].Should().Be((string)array[2]);
        }

        [Fact]
        public void WhenGivenDoubles_ShoudMakeDoubleArray()
        {
            var array = new object[] { 1.1, 1.2, 1.3 };
            ExcelFunctions.MakeArray(Name, array);
            var result = ObjectStore.Get<double[]>(Name);
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(3);
            result[0].Should().Be((double)array[0]);
            result[1].Should().Be((double)array[1]);
            result[2].Should().Be((double)array[2]);
        }

        [Fact]
        public void WhenGivenStringsAndDoubles_ShouldThrow()
        {
            var array = new object[] { "one", 1.2, 1.3 };
            Action action = () => ExcelFunctions.MakeArray(Name, array);
            action.ShouldThrow<NNXException>()
                .WithMessage("*Array must contain either only numbers or only strings. " +
                             "Encountered both in this array.*");
        }

        [Theory]
        [MemberData("WhenGivenEmptyArray_Throw_Cases")]
        public void WhenGivenEmptyArray_Throw(object[] emtpyArray)
        {
            Action action = () => ExcelFunctions.MakeArray(Name, emtpyArray);
            action.ShouldThrow<NNXException>()
                .WithMessage("*Array cannot be empty.*");
        }

        public static IEnumerable<object[]> WhenGivenEmptyArray_Throw_Cases()
        {
            return new[]
            {
                new object[] {new object[0]},
                new object[] {new object[] {null, null}},
                new object[] {new object[] {ExcelEmpty.Value, null}},
            };
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void WhenGivenNeitherDoubleNorString_Throw(int position)
        {
            var array = new object[] { 1.1, 1.2, 1.3 };
            array[position] = new object();

            Action action = () => ExcelFunctions.MakeArray(Name, array);
            action.ShouldThrow<NNXException>()
                .WithMessage("*Array elements must be either numbers or strings. Element" +
                             $" at position {position} was neither number nor string.*");
        }

        [Theory]
        [InlineData("one")]
        [InlineData(1.1)]
        [InlineData(1.0)]
        public void ShouldReturnObjectName(object obj)
        {
            var array = new object[1];
            array[0] = obj;
            var name = ExcelFunctions.MakeArray(Name, array);
            name.Should().Be(Name);
        }

        [Theory]
        [MemberData("IfCellIsEmpty_ShouldSkipCell_Cases")]
        public void IfCellIsEmpty_ShouldSkipCell(object empty)
        {
            var array = new[] { 1.0, empty, 1.2 };
            var expected = new[] { 1.0, 1.2 };
            ExcelFunctions.MakeArray(Name, array);
            var result = ObjectStore.Get<double[]>(Name);
            result.Should().Equal(expected);
        }

        [Theory]
        [MemberData("IfCellIsEmpty_ShouldSkipCell_Cases")]
        public void IfFirstCellIsEmpty_ShouldSkipCell_Numbers(object empty)
        {
            var array = new[] { empty, 1.0, 1.2 };
            var expected = new[] { 1.0, 1.2 };
            ExcelFunctions.MakeArray(Name, array);
            var result = ObjectStore.Get<double[]>(Name);
            result.Should().Equal(expected);
        }

        [Theory]
        [MemberData("IfCellIsEmpty_ShouldSkipCell_Cases")]
        public void IfFirstCellIsEmpty_ShouldSkipCell_Strings(object empty)
        {
            var array = new[] { empty, "one", "two" };
            var expected = new[] { "one", "two" };
            ExcelFunctions.MakeArray(Name, array);
            var result = ObjectStore.Get<string[]>(Name);
            result.Should().Equal(expected);
        }

        public static IEnumerable<object[]> IfCellIsEmpty_ShouldSkipCell_Cases()
        {
            return new[]
            {
                new object[] {null},
                new object[] {ExcelEmpty.Value},
            };
        }



    }
}
