using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Heurocosm.Rnd;
using NullGuard;

namespace Heurocosm
{
    public class Progress
    {
        public int Generation { get; set; }
    }

    public class Engine<T>
    {
        private struct Neighbour
        {
            public T Item;
            public double Fitness;

            public Neighbour(T item, double fitness)
            {
                Item = item;
                Fitness = fitness;
            }
        }

        private double mutationProbability;
        private double crossoverProbability;
        private int populationSize;

        private readonly Subject<Progress> progress;

        public Engine(ISpawner<T> spawner, IFitnessCalculator<T> fitnessCalculator, ICrossoverOperator<T> crossover, IMutatorOperator<T> mutator)
        {
            Spawner = spawner;
            FitnessCalculator = fitnessCalculator;
            Crossover = crossover;
            Mutator = mutator;

            PopulationSize = 300;
            CrossoverProbability = 0.87;
            MutationProbability = 0.01;

            progress = new Subject<Progress>();
        }

        public int PopulationSize
        {
            get { return populationSize; }
            set
            {
                if (value <= 0)
                    throw new System.ArgumentOutOfRangeException("Population must be greater than zero.");
                populationSize = value;
            }
        }

        public double CrossoverProbability
        {
            get { return crossoverProbability; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("CrossoverProbability must be within 0.0 - 1.0");
                crossoverProbability = value;
            }
        }

        public double MutationProbability
        {
            get { return mutationProbability; }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException("MutationProbability must be within 0.0 - 1.0");
                mutationProbability = value;
            }
        }

        public ISpawner<T> Spawner { get; set; }
        public IFitnessCalculator<T> FitnessCalculator { get; set; }
        public ICrossoverOperator<T> Crossover { get; set; }
        public IMutatorOperator<T> Mutator { get; set; }
        [AllowNull]
        public ITerminator<T> Terminator { get; set; }

        public IObservable<Progress> Progress { get { return progress; } }

        public Task<T> Run(CancellationToken token, TaskScheduler scheduler = null)
        {
            if (scheduler == null)
                scheduler = TaskScheduler.Current;

            if (Terminator == null)
                Terminator = new DefaultTerminator<T>();

            return Task.Factory.StartNew(() =>
            {
                var popts = new ParallelOptions() { CancellationToken = token, TaskScheduler = scheduler };

                var rnd = new ThreadSafeRandom();

                int generation = 0;

                var population = CreatePopulation(popts, rnd);
                var currentBest = GetBestNeighbour(population);

                progress.OnNext(new Progress() { Generation = generation++ });

                while (!Terminator.IsFinished(currentBest.Item, currentBest.Fitness))
                {
                    population = CreateNextGeneration(population, popts, rnd);
                    currentBest = GetBestNeighbour(population);

                    progress.OnNext(new Progress() { Generation = generation++ });
                }

                return currentBest.Item;
            }, token, TaskCreationOptions.None, scheduler);
        }

        private Neighbour GetBestNeighbour(IEnumerable<Neighbour> population)
        {
            return population.Aggregate((l, r) => l.Fitness < r.Fitness ? l : r);
        }

        private Neighbour[] CreatePopulation(ParallelOptions popts, Random rnd)
        {
            var result = new Neighbour[PopulationSize];

            Parallel.For(0, PopulationSize, popts, i =>
            {
                var item = Spawner.CreateItem(rnd);
                var fitness = FitnessCalculator.CalculateFitness(item);
                result[i] = new Neighbour(item, fitness);
            });

            return result;
        }

        private Neighbour[] CreateNextGeneration(Neighbour[] population, ParallelOptions popts, Random rnd)
        {
            var maxFitness = population.Max(g => g.Fitness) + 1;
            var sumOfMaxMinusFitness = population.Sum(g => maxFitness - g.Fitness);

            var result = new Neighbour[PopulationSize * 2];

            Parallel.For(0, PopulationSize, popts, i =>
            {
                var children = CreateChildren(
                    FindRandomHighQualityParent(population, rnd, sumOfMaxMinusFitness, maxFitness),
                    FindRandomHighQualityParent(population, rnd, sumOfMaxMinusFitness, maxFitness),
                    rnd);
                result[i * 2] = children[0];
                result[i * 2 + 1] = children[1];
            });

            return result;
        }

        private Neighbour[] CreateChildren(Neighbour parent1, Neighbour parent2, Random rnd)
        {
            T child1, child2;

            if (rnd.NextDouble() < CrossoverProbability)
            {
                child1 = Crossover.Crossover(rnd, parent1.Item, parent2.Item);
                child2 = Crossover.Crossover(rnd, parent2.Item, parent1.Item);
            }
            else
            {
                child1 = parent1.Item;
                child2 = parent2.Item;
            }

            if (rnd.NextDouble() < MutationProbability) child1 = Mutator.Mutate(rnd, child1);
            if (rnd.NextDouble() < MutationProbability) child2 = Mutator.Mutate(rnd, child2);

            return new Neighbour[] {
                new Neighbour(child1, FitnessCalculator.CalculateFitness(child1)),
                new Neighbour(child2, FitnessCalculator.CalculateFitness(child2)),
            };
        }

        // TODO Make selection interface
        private Neighbour FindRandomHighQualityParent(Neighbour[] population, Random rnd, double sumOfMaxMinusFitness, double max)
        {
            var val = rnd.NextDouble() * sumOfMaxMinusFitness;
            for (int i = 0; i < population.Length; i++)
            {
                var maxMinusFitness = max - population[i].Fitness;
                if (val < maxMinusFitness) return population[i];
                val -= maxMinusFitness;
            }
            throw new InvalidOperationException("Not to be, apparently.");
        }
    }
}