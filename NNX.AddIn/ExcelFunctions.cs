using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExcelDna.Integration;
using NNX.Core;
using NNX.Core.Training;
using NNX.Core.Utils;

namespace NNX.AddIn
{
    public static class ExcelFunctions
    {
        public static readonly Dictionary<string, Type> DefaultSupportedObjects = new Dictionary<string, Type>
        {
            { "MultilayerPerceptron", typeof(MultilayerPerceptron)}
        };

        public static Dictionary<string, Type> SupportedObjects = DefaultSupportedObjects;

        //===================== Excel functions =============================

        [ExcelFunction(Name= "nnMakeArray")]
        public static string MakeArray(
            [ExcelArgument(Description = "Name of the array object to create.")] string name,
            [ExcelArgument(Description = "Array of values.")] object[] values)
        {
            if (values == null || values.Length == 0)
                throw new NNXException("Array cannot be empty.");

            object result;
            const string inconsistentArrayMessage = "Array must contain either only numbers or only strings. " +
                                                    "Encountered both in this array.";
            const string badTypeMessage = "Array elements must be either numbers or strings. Element" +
                                          " at position {0} was neither number nor string.";

            // Determine the type of the array. To do that,
            // find the first element that is neither null nor empty.
            var firstNonEmptyIndex = 0;

            while (firstNonEmptyIndex < values.Length && IsEmpty(values[firstNonEmptyIndex]))
                firstNonEmptyIndex++;

            if (firstNonEmptyIndex >= values.Length)
                throw new NNXException("Array cannot be empty.");

            var firstNonEmpty = values[firstNonEmptyIndex];

            if (firstNonEmpty is double)
            {
                var goodValues = new List<double>(values.Length - firstNonEmptyIndex);

                for (var i = 0; i < values.Length; i++)
                {
                    var value = values[i];

                    if (IsEmpty(value))
                        continue;

                    if (value is double)
                    {
                        goodValues.Add((double) value);
                        continue;
                    }

                    if (value is string)
                        throw new NNXException(inconsistentArrayMessage);
                    
                    throw new NNXException(string.Format(badTypeMessage, i));
                }

                result = goodValues.ToArray();
            }
            else if (firstNonEmpty is string)
            {
                var goodValues = new List<string>(values.Length - firstNonEmptyIndex);

                for (var i = 0; i < values.Length; i++)
                {
                    var value = values[i];

                    if (IsEmpty(value))
                        continue;

                    if (value is string)
                    {
                        goodValues.Add((string)value);
                        continue;
                    }

                    if (value is double)
                        throw new NNXException(inconsistentArrayMessage);

                    throw new NNXException(string.Format(badTypeMessage, i));
                }

                result = goodValues.ToArray();
            }
            else
            {
                throw new NNXException(string.Format(badTypeMessage, 0));
            }

            ObjectStore.Add(name, result);
            return name;
        }

        [ExcelFunction(Name = "nnMakeWeights")]
        public static string MakeWeights(
            [ExcelArgument(Description = "Name of the weights object to create.")] string name,
            [ExcelArgument(Description = "Array of array objects created using =nnMakeArray().")] object[] weightArrays)
        {
            if (weightArrays == null || weightArrays.Length == 0)
                throw new NNXException("Argument WeightArrays cannot be null or empty.");

            var weightArrayNames = weightArrays.Select(o => o.ToString()).ToArray();

            var results = new double[weightArrayNames.Length][];

            for (var i = 0; i < weightArrayNames.Length; i++)
            {
                var arrayName = weightArrayNames[i];

                double[] array;

                if (!ObjectStore.TryGet(arrayName, out array))
                    throw new NNXException($"Element at index {i + 1} ('{arrayName}') does not " +
                                           "point to a valid array of numbers.");

                results[i] = array;
            }

            ObjectStore.Add(name, results);

            return name;
        }

        //[ExcelFunction(Name = "nnMakeObject")]
        public static string MakeObject(string name, string typeName, object[,] properties)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new NNXException("Argument TypeName should not be null or empty.");

            Type objectType;

            if (!SupportedObjects.TryGetValue(typeName, out objectType))
                throw new NNXException($"Unrecognized object type: '{typeName}'.");

            var obj = Activator.CreateInstance(objectType);

            if (properties == null || properties.GetLength(0) == 0)
            {
                // No properties to set, we're done here.
                ObjectStore.Add(name, obj);
                return name;
            }

            if (properties.GetLength(1) != 2)
                throw new NNXException($"Argument Properties must have width 2; was: {properties.GetLength(1)}.");

            var numProperties = properties.GetLength(0);
            var objectProperties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                             .Where(p => p.CanWrite).ToList();

            for (var i = 0; i < numProperties; i++)
            {
                var propertyName = properties[i, 0].ToString();
                var value = properties[i, 1];

                if (string.IsNullOrEmpty(propertyName))
                    throw new NNXException($"Property at position {i + 1} was empty.");

                var targetProperty = objectProperties.FirstOrDefault(p => p.Name == propertyName);

                if (targetProperty == null)
                    throw new NNXException($"Object type {typeName} does not have property {propertyName}.");

                var propertyType = targetProperty.PropertyType;

                // Special case: assigning an integer double to int.
                if (propertyType == typeof (int) 
                    && value is double 
                    && (((double)value) % 1 == 0.0))
                {
                    targetProperty.SetValue(obj, (int)((double)value));
                    continue;
                }

                // Special case: assigning int to double.
                if (propertyType == typeof (double) && value is int)
                {
                    targetProperty.SetValue(obj, (double)((int)value));
                    continue;
                }

                // Special case: string.
                if (propertyType == typeof (string))
                {
                    var stringValue = value?.ToString() ?? "";
                    targetProperty.SetValue(obj, stringValue);
                    continue;
                }

                // General case.
                if (propertyType.IsInstanceOfType(value))
                {
                    targetProperty.SetValue(obj, value);
                    continue;
                }

                // If this was a simple type, then the user provided a value
                // of the wrong type.
                if (propertyType.IsAssignableFrom(typeof (int))
                    || propertyType.IsAssignableFrom(typeof (double))
                    || propertyType.IsAssignableFrom(typeof (string)))
                {
                    var propertyTypeName = MapPropertyType(propertyType);
                    var aOrAn = AOrAn(propertyTypeName);

                    throw new NNXException($"Property {propertyName} of object type {typeName} " +
                                            $"must be {aOrAn} {propertyTypeName}; was {value}.");
                }

                // If we get here, then the target property is an array or array of arrays.
                var referencedName = properties[i, 1].ToString();

                // Special case: weights.
                if (propertyType.IsAssignableFrom(typeof (double[][])))
                {
                    double[][] weights;

                    if (!ObjectStore.TryGet(referencedName, out weights))
                        throw new NNXException($"Property {propertyName} of type {typeName} must be of type " +
                                               "Weights created using nnMakeWeights() function.");

                    targetProperty.SetValue(obj, weights);
                    continue;
                }

                // Special case: enumerables.
                if (propertyType.IsAssignableFrom(typeof (string[])))
                {
                    string[] array;

                    if (!ObjectStore.TryGet(referencedName, out array))
                        throw new NNXException($"Property {propertyName} of type {typeName} must refer " +
                                               $"to array containing elements of type string.");

                    targetProperty.SetValue(obj, array);
                    continue;
                }

                if (propertyType.IsAssignableFrom(typeof(double[])))
                {
                    double[] array;

                    if (!ObjectStore.TryGet(referencedName, out array))
                        throw new NNXException($"Property {propertyName} of type {typeName} must refer " +
                                               $"to array containing elements of type number.");

                    targetProperty.SetValue(obj, array);
                    continue;
                }

                if (propertyType.IsAssignableFrom(typeof(int[])))
                {
                    int[] intArray;

                    if (ObjectStore.TryGet(referencedName, out intArray))
                    {
                        targetProperty.SetValue(obj, intArray);
                        continue;
                    }

                    double[] doubleArray;

                    if (ObjectStore.TryGet(referencedName, out doubleArray))
                    {
                        var convertedArray = new int[doubleArray.Length];

                        for (var j = 0; j < doubleArray.Length; j++)
                        {
                            var doubleValue = doubleArray[j];

                            if (doubleValue % 1 != 0)
                                throw new NNXException($"Property {propertyName} of type {typeName} must refer " +
                                                         $"to array containing elements of type integer.");

                            convertedArray[j] = (int)doubleValue;
                        }

                        targetProperty.SetValue(obj, convertedArray);
                        continue;
                    }


                    throw new NNXException($"Property {propertyName} of type {typeName} must refer " +
                                               $"to array containing elements of type integer.");
                }

                throw new Exception("Reached unreacheable code.");
            }

            ObjectStore.Add(name, obj);
            return name;
        }

        [ExcelFunction(Name = "nnMakeSimpleGradientTrainer")]
        public static string MakeSimpleGradientTrainer(
            [ExcelArgument(Description = "Name of trainer object to create.")] string name,
            [ExcelArgument(Description = "Number of backpropagation steps to run. Each step may be on-line or a batch step.")] int numEpochs,
            [ExcelArgument(Description = "Impact of each backpropagation step on weight adjustment.")] double learningRate,
            [ExcelArgument(Description = "Impact of previous backpropagation stepts each step's adjustment.")] double momentum,
            [ExcelArgument(Description = "Higher numbers help with keeping weights from becoming too large.")] double quadraticRegularization,
            [ExcelArgument(Description = "Number of training samples to evaluate for each backpropagation step.")] int batchSize,
            [ExcelArgument(Description = "Seed for random number generation.")] int seed)
        {
            var config = new SimpleGradientTrainer
            {
                NumEpochs = numEpochs,
                LearningRate = learningRate,
                Momentum = momentum,
                QuadraticRegularization = quadraticRegularization,
                BatchSize = batchSize,
                Seed = seed,
            };

            config.Validate();

            ObjectStore.Add(name, config);
            return name;
        }

        [ExcelFunction(Name = "nnMakeUntilDoneGradientTrainer")]
        public static string MakeUntilDoneGradientTrainer(
            [ExcelArgument(Description = "Name of trainer object to create.")] string name,
            [ExcelArgument(Description = "Maximum number of backpropagation steps to run. " +
                                         "Each step may be on-line or a batch step.")] int numEpochs,
            [ExcelArgument(Description = "Impact of each backpropagation step on weight adjustment.")] double learningRate,
            [ExcelArgument(Description = "Impact of previous backpropagation stepts each step's adjustment.")] double momentum,
            [ExcelArgument(Description = "Higher numbers help with keeping weights from becoming too large.")] double quadraticRegularization,
            [ExcelArgument(Description = "Number of training samples to evaluate for each backpropagation step.")] int batchSize,
            [ExcelArgument(Description = "Portion of the training set to use as validation set to check whether " +
                                         "training has improved performance of the neural network. " +
                                         "Must be between 0 and 1.")] double validationSetFraction,
            [ExcelArgument(Description = "Training will abort after there is no error improvement on validation set after" +
                                         "this number of backpropagation steps.")] int maxEpochsWithoutImprovement,
            [ExcelArgument(Description = "Number of backpropagation steps before checking for error improvement on" +
                                         "validation set.")] int epochsBetweenValidations,
            [ExcelArgument(Description = "Seed for random number generation.")] int seed)
        {
            var config = new UntilDoneGradientTrainer
            {
                NumEpochs = numEpochs,
                LearningRate = learningRate,
                Momentum = momentum,
                QuadraticRegularization = quadraticRegularization,
                BatchSize = batchSize,
                ValidationSetFraction = validationSetFraction,
                MaxEpochsWithoutImprovement = maxEpochsWithoutImprovement,
                EpochsBetweenValidations = epochsBetweenValidations,
                Seed = seed,
            };

            config.Validate();

            ObjectStore.Add(name, config);
            return name;
        }

        [ExcelFunction(Name = "nnMakeMultilayerPerceptron")]
        public static string MakeMultilayerPerceptron(
            [ExcelArgument(Description = "Name of perceptron object to create.")] string name,
            [ExcelArgument(Description = "Number of input nodes, not including input bias.")] int numInputs,
            [ExcelArgument(Description = "Number of output nodes.")] int numOutputs,
            [ExcelArgument(Description = "Number of nodes in each hidden layer, not including biases. " +
                                         "Must be an array of integers.")] double[] hiddenLayerSizes,
            [ExcelArgument(Description = "Weights object created using =nnMakeWeights().")] string weights)
        {
            var nn = new MultilayerPerceptron(numInputs, numOutputs, hiddenLayerSizes.ToIntArray());

            double[][] weightValues;
            
            if (!ObjectStore.TryGet(weights, out weightValues))
                throw new NNXException("Argument Weights should be a weights object created using nnMakeWeights().");

            if (weightValues.Length != nn.Weights.Length)
                throw new NNXException($"Argument Weights was expected to have {nn.Weights.Length} " +
                                       $"layers; had: {weightValues.Length}.");

            for (var layer = 0; layer < weightValues.Length; layer++)
            {
                if (weightValues[layer].Length != nn.Weights[layer].Length)
                    throw new NNXException($"Argument Weights was expected to have {nn.Weights[layer].Length} " +
                                           $"values in layer {layer + 1}; had: {weightValues[layer].Length}.");
            }

            weightValues.DeepCopyTo(nn.Weights);

            ObjectStore.Add(name, nn);
            return name;
        }

        [ExcelFunction(Name = "nnTrainMultilayerPerceptron")]
        public static string TrainMultilayerPerceptron(
            [ExcelArgument(Description = "Name of perceptron object to create.")] string name,
            [ExcelArgument(Description = "Name of the trainer that will train this neural network.")] string trainerName,
            [ExcelArgument(Description = "Matrix of training inputs.")] object[,] inputs,
            [ExcelArgument(Description = "Matrix of training targets.")] object[,] targets,
            [ExcelArgument(Description = "Number of nodes in each hidden layer, not including biases. " +
                                         "Must be an array of integers.")] double[] hiddenLayerSizes)
        {
            var inputTargets = PrepareInputTargetSet(inputs, targets);

            var inputWidth = inputs.GetLength(1);
            var targedWidth = targets.GetLength(1);
            var trainer = ObjectStore.Get<ITrainer>(trainerName);

            var intHiddenLayerSizes = hiddenLayerSizes.Select(h => (int) h).ToArray();
            var nn = new MultilayerPerceptron(inputWidth, targedWidth, intHiddenLayerSizes);

            trainer.Train(inputTargets, nn);

            ObjectStore.Add(name, nn);
            return name;
        }

        //[ExcelFunction(Name = "nnGetTrainingStats")]
        public static object[,] GetTrainingStats(string name)
        {
            throw new NotImplementedException();
        }

        [ExcelFunction(Name = "nnGetWeights")]
        public static double[,] GetWeights(
            [ExcelArgument(Description = "Name of neural network for which to get weights.")] string name,
            [ExcelArgument(Description = "Neural network layer number, starting with 1.")] int layer)
        {
            var perceptron = ObjectStore.Get<INeuralNetwork>(name);

            if (layer > perceptron.Weights.Length || layer <= 0)
                throw new NNXException($"Layer for neural network {name} must be between " +
                                       $"1 and {perceptron.Weights.Length}; was {layer}.");

            var result = perceptron.Weights[layer - 1].ToVertical2DArray();

            ResizeOutputToArray(result);
            return result;
        }

        [ExcelFunction(Name = "nnClearAllObjects", Description = "Clear all objects from NNX object store.")]
        public static string ClearAllObjects()
        {
            ObjectStore.Clear();
            return "OK";
        }

        [ExcelFunction(Name = "nnFeedForward")]
        public static double[,] FeedForward(
            [ExcelArgument(Description = "Name of neural network.")] string neuralNetworkName,
            [ExcelArgument(Description = "Array of inputs.")] double[] inputs)
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

        private static string MapPropertyType(Type propertyType)
        {
            if (propertyType == typeof (string))
                return "string";

            if (propertyType == typeof (int))
                return "integer";

            if (propertyType == typeof (double))
                return "number";

            if (propertyType.IsAssignableFrom(typeof(int[])))
                return "integer array";

            if (propertyType.IsAssignableFrom(typeof(double[])))
                return "number array";

            if (propertyType.IsAssignableFrom(typeof(string[])))
                return "string array";

            if (propertyType.IsAssignableFrom(typeof(double[][])))
                return "weights";

            throw new Exception($"Unsupported type: {propertyType}");
        }

        private static string AOrAn(string word)
        {
            var firstLetter = word.ToLower()[0];

            if (firstLetter == 'a'
                || firstLetter == 'e'
                || firstLetter == 'i'
                || firstLetter == 'o'
                || firstLetter == 'u'
                || firstLetter == 'y')
                return "an";

            return "a";
        }

        private static bool IsEmpty(object cellValue)
        {
            return cellValue == null
                   || (cellValue is string && ((string) cellValue) == "")
                   || cellValue == ExcelEmpty.Value;
        }
    }
}
