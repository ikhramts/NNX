using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace NeuralNetworks.DataFlow
{
    public class InputSet : IInputSet
    {
        public List<double[]> Inputs { get; set; }
        public List<string> InputHeaders { get; set; }
        public int InputWidth { get { return InputHeaders.Count; } }

        public static InputSet FromCsv(string csv)
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

            // Get headers.
            var inputSet = new InputSet();
            var headers = GetTrimmedInputCells(csvReader.CurrentRecord);

            if (!headers.Any())
                throw new NeuralNetworkException("Input file header row cannot start with an empty cell.");

            inputSet.InputHeaders = headers;
            double _;

            if (headers.All(h => double.TryParse(h, out _)))
                throw new NeuralNetworkException("Input file is missing the header row.");
            
            // Get data.
            var inputs = new List<double[]>();

            while (true)
            {
                var hasRecord = csvReader.Read();

                if (!hasRecord)
                    break;

                if (csvReader.IsRecordEmpty())
                {
                    continue;
                }

                var dataCells = GetTrimmedInputCells(csvReader.CurrentRecord);

                if (dataCells.Count != inputSet.InputWidth)
                    throw new NeuralNetworkException("Input data file row " + csvReader.Row + ": number of input data cells" +
                                                        " does not match number if input headers.");

                var inputRow = new double[inputSet.InputWidth];

                for (var i = 0; i < inputSet.InputWidth; i++)
                {
                    double doubleValue;

                    if (!double.TryParse(dataCells[i], out doubleValue))
                    {
                        var message =
                            string.Format("Input data file row {0} column {1}: cannot convert '{2}' to number.",
                                csvReader.Row, i, dataCells[i]);
                        throw new NeuralNetworkException(message);
                    }

                    inputRow[i] = doubleValue;
                }

                inputs.Add(inputRow);
            }

            if (inputs.Count == 0)
                throw new NeuralNetworkException("Input file does not contain data.");

            inputSet.Inputs = inputs;

            return inputSet;
        }

        public static InputSet FromFile(string filePath)
        {
            return FromCsv(File.ReadAllText(filePath));
        }

        private static List<string> GetTrimmedInputCells(IEnumerable<string> entireRow)
        {
            var cleanedCells = new List<string>();

            foreach (var cell in entireRow)
            {
                if (string.IsNullOrWhiteSpace(cell))
                    break;

                cleanedCells.Add(cell.Trim());
            }

            return cleanedCells;
        }
    }
}
