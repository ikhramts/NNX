using System;
using System.Collections.Generic;
using System.Linq;
using ExcelDna.Integration;
using NeuralNetworks;
using NeuralNetworks.Training;

namespace NNX
{
    public static class ExcelFunctions
    {
        //===================== Excel functions =============================
        [ExcelFunction(Name = "nnMakeTrainerConfig")]
        public static string MakeTrainerConfig(string name, int numEpochs, double learningRate,
            double momentum, double quadraticRegularization, int seed)
        {
            var config = new TrainerConfig
            {
                NumEpochs = numEpochs,
                LearningRate = learningRate,
                Momentum = momentum,
                QuadraticRegularization = quadraticRegularization,
                Seed = seed,
            };

            config.Validate();

            ObjectStore.Add(name, config);
            return name;
        }

        [ExcelFunction(Name = "nnMakeTwoLayerPerceptron")]
        public static string MakeTwoLayerPerceptron(string name, int numHiddenNodes, 
            double[] hiddenWeights, double[] outputWeights)
        {
            var numInputs = (hiddenWeights.Length / numHiddenNodes) - 1;
            var numOutputs = outputWeights.Length / (numHiddenNodes + 1);

            if ((numInputs + 1) * numHiddenNodes != hiddenWeights.Length)
                throw new NNXException($"Number of hidden nodes ({numHiddenNodes}) is not consistent " +
                                        $"with number of input-to-hidden weights ({hiddenWeights.Length}).");

            if (numOutputs * (numHiddenNodes + 1) != outputWeights.Length)
                throw new NNXException($"Number of hidden nodes ({numHiddenNodes}) is not consistent " +
                                        $"with number of hidden-to-output weights ({outputWeights.Length}).");

            var nn = new TwoLayerPerceptron(numInputs, numHiddenNodes, numOutputs);
            Array.Copy(hiddenWeights, nn.HiddenWeights, hiddenWeights.Length);
            Array.Copy(outputWeights, nn.OutputWeights, outputWeights.Length);

            ObjectStore.Add(name, nn);

            return name;
        }

        [ExcelFunction(Name = "nnTrainTwoLayerPerceptron")]
        public static string TrainTwoLayerPerceptron(string neuralNetworkName, string trainerConfigName, 
            object[,] inputs, object[,] targets, int numHiddenNodes)
        {
            // Check inputs.
            if (numHiddenNodes <= 0)
                throw new NNXException($"Parameter NumHiddenNodes should be positive; was {numHiddenNodes}.");

            var inputTargets = PrepareInputTargetSet(inputs, targets);

            var inputWidth = inputs.GetLength(1);
            var targedWidth = targets.GetLength(1);

            var trainerConfig = ObjectStore.Get<TrainerConfig>(trainerConfigName).Clone();
            var trainer = TrainerProvider.GetTrainer();
            trainer.Config = trainerConfig;
            var nn = new TwoLayerPerceptron(inputWidth, numHiddenNodes, targedWidth);
            trainer.Train(inputTargets, nn);
            
            ObjectStore.Add(neuralNetworkName, nn);
            return neuralNetworkName;
        }

        [ExcelFunction(Name = "nnTrainMultilayerPerceptron")]
        public static string TrainMultilayerPerceptron(string neuralNetworkName, string trainerConfigName,
            object[,] inputs, object[,] targets, double[] hiddenLayerSizes)
        {
            var inputTargets = PrepareInputTargetSet(inputs, targets);

            var inputWidth = inputs.GetLength(1);
            var targedWidth = targets.GetLength(1);
            var trainerConfig = ObjectStore.Get<TrainerConfig>(trainerConfigName).Clone();
            var trainer = TrainerProvider.GetTrainer();
            trainer.Config = trainerConfig;

            var intHiddenLayerSizes = hiddenLayerSizes.Select(h => (int) h).ToArray();
            var nn = new MultilayerPerceptron(inputWidth, targedWidth, intHiddenLayerSizes);

            trainer.Train(inputTargets, nn);

            ObjectStore.Add(neuralNetworkName, nn);
            return neuralNetworkName;
        }

        [ExcelFunction(Name = "nnGetTrainingStats")]
        public static object[,] GetTrainingStats(string name)
        {
            throw new NotImplementedException();
        }

        [ExcelFunction(Name = "nnGetWeights")]
        public static double[,] GetWeights(string neuralNetworkName, int layer)
        {
            var perceptron = ObjectStore.Get<TwoLayerPerceptron>(neuralNetworkName);

            if (layer != 1 && layer != 2)
                throw new NNXException($"Layer number must be 1 or 2; was {layer}.");

            double[,] result;

            if (layer == 1)
                result = perceptron.HiddenWeights.ToVertical2DArray();
            else
                result = perceptron.OutputWeights.ToVertical2DArray();

            ResizeOutputToArray(result);
            return result;
        }

        [ExcelFunction(Name = "nnClearAllObjects")]
        public static string ClearAllObjects()
        {
            ObjectStore.Clear();
            return "OK";
        }

        [ExcelFunction(Name = "nnFeedForward")]
        public static double[,] FeedForward(string neuralNetworkName, double[] inputs)
        {
            var nn = ObjectStore.Get<INeuralNetwork>(neuralNetworkName);
            var outputs = nn.FeedForward(inputs);
            var result = outputs.Output.ToHorizontal2DArray();

            ResizeOutputToArray(result);
            return result;
        }

        [ExcelFunction(Name = "nnGetCrossEntropyError")]
        public static double GetCrossEntropyError(double[] expected, double[] actual)
        {
            return ErrorCalculations.CrossEntropyError(expected, actual);
        }

        [ExcelFunction(Name = "nnGetMeanSquareError")]
        public static double GetMeanSquareError(double[] expected, double[] actual)
        {
            return ErrorCalculations.MeanSquareError(expected, actual);
        }

        //===================== Private helpers =============================
        private static void ResizeOutputToArray(double[,] arr)
        {
            try
            {
                XlCall.Excel(XlCall.xlUDF, "Resize", arr);
            }
            catch (Exception)
            {
                // Don't care if it succeeds;
            }
        }

        private static List<InputOutput> PrepareInputTargetSet(object[,] inputs, object[,] targets)
        {
            // Validate inputs.
            var numInputPoints = inputs.GetLength(0);
            var numTargetPoints = targets.GetLength(0);

            if (numInputPoints != numTargetPoints)
                throw new NNXException(
                    $"Height of Inputs matrix (was {numInputPoints}) should be equal to height " +
                    $"of Targets matrix (was {numTargetPoints}).");

            var numPoints = inputs.GetLength(0);

            var inputTargets = new List<InputOutput>(numPoints);

            for (var i = 0; i < numPoints; i++)
            {
                var rawInput = inputs.ExtractRow(i);

                if (!rawInput.All(r => r is double || r is int))
                    continue;

                var rawTarget = targets.ExtractRow(i);

                if (!rawTarget.All(t => t is double || t is int))
                    continue;


                var inputTarget = new InputOutput
                {
                    Input = rawInput.ToDoubles(),
                    Output = rawTarget.ToDoubles()
                };

                inputTargets.Add(inputTarget);
            }

            if (!inputTargets.Any())
                throw new NNXException("There were no good input/target point pairs.");

            return inputTargets;
        }
    }
}
