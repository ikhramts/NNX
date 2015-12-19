using System;
using System.Collections.Generic;

namespace NNX
{
    public static class ListExtensions
    {
        public static T[,] ToVertical2DArray<T>(this IList<T> list)
        {
            var result = new T[list.Count, 1];

            for (var i = 0; i < list.Count; i++)
                result[i, 0] = list[i];

            return result;
        }

        public static T[,] ToHorizontal2DArray<T>(this IList<T> list)
        {
            var result = new T[1, list.Count];

            for (var i = 0; i < list.Count; i++)
                result[0, i] = list[i];

            return result;
        }

        public static double[] ToDoubles<T>(this IList<T> list)
        {
            var result = new double[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                result[i] = Convert.ToDouble(list[i]);
            }

            return result;
        } 
    }
}
