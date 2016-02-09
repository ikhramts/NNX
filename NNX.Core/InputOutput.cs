using System;
using NNX.Core.Utils;

namespace NNX.Core
{
    public class InputOutput
    {
        public double[] Input { get; set; }
        public double[] Output { get; set;  }

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

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;

            return Equals((InputOutput)obj);
        }

        protected bool Equals(InputOutput other)
        {
            return Input.ListEquals(other.Input) && Output.ListEquals(other.Output);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Input.GetListHashCode() * 397) ^ Output.GetListHashCode();
            }
        }
    }
}
