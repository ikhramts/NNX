using System.Collections.Generic;
using FluentAssertions;
using NeuralNetworks.DataFlow;
using NeuralNetworks.Tests.TestHelpers;
using Xunit;

namespace NeuralNetworks.Tests.DataFlow
{
    public class InputSetTests
    {
        [Fact]
        public void FromCsv_ShouldLoadInputs()
        {
            var csv = GetSampleInputCsv().ToCsv();
            var set = InputSet.FromCsv(csv);

            set.Should().NotBeNull();
            set.Inputs.Should().NotBeNullOrEmpty();
            set.Inputs.Should().HaveCount(2);
            set.Inputs[0].ShouldBeEquivalentTo(GetExpectedFirstRow());
            set.Inputs[1].ShouldBeEquivalentTo(GetExpectedSecondRow());
        }

        [Fact]
        public void FromCsv_ShouldLoadInputHeaders()
        {
            var csv = GetSampleInputCsv().ToCsv();
            var set = InputSet.FromCsv(csv);

            set.Should().NotBeNull();
            set.InputHeaders.Should().NotBeNullOrEmpty();
            set.InputHeaders.ShouldBeEquivalentTo(GetExpectedHeaders());
        }

        [Fact]
        public void FromCsv_ShouldLoadInputWidth()
        {
            var csv = GetSampleInputCsv().ToCsv();
            var set = InputSet.FromCsv(csv);

            set.Should().NotBeNull();
            set.InputWidth.ShouldBeEquivalentTo(3);
        }

        [Fact]
        public void FromCsv_IfHeaderRowMissing_Throw()
        {
            var rows = GetSampleInputCsv();
            rows.RemoveAt(0);
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => InputSet.FromCsv(csv));
        }

        [Theory]
        [InlineData("Header1, Header2")]
        [InlineData("Header1, Header2,")]
        [InlineData("Header1, ,Header3")]
        public void FromCsv_IfSomeHeadersMissing_Throw(string badHeaders)
        {
            var rows = GetSampleInputCsv();
            rows[0] = badHeaders;
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => InputSet.FromCsv(csv));
        }

        [Theory]
        [InlineData("2.3")]
        [InlineData("2.3, 2.4, 2.5, 2.6")]
        public void FromCsv_IfInconsistentInputWidth_Throw(string badRow)
        {
            var rows = GetSampleInputCsv();
            rows.Add(badRow);
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => InputSet.FromCsv(csv));
        }

        [Theory]
        [InlineData("")]
        [InlineData("\r\n\r\n")]
        [InlineData("\r\n")]
        [InlineData(",")]
        [InlineData(",\r\n,")]
        public void FromCsv_IfEmptyCsv_Throw(string csv)
        {
            Assert.Throws<NeuralNetworkException>(() => InputSet.FromCsv(csv));
        }

        [Fact]
        public void FromCsv_IfMissingData_Throw()
        {
            var rows = GetSampleInputCsv();
            rows.RemoveAt(2);
            rows.RemoveAt(1);
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => InputSet.FromCsv(csv));
        }

        [Theory]
        [InlineData("")]
        [InlineData(",")]
        [InlineData(",\r\n,")]
        [InlineData("\r\n")]
        public void FromCsv_IfEmptyRowsBeforeHeaders_SkipEmptyRows(string emptyRows)
        {
            var rows = GetSampleInputCsv();
            rows.Insert(0, emptyRows);
            var csv = rows.ToCsv();
            var set = InputSet.FromCsv(csv);

            set.Should().NotBeNull();
            set.InputHeaders.ShouldBeEquivalentTo(GetExpectedHeaders());
        }

        [Theory]
        [InlineData("")]
        [InlineData(",")]
        [InlineData(",\r\n,")]
        [InlineData("\r\n")]
        public void FromCsv_IfEmptyRowsAfterData_SkipEmptyRows(string emptyRows)
        {
            var rows = GetSampleInputCsv();
            rows.Add(emptyRows);
            var csv = rows.ToCsv();
            var set = InputSet.FromCsv(csv);

            set.Should().NotBeNull();
            set.Inputs.Should().HaveCount(2);
            set.Inputs[1].ShouldBeEquivalentTo(GetExpectedSecondRow());
        }

        [Theory]
        [InlineData("")]
        [InlineData(",")]
        [InlineData(",\r\n,")]
        [InlineData("\r\n")]
        public void FromCsv_IfEmptyRowInData_SkipEmptyRow(string emptyRows)
        {
            var rows = GetSampleInputCsv();
            rows.Insert(1, emptyRows);
            var csv = rows.ToCsv();
            var set = InputSet.FromCsv(csv);

            set.Inputs.Should().HaveCount(2);
            set.Inputs[1].ShouldBeEquivalentTo(GetExpectedSecondRow());
        }

        [Fact]
        public void FromCsv_IfRowHasDataAfterEmptyCell_IngnoreData()
        {
            var rows = GetSampleInputCsv();
            rows[1] = "12, 3.4, 3.2,,Blah";
            var csv = rows.ToCsv();
            var set = InputSet.FromCsv(csv);
            set.Should().NotBeNull();
            set.Inputs.Should().NotBeNullOrEmpty();
            set.Inputs[0].ShouldBeEquivalentTo(GetExpectedFirstRow());
        }

        [Fact]
        public void FromCsv_IfAnyDataCellIsNotDouble_Throw()
        {
            var rows = GetSampleInputCsv();
            rows[1] = "2, Blah, 3.0";
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => InputSet.FromCsv(csv));
        }

        [Theory]
        [InlineData("Header1, Header2")]
        [InlineData("Header1, Header2, Header3, Header4")]
        public void FromCsv_IfNumHeadersDoesNotMatchDataWidth_Throw(string badHeaders)
        {
            var rows = GetSampleInputCsv();
            rows[0] = badHeaders;
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => InputSet.FromCsv(csv));
        }

        [Fact]
        public void FromCsv_IfHasStuffAfterHeaders_IgnoreStuff()
        {
            var rows = GetSampleInputCsv();
            rows[0] = "Header1, Header2, Header3, , Blah";
            var csv = rows.ToCsv();
            var set = InputSet.FromCsv(csv);
            set.Should().NotBeNull();
            set.InputHeaders.ShouldBeEquivalentTo(GetExpectedHeaders());
        }

        [Fact]
        public void FromCsv_IfHeaderRowStartsWithEmptyCell_Throw()
        {
            var rows = GetSampleInputCsv();
            rows[0] = ",Header1, Header2, Header3";
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => InputSet.FromCsv(csv));
        }


        public static List<string> GetSampleInputCsv()
        {
            return new List<string>
            {
                "Header1, Header2, Header3",
                "12, 3.4, 3.2",
                "-1, 55, 0.0005",
            };
        }

        public static double[] GetExpectedFirstRow()
        {
            return new[] {12.0, 3.4, 3.2};
        }

        public static double[] GetExpectedSecondRow()
        {
            return new[] {-1.0, 55.0, 0.0005};
        }

        public static List<string> GetExpectedHeaders()
        {
            return new List<string> { "Header1", "Header2", "Header3" };
        }
    }
}
