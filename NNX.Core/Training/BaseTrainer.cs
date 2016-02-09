using System;
using System.Collections.Generic;
using NNX.Core.Utils;

namespace NNX.Core.Training
{
    public abstract class BaseTrainer : ITrainer
    {
        //========================= Inheritance contract =========================
        public abstract void Train(IList<InputOutput> trainingSet, 
            IList<InputOutput> validationSet,
            IRandomGenerator rand,
            INeuralNetwork nn);

        public abstract void Validate();

        public abstract double GetValidationSetFraction();

        //========================= Main Interface =========================
        public int Seed { get; set; }
        public bool ShouldInitializeWeights { get; set; } = true;

        public void Train(IList<InputOutput> trainingSet, INeuralNetwork nn)
        {
            Validate();

            if (nn == null)
                throw new ArgumentNullException(nameof(nn));

            var rand = RandomProvider.GetRandom(Seed);

            if (ShouldInitializeWeights)
                InitializeWeights(nn, rand);

            var validationSetFraction = GetValidationSetFraction();

            IList<InputOutput> trainingSubSet;
            IList<InputOutput> validationSubSet = null;

            if (validationSetFraction > 0)
            {
                var split = trainingSet.Shuffle(rand).Split(validationSetFraction);
                validationSubSet = split.First;
                trainingSubSet = split.Second;
            }
            else
            {
                trainingSubSet = trainingSet;
            }

            Train(trainingSubSet, validationSubSet, rand, nn);
        }

        //========================= Misc public helpers =========================
        public static void InitializeWeights(INeuralNetwork nn, IRandomGenerator rand)
        {
            var weights = nn.Weights;

            foreach (var weightsSubList in weights)
            {
                for (int i = 0; i < weightsSubList.Length; i++)
                    weightsSubList[i] = rand.NextDouble() - 0.5;
            }
        }

        public static double GetError(INeuralNetwork nn, IList<InputOutput> testSet)
        {
            var error = 0.0;

            foreach (var inputOutput in testSet)
            {
                var result = nn.FeedForward(inputOutput.Input);
                error += ErrorCalculations.CrossEntropyError(inputOutput.Output, result.Output);
            }

            return error / testSet.Count;
        }

        public static double GetAccuracy(INeuralNetwork nn, IList<InputOutput> testSet)
        {
            var numHits = 0;

            foreach (var inputOutput in testSet)
            {
                var expected = inputOutput.Output.MaxIndex();
                var actual = nn.FeedForward(inputOutput.Input).Output.MaxIndex();

                numHits += expected == actual ? 1 : 0;
            }

            return ((double)numHits) / testSet.Count;
        }

        public static IList<InputOutput> GetBatch(IList<InputOutput> set, int size, IRandomGenerator rand)
        {
            var batch = new InputOutput[size];

            for (var i = 0; i < size; i++)
                batch[i] = set[rand.Next(set.Count)];

            return batch;
        }

        //========================= Private helpers =========================

    }
}
