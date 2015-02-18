using System;
using System.Collections.Generic;

namespace StepOne
{
    public class Partitioner
    {
        private readonly int partitionSize;
        private readonly List<int> initialCollection;

        public Queue<List<int>> Partitions { get; private set; }

        public Partitioner(int partitionSize, List<int> initialCollection)
        {
            this.partitionSize = partitionSize;
            this.initialCollection = initialCollection;
            Partitions = GetPartitions();
        }

        private Queue<List<int>> GetPartitions()
        {
            var partitions = new Queue<List<int>>();

            int requiredPartitions = (int)Math.Ceiling(initialCollection.Count / (double)partitionSize);

            for (int i = 0; i < requiredPartitions; i++)
            {
                partitions.Enqueue(GetPartition(i));
            }

            return partitions;
        }

        private List<int> GetPartition(int partitionNumber)
        {
            var partition = new List<int>();

            int lowerBound = partitionNumber * partitionSize;
            int higherBound = lowerBound + partitionSize;

            if (higherBound > initialCollection.Count)
            {
                higherBound = lowerBound + (initialCollection.Count - lowerBound);
            }

            for (int i = lowerBound; i < higherBound; ++i)
            {
                partition.Add(initialCollection[i]);
            }

            return partition;
        }
    }
}
