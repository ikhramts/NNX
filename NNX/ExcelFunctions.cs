using System;
using ExcelDna.Integration;
using NeuralNetworks;
using NNX.NeuralNetwork;

namespace NNX
{
    public static class ExcelFunctions
    {
        //===================== Excel functions =============================
        [ExcelFunction(Name = "nnMakeTrainer")]
        public static string MakeTrainer(string name, int numEpochs, double learningRate, double quadraticRegularization,
            double momentum)
        {
            throw new NotImplementedException();
        }

        [ExcelFunction(Name = "nnMakeTwoLayerPerceptronConfig")]
        public static string MakeTwoLayerPerceptronConfig(string configName, int numHiddenNodes)
        {
            if (numHiddenNodes <= 0)
                throw new NNXException("Numebr of hidden nodes must be greater than zero; " + 
                                        $"provided number was {numHiddenNodes}.");

            var config = new TwoLayerPerceptronConfig
            {
                NumHiddenNodes = numHiddenNodes
            };

            ObjectStore.Add(configName, config);

            return configName;
        }

        [ExcelFunction(Name = "nnMakeTwoLayerPerceptron")]
        public static string MakeTwoLayerPerceptron(string name, int numHiddenNodes, double[] hiddenWeights, double[] outputWeights)
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

        [ExcelFunction(Name = "nnTrain")]
        public static string Train(string neuralNetworkName, string trainerName, double[,] inputs, double[] targets)
        {
            throw new NotImplementedException();
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
                result = perceptron.HiddenWeights.To2DArray();
            else
                result = perceptron.OutputWeights.To2DArray();

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
        public static double[] FeedForward(string neuralNetworkName, double[] inputs)
        {
            throw new NotImplementedException();
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

    }
}
