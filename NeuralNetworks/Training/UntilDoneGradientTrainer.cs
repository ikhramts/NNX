using System.Collections.Generic;
using NeuralNetworks.Utils;

namespace NeuralNetworks.Training
{
    /// <summary>
    /// An on-line gradient descent trainer that radnomly splits the input trainingSet into a training and validation
    /// subsets. The trainier will keep selecting a random input/target pair from the training set and applying 
    /// backpropagation until cross-entropy error on the validation set does not improve after
    /// MaxEpochsWithoutImprovement. The trainer will terminate after NumEpochs even if MaxEpochsWithoutImprovement
    /// condition has not been reached.
    /// </summary>
    public class UntilDoneGradientTrainer : SimpleGradientTrainer
    {
        public double ValidationSetFraction { get; set; }
        public double MaxEpochsWithoutImprovement { get; set; }

        public override void Train(IList<InputOutput> trainingSet,
            IList<InputOutput> validationSet,
            IRandomGenerator rand,
            INeuralNetwork nn)
        {
            base.Train(trainingSet, validationSet, rand, nn);
        }

        public void Train(IList<InputOutput> trainingSet, IList<InputOutput> validationSet,
            INeuralNetwork nn, IRandomGenerator rand)
        {
            
        }

        public override void Validate()
        {
            base.Validate();

            if (ValidationSetFraction <= 0 || ValidationSetFraction >= 1)
                throw new NeuralNetworkException("Property ValidationSetFraction must be strictly between 0 and 1; was " +
                                                 $"{ValidationSetFraction}.");

            if (MaxEpochsWithoutImprovement <= 0)
                throw new NeuralNetworkException("Property MaxEpochsWithoutImprovement must be positive; was " +
                                                 $"{MaxEpochsWithoutImprovement}.");
        }
    }
}
