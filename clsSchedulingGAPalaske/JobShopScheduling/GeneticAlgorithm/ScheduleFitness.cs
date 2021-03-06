namespace JobShopScheduling.GeneticAlgorithm
{
    using System;
    using System.Linq;
    using Advanced.Algorithms.Graph;
    using GeneticSharp.Domain.Chromosomes;
    using GeneticSharp.Domain.Fitnesses;
    using Graph;

    /// <summary>
    /// Computes fitness and the schedule length.
    /// Schedule length is computed as the length of longest path
    /// from source to target in the schedule graph representation.
    /// Fitness is defined as 1 / (scheduleLength + 1).
    /// </summary>
    public class ScheduleFitness : IFitness
    {
        public double Evaluate(IChromosome chromosome)
        {
            return Evaluate((ScheduleChromosome) chromosome);
        }

        private double Evaluate(ScheduleChromosome chromosome)
        {
            if (chromosome.Fitness != null)
            {
                return chromosome.Fitness.Value;
            }

            // create graph
            chromosome.FixChromosome();
            var graph = chromosome.Graph;

            double scheduleLength = new GraphHandler().GetMaximumCost(graph, chromosome.TopologicalOrder);
            chromosome.ScheduleLength = scheduleLength;
            chromosome.Fitness = 1 / (scheduleLength + 1);

            return chromosome.Fitness.Value;
        }
    }
}