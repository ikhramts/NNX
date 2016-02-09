using System.Collections.Generic;
using NNX.Core.Utils;

namespace NNX.Core.Training
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
        public int MaxEpochsWithoutImprovement { get; set; }
        public int EpochsBetweenValidations { get; set; } = 1;

        public override void Train(IList<InputOutput> trainingSet,
            IList<InputOutput> validationSet,
            IRandomGenerator rand,
            INeuralNetwork nn)
        {
            var bestWeights = nn.Weights.DeepClone();
            var bestError = GetError(nn, validationSet);
            var epochsSinceLastImprovement = 0;
            var epochsToNextTest = EpochsBetweenValidations;

            var prevWeightGradients = nn.Weights.DeepClone();

            foreach (var gradSet in prevWeightGradients)
            {
                for (var j = 0; j < gradSet.Length; j++)
                    gradSet[j] = 0;
            }

            for (var epoch = 1; epoch <= NumEpochs; epoch++)
            {
                var batch = GetBatch(trainingSet, BatchSize, rand);

                var gradients = nn.Weights.DeepCloneToZeros();

                for (var j = 0; j < BatchSize; j++)
                {
                    gradients.AddInPlace(
                        nn.CalculateGradients(batch[j].Input.AddRelativeNoise(MaxRelativeNoise, rand), batch[j].Output));
                }

                gradients.MultiplyInPlace(1 / ((double)BatchSize));

                //var gradients = nn.CalculateGradients(inputOutput.Input, inputOutput.Output);
                AdjustWeights(nn, gradients, prevWeightGradients);
                gradients.DeepCopyTo(prevWeightGradients);

                // Check against validation set.
                epochsToNextTest--;

                if (epochsToNextTest == 0)
                {
                    epochsToNextTest = EpochsBetweenValidations;
                    var error = GetError(nn, validationSet);

                    if (error < bestError)
                    {
                        nn.Weights.DeepCopyTo(bestWeights);
                        epochsSinceLastImprovement = 0;
                    }
                    else
                    {
                        epochsSinceLastImprovement += EpochsBetweenValidations;

                        if (epochsSinceLastImprovement > MaxEpochsWithoutImprovement)
                            break;
                    }
                }
            }

            bestWeights.DeepCopyTo(nn.Weights);
        }

        public override double GetValidationSetFraction()
        {
            return ValidationSetFraction;
        }

        public override void Validate()
        {
            base.Validate();

            if (ValidationSetFraction <= 0 || ValidationSetFraction >= 1)
                throw new NeuralNetworkException("Property ValidationSetFraction must be strictly between 0 and 1; was " +
                                                 $"{ValidationSetFraction}.");

            if (MaxEpochsWithoutImprovement <= 0)
                throw new NeuralNetworkException("Property MaxEpochsWithoutImprovement must not be negative; was " +
                                                 $"{MaxEpochsWithoutImprovement}.");

            if (EpochsBetweenValidations <= 0)
                throw new NeuralNetworkException("Property EpochsBetweenValidations must be positive; was " +
                                                 $"{EpochsBetweenValidations}.");
        }
    }
}
