using System;
using System.Collections.Generic;

namespace NeuralNetworks
{
    public static class ListExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list, Random customRand = null)
        {
            var result = new List<T>(list);

            var rand = customRand ?? new Random();

            // Use Fisher-Yates shuffle
            // http://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle

            var lastItem = result.Count;
            for (var i = 0; i < result.Count - 1; i++)
            {
                var k = rand.Next(i, lastItem);
                var temp = result[k];
                result[k] = result[i];
                result[i] = temp;
            }

            return result;
        }

        public static int MaxIndex(this IList<double> list)
        {
            var maxIndex = 0;

            for (var i = 1; i < list.Count; i++)
            {
                if (list[i] > list[maxIndex])
                    maxIndex = i;
            }

            return maxIndex;
        }

        public static double[][] DeepClone(this double[][] array)
        {
            var clone = (double[][])array.Clone();

            for (var i = 0; i < clone.Length; i++)
                clone[i] = (double[]) clone[i].Clone();

            return clone;
        }

        public static void DeepCopyTo(this double[][] array, double[][] target)
        {
            for (var i = 0; i < array.Length; i++)
                Array.Copy(array[i], target[i], array[i].Length);
        }
    }
}
