using System.Collections.Generic;
using NNX.Core.Utils;

namespace NNX.Core.Training
{
    public interface ITrainer
    {
        /// <summary>
        /// High level entry point. Just provide a training set and the neural network,
        /// everything else will be taken care of.
        /// </summary>
        void Train(IList<InputOutput> trainingSet, INeuralNetwork nn);

        /// <summary>
        /// Lower-level entry point when more precise control over random number generator
        /// and validation set is needed.  Some trainers might not use validation set or
        /// random number generator.
        /// </summary>
        void Train(IList<InputOutput> trainingSet, IList<InputOutput> validationSet,
            IRandomGenerator rand, INeuralNetwork nn);
    }
}
