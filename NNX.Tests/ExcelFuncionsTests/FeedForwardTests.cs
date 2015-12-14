using System;
using FluentAssertions;
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

            var output = new[] {0.5, 0.6};
            var expected = output.ToHorizontal2DArray();
            mock.SetupGet(n => n.Outputs).Returns(() => new[] {0.5, 0.6});
            var nn = mock.Object;

            ObjectStore.Add("nn", nn);

            var result = ExcelFunctions.FeedForward("nn", new[] {2.0, 2.0});
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IfObjectDoesNotExist_Throw()
        {
            Action action = () => ExcelFunctions.FeedForward("no such", new [] {1.0});
            action.ShouldThrow<NNXException>().WithMessage("*No such object: 'no such'*");
        }

        [Fact]
        public void IfObjectIsNotINeuralNetwork_Throw()
        {
            ObjectStore.Add("notNN", "notNN");
            Action action = () => ExcelFunctions.FeedForward("notNN", new[] { 1.0 });
            action.ShouldThrow<NNXException>().WithMessage("*was expected to be INeuralNetwork*");
        }
    }
}
