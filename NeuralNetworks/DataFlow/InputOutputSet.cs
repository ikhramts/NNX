using System;
using System.Collections.Generic;
using System.IO;

namespace NeuralNetworks.DataFlow
{
    public class InputOutputSet
    {
        public List<double[]> Inputs;
        public List<double[]> Outputs;
        public List<string> OutputsAsCategories;

        public List<string> InputHeaders;
        public List<string> OutputCategoryOptions;
        public string OutputHeader;

        public int InputWidth { get; private set; }
        public int OutputWidth { get; private set; }

        public static InputOutputSet FromCsv(string csv)
        {
            throw new NotImplementedException();
        }

        public static InputOutputSet FromFile(string filePath)
        {
            return FromCsv(File.ReadAllText(filePath));
        }

        public string ToCsv()
        {
            throw new NotImplementedException();
        }

        public void ToFile(string filePath)
        {
            File.WriteAllText(filePath, ToCsv());
        }
    }
}
