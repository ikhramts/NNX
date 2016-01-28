using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetworks.Utils
{
    public static class ListExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list, IRandomGenerator customRand = null)
        {
            var result = new List<T>(list);

            var rand = customRand ?? RandomProvider.GetRandom(0);

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

        public static Pair<IList<T>> Split<T>(this IList<T> list, double fractionInFirst)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (fractionInFirst >= 1 || fractionInFirst <= 0 || double.IsNaN(fractionInFirst))
                throw new ArgumentOutOfRangeException(nameof(fractionInFirst), fractionInFirst,
                    $"Argument {nameof(fractionInFirst)} must be between 0 and 1; was {fractionInFirst}.");

            if (list.Count == 0)
                return new Pair<IList<T>> (new T[] {}, new T[] {});

            var border = (int) (list.Count * fractionInFirst);
            var pair = new Pair<IList<T>>();
            pair.First = list.Take(border).ToArray();
            pair.Second = list.Skip(border).ToArray();

            return pair;
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

        public static bool ListEquals<T>(this IList<T> first, IList<T> second)
        {
            if (first == null && second == null) return true;
            if (first == null || second == null) return false;

            if (first.Count != second.Count) return false;

            for (var i = 0; i < first.Count; i++)
                if (!first[i].Equals(second[i]))
                    return false;

            return true;
        }

        public static int GetListHashCode<T>(this IList<T> list)
        {
            if (list == null) return 0;

            unchecked
            {
                var hash = 27;

                foreach (var item in list)
                    hash = (13 * hash) + item.GetHashCode();

                return hash;
            }
        }

        public static double[][] DeepCloneToZeros(this double[][] array)
        {
            var zeros = (double[][])array.Clone();

            for (var i = 0; i < zeros.Length; i++)
                zeros[i] = new double[array[i].Length];

            return zeros;
        }

        public static void AddInPlace(this double[][] target, double[][] other)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (target.Length == 0)
                throw new ArgumentException("Argument 'target' cannot be empty.", nameof(target));

            if (target.Length != other.Length)
                throw new ArgumentException("Argument 'other' should have same length as the target array " +
                                            $"({target.Length}); was {other.Length}.", nameof(other));

            for (var i = 0; i < target.Length; i++)
            {
                if (target[i].Length != other[i].Length)
                    throw new ArgumentException("Argument 'other' and target array have mismatching sub-array at " +
                                                $"index {i}; target subarray length: {target[i].Length}, other " +
                                                $"subarray length: {other[i].Length}.", nameof(other));

                for (var j = 0; j < target[i].Length; j++)
                    target[i][j] += other[i][j];
            }
        }

        public static void MultiplyInPlace(this double[][] target, double multiplier)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target.Length == 0)
                throw new ArgumentException("Argument 'target' cannot be empty.", nameof(target));

            if (double.IsNaN(multiplier))
                throw new ArgumentException("Argument 'multiplier' cannot be NaN.", nameof(multiplier));

            foreach (var subarray in target)
                for (var j = 0; j < subarray.Length; j++)
                    subarray[j] *= multiplier;
        }

        public static double[] AddRelativeNoise(this double[] array, double maxNoise, IRandomGenerator rand)
        {
            if (maxNoise == 0)
                return array;

            var result = new double[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = array[i] *(1 +  ((rand.NextDouble() * 2) - 1) * maxNoise);

            return result;
        }

        public static int[] ToIntArray(this double[] array)
        {
            var result = new int[array.Length];

            for (var i = 0; i < array.Length; i++)
                result[i] = (int) array[i];

            return result;
        }
    }
}
