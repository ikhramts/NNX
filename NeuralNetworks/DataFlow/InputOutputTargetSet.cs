using System;
using System.Collections.Generic;
using System.IO;

namespace NeuralNetworks.DataFlow
{
    public class InputOutputTargetSet
    {
        public List<double[]> Inputs;
        public List<double[]> Outputs;
        public List<double[]> Targets;
        public List<string> OutputsAsCategories;
        public List<string> TargetsAsCategories;

        public List<string> InputHeaders;
        public List<string> OutputCategoryOptions;
        public string OutputHeader;

        public int InputWidth { get; private set; }
        public int OutputWidth { get; private set; }

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
