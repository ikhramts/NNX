using System;
using System.Collections.Generic;

namespace NeuralNetworks.Training
{
    public interface ITrainer
    {
        TrainerConfig Config { get; set; }
        INeuralNetwork Train(IList<InputOutput> trainingSet);
    }
}
