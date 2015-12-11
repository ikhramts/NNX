using System.Collections.Generic;
using FluentAssertions;
using NeuralNetworks.DataFlow;
using NeuralNetworks.Tests.TestHelpers;
using Xunit;

namespace NeuralNetworks.Tests.DataFlow
{
    public class CsvDataLoaderTests
    {
        [Fact]
        public void ProcessCsv_ShouldInvokeHeaderCallbackOnce()
        {
            var invokeCount = 0;
            var rows = GetSampleInputCsv();
            var csv = rows.ToCsv();
            CsvDataLoader.ProcessCsv(csv, list => { invokeCount++; }, list => { });
            invokeCount.Should().Be(1);
        }

        [Fact]
        public void ProcessCsv_ShouldInvokeRowDataCallbackForEachRow()
        {
            var invokeCount = 0;
            var rows = GetSampleInputCsv();
            var csv = rows.ToCsv();
            CsvDataLoader.ProcessCsv(csv, list => { }, list => { invokeCount++; });
            invokeCount.Should().Be(2);
        }

        [Fact]
        public void ProcessCsv_IfHeaderRowMissing_Throw()
        {
            var rows = GetSampleInputCsv();
            rows.RemoveAt(0);
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => CsvDataLoader.ProcessCsv(csv, list => { }, list => { }));
        }

        [Theory]
        [InlineData("")]
        [InlineData("\r\n\r\n")]
        [InlineData("\r\n")]
        [InlineData(",")]
        [InlineData(",\r\n,")]
        public void ProcessCsv_IfEmptyCsv_Throw(string csv)
        {
            Assert.Throws<NeuralNetworkException>(() => CsvDataLoader.ProcessCsv(csv, list => { }, list => { }));
        }

        [Fact]
        public void ProcessCsv_IfMissingData_Throw()
        {
            var rows = GetSampleInputCsv();
            rows.RemoveAt(2);
            rows.RemoveAt(1);
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => CsvDataLoader.ProcessCsv(csv, list => { }, list => { }));
        }

        [Theory]
        [InlineData("")]
        [InlineData(",")]
        [InlineData(",\r\n,")]
        [InlineData("\r\n")]
        public void ProcessCsv_IfEmptyRowsBeforeHeaders_SkipEmptyRows(string emptyRows)
        {
            var rows = GetSampleInputCsv();
            rows.Insert(0, emptyRows);
            var csv = rows.ToCsv();

            IList<string> invokeArgs = null;
            CsvDataLoader.ProcessCsv(csv, list => { invokeArgs = list; }, list => { });

            invokeArgs.Should().NotBeNull();
            invokeArgs.ShouldBeEquivalentTo(GetExpectedHeaders());
        }

        [Theory]
        [InlineData("")]
        [InlineData(",")]
        [InlineData(",\r\n,")]
        [InlineData("\r\n")]
        public void ProcessCsv_IfEmptyRowsAfterData_SkipEmptyRows(string emptyRows)
        {
            var rows = GetSampleInputCsv();
            rows.Add(emptyRows);
            var csv = rows.ToCsv();

            IList<string> extractedHeaders = null;
            var extractedData = new List<IList<string>>();
            CsvDataLoader.ProcessCsv(csv, headers => { extractedHeaders = headers; }, extractedData.Add);

            extractedHeaders.Should().NotBeNull();
            extractedHeaders.ShouldBeEquivalentTo(GetExpectedHeaders());
            extractedData.Should().HaveCount(2);
            extractedData[1].ShouldBeEquivalentTo(GetExpectedSecondRow());
        }

        [Theory]
        [InlineData("")]
        [InlineData(",")]
        [InlineData(",\r\n,")]
        [InlineData("\r\n")]
        public void ProcessCsv_IfEmptyRowInData_SkipEmptyRow(string emptyRows)
        {
            var rows = GetSampleInputCsv();
            rows.Insert(1, emptyRows);
            var csv = rows.ToCsv();

            var extractedData = new List<IList<string>>();
            CsvDataLoader.ProcessCsv(csv, headers => { }, extractedData.Add);

            extractedData.Should().HaveCount(2);
            extractedData[1].ShouldBeEquivalentTo(GetExpectedSecondRow());
        }

        [Fact]
        public void ProcessCsv_IfHeaderRowStartsWithEmptyCell_Throw()
        {
            var rows = GetSampleInputCsv();
            rows[0] = " ,Header1, Header2, Header3";
            var csv = rows.ToCsv();
            Assert.Throws<NeuralNetworkException>(() => CsvDataLoader.ProcessCsv(csv, list => { }, list => { }));
        }


        public static List<string> GetSampleInputCsv()
        {
            return new List<string>
            {
                "Header1,Header2 Header3, ,OutHeader",
                "12, 3.4, 3.2",
                "-1, 55, 0.0005",
            };
        }

        public static IList<string> GetExpectedFirstRow()
        {
            return new[] { "12", " 3.4", " 3.2" };
        }

        public static IList<string> GetExpectedSecondRow()
        {
            return new[] { "-1", " 55", " 0.0005" };
        }

        public static IList<string> GetExpectedHeaders()
        {
            return new List<string> { "Header1", "Header2", "Header3", " ", "OutHeader" };
        }


        [Fact]
        public void ReadInputHeaders_ShouldExtractHeaders()
        {
            var inputSet = new InputSet();
            var row = new[] {"Header 1", "Header 2"};

            CsvDataLoader.ReadInputHeaders(inputSet, row);
            inputSet.InputHeaders.ShouldBeEquivalentTo(row);
        }

        [Fact]
        public void ReadInputHeaders_IfHeadersHaveStuffAferEmtyCell_IgnoreStuff()
        {
            var inputSet = new InputSet();
            var row = new[] { "Header 1", "Header 2", " ", "Header 3" };

            CsvDataLoader.ReadInputHeaders(inputSet, row);
            inputSet.InputHeaders.ShouldBeEquivalentTo(new[] {"Header 1", "Header 2"});
        }

        [Fact]
        public void ReadInputDataCells_ShouldAppendCurrentRow()
        {
            var inputSet = GetInputSetWithHeaders();
            var firstRow = new[] {1.0};
            inputSet.Inputs.Add(firstRow);
            var newRow = new[] {"2.3 ", " 4.5"};

            CsvDataLoader.ReadInputDataCells(inputSet, newRow);

            inputSet.Inputs.Should().HaveCount(2);
            inputSet.Inputs[0].ShouldBeEquivalentTo(firstRow);
            inputSet.Inputs[1].ShouldBeEquivalentTo(new object[] {2.3, 4.5});
        }

        [Fact]
        public void ReadInputDataCells_IfRowHasStuffAfterEmtyCell_IgnoreStuff()
        {
            var inputSet = GetInputSetWithHeaders();
            var newRow = new[] { "2.3 ", " 4.5" , " ", "blah"};

            CsvDataLoader.ReadInputDataCells(inputSet, newRow);

            inputSet.Inputs.ShouldBeEquivalentTo(new object[] { 2.3, 4.5 });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void ReadInputDataCells_IfDataLengthDoesNotMatchInputWidth_Throw(int numDataElements)
        {
            var inputSet = GetInputSetWithHeaders();
            var row = new List<string>();

            for (var i = 0; i < numDataElements; i++)
                row.Add("1.0");

            Assert.Throws<NeuralNetworkException>(() => CsvDataLoader.ReadInputDataCells(inputSet, row));
        }

        [Fact]
        public void ReadInputDataCells_IfRowContainsNonDouble_Throw(int numDataElements)
        {
            var inputSet = GetInputSetWithHeaders();
            var row = new[] {"1.3", "blah"};
            Assert.Throws<NeuralNetworkException>(() => CsvDataLoader.ReadInputDataCells(inputSet, row));
        }

        public static IInputSet GetInputSetWithHeaders()
        {
            return new InputSet
            {
                InputHeaders = new List<string> { "Header1", "Header2" },
                Inputs = new List<double[]>()
            };
        }

        [Fact]
        public void ReadOutputHeader_MissingTests()
        {
            Assert.True(false);
        }

        [Fact]
        public void ReadOuputDataCell_MissingTests()
        {
            Assert.True(false);
        }
    }
}
