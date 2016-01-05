using System;
using System.Collections.Generic;
using NeuralNetworks.Utils;

namespace NeuralNetworks.Training
{
    public class ParticleSwarmTrainer : BaseTrainer
    {
        public int NumParticles { get; set; }
        public int NumEpochs { get; set; }
        public int NumThreads { get; set; }
        public int BatchSize { get; set; }

        public ParticleSwarmTrainer()
        {
            NumThreads = Environment.ProcessorCount - 1;
            ShouldInitializeWeights = false;
            BatchSize = 0;
        }

        public override void Train(IList<InputOutput> trainingSet, IList<InputOutput> validationSet, 
            IRandomGenerator rand, INeuralNetwork nn)
        {
            throw new NotImplementedException();
        }

        public override void Validate()
        {
            if (NumParticles <= 0)
                throw new NeuralNetworkException($"Parameter {nameof(NumParticles)} must be positive; was {NumParticles}.");

            if (NumEpochs <= 0)
                throw new NeuralNetworkException($"Parameter {nameof(NumEpochs)} must be positive; was {NumEpochs}.");

            if (NumThreads <= 0)
                throw new NeuralNetworkException($"Parameter {nameof(NumThreads)} must be positive; was {NumThreads}.");

            if (BatchSize <= 0)
                throw new NeuralNetworkException($"Parameter {nameof(BatchSize)} must be positive; was {BatchSize}.");
        }

        protected override double GetValidationSetFraction()
        {
            return 0;
        }
    }
}
