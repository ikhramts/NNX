using System.Collections.Generic;

namespace NNX
{
    public static class ListExtensions
    {
        public static T[,] To2DArray<T>(this IList<T> list)
        {
            var result = new T[list.Count, 1];

            for (var i = 0; i < list.Count; i++)
                result[i, 0] = list[i];

            return result;
        }
    }
}
