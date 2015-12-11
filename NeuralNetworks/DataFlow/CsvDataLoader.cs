using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace NeuralNetworks.DataFlow
{
    public static class CsvDataLoader
    {
        public static void ProcessCsv(string csv, Action<IList<string>> onReadHeaders, Action<IList<string>> onReadDataRow)
        {
            var csvReader = new CsvReader(new StringReader(csv));
            csvReader.Configuration.HasHeaderRecord = false;

            // Skip leading empty rows.
            while (true)
            {
                var hasRecord = csvReader.Read();

                if (!hasRecord)
                    throw new NeuralNetworkException("Input file is empty");

                if (!csvReader.IsRecordEmpty())
                    break;
            }

            var headers = csvReader.CurrentRecord;

            if (string.IsNullOrWhiteSpace(headers[0]))
                throw new NeuralNetworkException("Input file headers should be empty");

            double dummy;
            if (double.TryParse(headers[0], out dummy))
                throw new NeuralNetworkException("Input file headers should not be numbers");

            onReadHeaders(csvReader.CurrentRecord);


            throw new NotImplementedException();
            // Get headers.
            //var inputSet = new InputSet();
            //var headers = GetTrimmedInputCells(csvReader.CurrentRecord);

            //if (!headers.Any())
            //    throw new NeuralNetworkException("Input file header row cannot start with an empty cell.");

            //inputSet.InputHeaders = headers;
            //double _;

            //if (headers.All(h => double.TryParse(h, out _)))
            //    throw new NeuralNetworkException("Input file is missing the header row.");

            //// Get data.
            //var inputs = new List<double[]>();

            //while (true)
            //{
            //    var hasRecord = csvReader.Read();

            //    if (!hasRecord)
            //        break;

            //    if (csvReader.IsRecordEmpty())
            //    {
            //        continue;
            //    }

            //    var dataCells = GetTrimmedInputCells(csvReader.CurrentRecord);

            //    if (dataCells.Count != inputSet.InputWidth)
            //        throw new NeuralNetworkException("Input data file row " + csvReader.Row + ": number of input data cells" +
            //                                            " does not match number if input headers.");

            //    var inputRow = new double[inputSet.InputWidth];

            //    for (var i = 0; i < inputSet.InputWidth; i++)
            //    {
            //        double doubleValue;

            //        if (!double.TryParse(dataCells[i], out doubleValue))
            //        {
            //            var message =
            //                string.Format("Input data file row {0} column {1}: cannot convert '{2}' to number.",
            //                    csvReader.Row, i, dataCells[i]);
            //            throw new NeuralNetworkException(message);
            //        }

            //        inputRow[i] = doubleValue;
            //    }

            //    inputs.Add(inputRow);
            //}

            //if (inputs.Count == 0)
            //    throw new NeuralNetworkException("Input file does not contain data.");


        }

        public static void ReadInputHeaders(IInputSet inputSet, IList<string> headers)
        {
            
        }

        public static void ReadOutputHeader(IOutputSet outputSet, IList<string> headers)
        {
            
        }

        public static void ReadInputDataCells(IInputSet inputSet, IList<string> row)
        {
            
        }

        public static void ReadOuputDataCell(IInputSet inputSet, IList<string> row)
        {

        }
    }
}
