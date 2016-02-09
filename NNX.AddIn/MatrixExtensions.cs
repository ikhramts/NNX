namespace NNX.AddIn
{
    public static class MatrixExtensions
    {
        public static T[] ExtractRow<T>(this T[,] matrix, int rowNum)
        {
            var width = matrix.GetLength(1);
            var result = new T[width];

            for (var i = 0; i < width; i++)
                result[i] = matrix[rowNum, i];

            return result;
        }
    }
}
