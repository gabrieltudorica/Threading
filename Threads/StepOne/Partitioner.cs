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

        public List<List<int>> GetPartitionBatches(int batchNumber)
        {
            if (Partitions.Count >= batchNumber)
            {
                return GetBatchOf(batchNumber);
            }

            return GetBatchOf(Partitions.Count);
        }

        private List<List<int>> GetBatchOf(int batchNumber)
        {
            var partitions = new List<List<int>>();

            for (int i = 0; i < batchNumber; i++)
            {
                partitions.Add(Partitions.Dequeue());
            }

            return partitions;
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

            for (int i = lowerBound; i < higherBound && i != initialCollection.Count; ++i)
            {
                partition.Add(initialCollection[i]);
            }

            return partition;
        }
    }
}
