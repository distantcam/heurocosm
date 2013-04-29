using System;

namespace Heurocosm
{
    public interface ISpawner<T>
    {
        T CreateItem(Random rnd);
    }

    public interface IFitnessCalculator<T>
    {
        double CalculateFitness(T item);
    }

    public interface ICrossoverOperator<T>
    {
        T Crossover(Random rnd, T item1, T item2);
    }

    public interface IMutatorOperator<T>
    {
        T Mutate(Random rnd, T item);
    }

    public interface ITerminator<T>
    {
        bool IsFinished(T item, double fitness);
    }

    public class DefaultTerminator<T> : ITerminator<T>
    {
        public bool IsFinished(T item, double fitness)
        {
            return fitness == 0;
        }
    }
}