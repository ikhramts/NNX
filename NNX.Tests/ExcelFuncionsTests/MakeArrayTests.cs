using System;
using FluentAssertions;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
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

        [Fact]
        public void WhenGivenEmptyArray_Throw()
        {
            var array = new object[0];
            Action action = () => ExcelFunctions.MakeArray(Name, array);
            action.ShouldThrow<NNXException>()
                .WithMessage("*Array cannot be empty.*");
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
    }
}
