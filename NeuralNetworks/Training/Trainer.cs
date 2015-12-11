using System;
using System.Collections.Generic;

namespace NeuralNetworks.Training
{
    public class Trainer
    {
        public TrainerConfig Config { get; set; }

        public Trainer() { }

        public Trainer(TrainerConfig config)
        {
            Config = config;
        }

        public INeuralNetwork Train(IList<InputOutput> trainingSet, Random customRand = null)
        {
            if (Config == null)
                throw new NeuralNetworkException("Trainer is missing Config property.");

            var numEpochs = Config.NumEpochs;

            if (numEpochs <= 0)
                throw new NeuralNetworkException("Config.NumEpochs property should be a positive integer.  Was: " + Config.NumEpochs);

            if (Config.NeuralNetworkConfig == null)
                throw new NeuralNetworkException("Config property is missing NeuralNetworkConfig.");

            var rand = customRand ?? new Random();
            var nn = NeuralNetworkBuilder.Build(Config.NeuralNetworkConfig);
            var prevWeightGradients = nn.Weights.DeepClone();

            foreach (var gradSet in prevWeightGradients)
            {
                for (var j = 0; j < gradSet.Length; j++)
                    gradSet[j] = 0;
            }

            for (var s = 0; s < numEpochs; s++)
            {
                var t = rand.Next(trainingSet.Count);
                var inputOutput = trainingSet[t];

                var inputLength = inputOutput.Input.Length;
                var targetLength = inputOutput.Output.Length;
                
                if (nn.NumInputs != inputLength)
                    throw new NeuralNetworkException("Training input had " + inputLength + " while neural network expected " + nn.NumInputs);

                if (nn.NumOutputs != targetLength)
                    throw new NeuralNetworkException("Training input had " + targetLength + " while neural network expected " + nn.NumOutputs);

                nn.SetInputs(inputOutput.Input);
                nn.FeedForward();
                var gradients = nn.CalculateGradients(inputOutput.Output);
                AdjustWeights(nn, gradients, prevWeightGradients);
                gradients.DeepCopyTo(prevWeightGradients);
            }

            return nn;
        }

        public static double GetError(INeuralNetwork nn, IList<InputOutput> testSet)
        {
            var error = 0.0;

            foreach (var inputOutput in testSet)
            {
                nn.SetInputs(inputOutput.Input);
                nn.FeedForward();

                error += ErrorCalculations.CrossEntropyError(nn.Outputs, inputOutput.Output);
            }

            return error / testSet.Count;
        }

        public static double GetAccuracy(INeuralNetwork nn, IList<InputOutput> testSet)
        {
            var numHits = 0;

            foreach (var inputOutput in testSet)
            {
                nn.SetInputs(inputOutput.Input);
                nn.FeedForward();
                var expected = inputOutput.Output.MaxIndex();
                var actual = nn.Outputs.MaxIndex();

                numHits += expected == actual ? 1 : 0;
            }

            return ((double) numHits) / testSet.Count;
        }

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
                    var fullGradient = gradientSubList[j] + Config.QuadraticRegularization * prevWeight +
                                        Config.Momentum * prevWeightGradients[i][j];
                    weightSubList[j] = prevWeight - Config.LearningRate * fullGradient;
                }
            }
        }
    }
}
