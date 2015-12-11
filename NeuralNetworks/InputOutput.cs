using System;

namespace NeuralNetworks
{
    public class InputOutput
    {
        public double[] Input { get; set; }
        public double[] Output { get; set; }

        public static InputOutput FromCsvRow(string[] csvCells)
        {
            var endInput = Array.IndexOf(csvCells, "");

            if (endInput == -1)
                throw new Exception("No end of input found.");

            var inputTarget = new InputOutput
            {
                Input = new double[endInput],
                Output = new double[csvCells.Length - endInput - 1]
            };

            for (var i = 0; i < csvCells.Length; i++)
            {
                if (i < endInput)
                    inputTarget.Input[i] = double.Parse(csvCells[i]);

                if (i > endInput)
                    inputTarget.Output[i - endInput - 1] = double.Parse(csvCells[i]);
            }

            return inputTarget;
        }
    }
}
