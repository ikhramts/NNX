using System;
using FluentAssertions;
using NeuralNetworks;
using NeuralNetworks.Training;
using Xunit;

namespace NNX.Tests.ExcelFuncionsTests
{
    public class MakeSimpleGradientTrainerTests : ObjectStoreTests
    {
        [Fact]
        public void ShouldMakeTrainerConfig()
        {
            ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, 0, 0, 2, 0);
            var result = ObjectStore.Get<SimpleGradientTrainer>("t");
            result.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnObjectName()
        {
            var expected = "t";
            var result = ExcelFunctions.MakeSimpleGradientTrainer(expected, 10, 0.1, 0, 0, 2, 0);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ShouldStoreLearningRate()
        {
            ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, 0, 0, 2, 0);
            var result = ObjectStore.Get<SimpleGradientTrainer>("t");
            Assert.Equal(0.1, result.LearningRate);
        }

        [Fact]
        public void ShouldStoreMomentum()
        {
            ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, 0.2, 0, 2, 0);
            var result = ObjectStore.Get<SimpleGradientTrainer>("t");
            Assert.Equal(0.2, result.Momentum);
        }

        [Fact]
        public void ShouldStoreQuadraticRegularization()
        {
            ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, 0, 0.3, 2, 0);
            var result = ObjectStore.Get<SimpleGradientTrainer>("t");
            Assert.Equal(0.3, result.QuadraticRegularization);
        }

        [Fact]
        public void ShouldStoreNumEpochs()
        {
            ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, 0, 0.3, 2, 20);
            var result = ObjectStore.Get<SimpleGradientTrainer>("t");
            Assert.Equal(10, result.NumEpochs);
        }

        [Fact]
        public void ShouldStoreSeed()
        {
            ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, 0, 0.3, 2, 20);
            var result = ObjectStore.Get<SimpleGradientTrainer>("t");
            Assert.Equal(20, result.Seed);
        }

        [Fact]
        public void ShouldStoreBatchSize()
        {
            ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, 0, 0.3, 2, 20);
            var result = ObjectStore.Get<SimpleGradientTrainer>("t");
            Assert.Equal(2, result.BatchSize);
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(0)]
        public void IfNumEpochsNotPositive_Throw(int badNumEpochs)
        {
            Action action = () => ExcelFunctions.MakeSimpleGradientTrainer("t", badNumEpochs, 0.1, 0, 0, 2, 0);
            action.ShouldThrow<NeuralNetworkException>();
        }

        [Theory]
        [InlineData(-2.0)]
        [InlineData(0.0)]
        public void IfLearningRateNotPositive_Throw(double badLearningRate)
        {
            Action action = () => ExcelFunctions.MakeSimpleGradientTrainer("t", 10, badLearningRate, 0, 0, 2, 0);
            action.ShouldThrow<NeuralNetworkException>();
        }

        [Fact]
        public void IfMomentumNotPositive_Throw()
        {
            Action action = () => ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, -2, 0, 2, 0);
            action.ShouldThrow<NeuralNetworkException>();
        }

        [Fact]
        public void IfQuadraticRegularizationNotPositive_Throw()
        {
            Action action = () => ExcelFunctions.MakeSimpleGradientTrainer("t", 10, 0.1, 0, -2, 2, 0);
            action.ShouldThrow<NeuralNetworkException>();        }
    }
}
