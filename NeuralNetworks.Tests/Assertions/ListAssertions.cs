using System;
using System.Collections.Generic;

namespace NeuralNetworks.Tests.Assertions
{
    public static class ListAssertions
    {
        public static void ShouldApproximatelyEqual(this IList<double> actual, 
            IList<double> expected, double relTolerance = 1e-12)
        {
            if (actual == null)
            {
                if (expected != null)
                    throw new Exception("Expected null but got not null");

                return;
            }

            if (expected == null)
                throw new Exception("Expected not null but got null.");

            if (actual.Count != expected.Count)
                throw new Exception($"Expected length {expected.Count} but actual length is {actual.Count}.");

            for (var i = 0; i < expected.Count; i++)
            {
                var error = expected[i] == 0.0 ? actual[i] : (actual[i] - expected[i]) / expected[i];

                if (Math.Abs(error) > relTolerance)
                    throw new Exception($"At index {i}: Expected error to be between than +/-{relTolerance}; was {error}");
            }
                
        }
    }
}
