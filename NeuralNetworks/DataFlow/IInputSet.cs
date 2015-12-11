using System.Collections.Generic;

namespace NeuralNetworks.DataFlow
{
    public interface IInputSet
    {
        List<double[]> Inputs { get; set; }
        List<string> InputHeaders { get; set; }
        int InputWidth { get; } 
    }
}
