using System;

namespace NeuralNetworks.Training
{
    public class TrainerProvider
    {
        public static Func<ITrainer> GetTrainer = GetDefaultTrainer; 
        public static ITrainer GetDefaultTrainer() => new Trainer();
    }
}
