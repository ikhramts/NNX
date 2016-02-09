using System.Collections.Generic;
using NNX.Core.Utils;

namespace NNX.Core.Training
{
    /// <summary>
    /// A SimpleGradientTrainer is an on-line gradient descent trainer that applies backpropagation
    /// (with a few adjustments) NumEpochs times. Next input/target pair is selected randomly from 
    /// the training set.
    /// </summary>
    public class SimpleGradientTrainer : BaseTrainer
    {
        public double LearningRate { get; set; }
        public double Momentum { get; set; }
        public double QuadraticRegularization { get; set; }
        public double NumEpochs { get; set; }
        public int BatchSize { get; set; } = 1;
        public double MaxRelativeNoise { get; set; }

        public override void Train(IList<InputOutput> trainingSet,
            IList<InputOutput> validationSet,
            IRandomGenerator rand,
            INeuralNetwork nn)
        {
            var prevWeightGradients = nn.Weights.DeepClone();

            foreach (var gradSet in prevWeightGradients)
            {
                for (var j = 0; j < gradSet.Length; j++)
                    gradSet[j] = 0;
            }

            for (var s = 0; s < NumEpochs; s++)
            {
                var t = rand.Next(trainingSet.Count);
                var inputOutput = trainingSet[t];

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
            }
        }

        public override double GetValidationSetFraction() => 0;

        public void AdjustWeights(INeuralNetwork nn, double[][] weightGradients, double[][] prevWeightGradients)
        {
            var weights = nn.Weights;

            for (var i = 0; i < weightGradients.Length; i++)
            {
                var gradientSubList = weightGradients[i];
                var weightSubList = weights[i];

                for (var j = 0; j < gradientSubList.Length; j++)
                {
                    var prevWeight = weightSubList[j];
                    var fullGradient = gradientSubList[j] + QuadraticRegularization * prevWeight +
                                        Momentum * prevWeightGradients[i][j];
                    weightSubList[j] = prevWeight - LearningRate * fullGradient;
                }
            }
        }

        public override void Validate()
        {
            if (LearningRate <= 0)
                throw new NeuralNetworkException($"Property LearningRate must be positive; was {LearningRate}.");

            if (Momentum < 0)
                throw new NeuralNetworkException($"Property Momentum cannot be negative; was {Momentum}.");

            if (QuadraticRegularization < 0)
                throw new NeuralNetworkException(
                    $"Property QuadraticRegularization cannot be negative; was {QuadraticRegularization}.");

            if (NumEpochs <= 0)
                throw new NeuralNetworkException($"Property NumEpochs must be positive; was {NumEpochs}.");
        }

    }
}
