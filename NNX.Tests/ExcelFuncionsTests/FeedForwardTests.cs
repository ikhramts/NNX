using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NeuralNetworks;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class FeedForwardTests : ObjectStoreTestBase
    {
        [Fact]
        public void ShouldFeedForward()
        {
            var mock = new Mock<INeuralNetwork>();
            mock.Setup(n => n.SetInputs(It.IsAny<double[]>()));
            mock.Setup(n => n.FeedForward());

            var expected = new[] {0.5, 0.6};
            mock.SetupGet(n => n.Outputs).Returns(() => new[] {0.5, 0.6});
            var nn = mock.Object;

            ObjectStore.Add("nn", nn);

            var result = ExcelFunctions.FeedForward("nn", new[] {2.0, 2.0});
            Assert.Equal(expected, result);
        }
    }

        [Fact]
        public void IfObjectDoesNotExist_Throw()
        {
            
        }

        [Fact]
        public void IfObjectIsNotINeuralNetwork_Throw()
        {
            
        }

        [Fact]
        public void IfInputIsWrongDimension_Throw()
        {
            
        }


    }
}
