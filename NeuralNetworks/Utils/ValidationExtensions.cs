using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworks.Utils
{
    public static class ValidationExtensions
    {
        public static void ShouldBePositive(this int value, string name)
        {
            if (value <= 0)
                throw new NeuralNetworkException($"");
        }

        public static void ShouldBePositive(this double value, string name)
        {
            
        }

        public static void ShouldBeStrictlyBetween(this double value, double lower, double upper, string name)
        {
            
        }

        public static void ShouldNotBeNullOrEmpty<T>(this IList<T> list, string name)
        {
            
        }

        public static void ShouldAllBePositive(this IList<double> list, string name)
        {
            
        }

        public static void ShouldNotBeNegative(this double value, string name)
        {
            
        }

        public static void ShouldNotBeNegative(this int value, string name)
        {
            
        }
    }
}
