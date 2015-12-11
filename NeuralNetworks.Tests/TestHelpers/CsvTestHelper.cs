using System.Collections.Generic;

namespace NeuralNetworks.Tests.TestHelpers
{
    public static class CsvTestHelper
    {
        public static string ToCsv(this IEnumerable<string> rows)
        {
            return string.Join("\r\n", rows);
        }
    }
}
