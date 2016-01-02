using System.Collections.Generic;

namespace NeuralNetworks.Training
{
    public interface ITrainer
    {
        INeuralNetwork Train(IList<InputOutput> trainingSet, INeuralNetwork nn);
    }
}
