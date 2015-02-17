using System.Collections.Generic;
using System.Threading;

namespace StepOne
{
    public class MaximumNumber
    {
        private readonly object locker = new object();

        private readonly int maxThreadPoolSize;
        private readonly int partitionSize;

        private static List<int> partitionsMaximumNumbers;

        private WaitHandle[] waitHandles;
        private Partitioner partitioner;

        public MaximumNumber(int maxThreadPoolSize, int partitionSize)
        {
            this.maxThreadPoolSize = maxThreadPoolSize;
            this.partitionSize = partitionSize;
            partitionsMaximumNumbers = new List<int>();
        }

        public int GetFrom(List<int> numbers)
        {
            partitioner = new Partitioner(partitionSize, numbers);

            while (partitioner.Partitions.Count != 0)
            {
                if (partitionsMaximumNumbers.Count == 1)
                {
                    return partitionsMaximumNumbers[0];
                }

                SearchInParallelBatches(partitioner.GetPartitionBatches(maxThreadPoolSize));

                if (partitioner.Partitions.Count == 0 && partitionsMaximumNumbers.Count > 1)
                {
                    RestartWithPartition(partitionsMaximumNumbers);
                }
            }

            return partitionsMaximumNumbers[0];
        }

        private void SearchInParallelBatches(List<List<int>> partitionBatches)
        {
            waitHandles = GetWaitHandles(partitionBatches.Count);
            StartThreadsFor(partitionBatches, waitHandles);
            
            WaitHandle.WaitAll(waitHandles);

            foreach (WaitHandle waitHandle in waitHandles)
            {
                waitHandle.Dispose();
            }
        }

        private static WaitHandle[] GetWaitHandles(int count)
        {
            var waitHandles = new WaitHandle[count];

            for (int i = 0; i < count; i++)
            {
                waitHandles[i] = new EventWaitHandle(false, EventResetMode.AutoReset);
            }

            return waitHandles;
        }

        private void StartThreadsFor(List<List<int>> partitionBatches, WaitHandle[] eventWaitHandles)
        {
            for (int index = 0; index < partitionBatches.Count; ++index)
            {
                int temp = index;
                new Thread((() => FindMaximumValueInPartition(partitionBatches[temp], (EventWaitHandle)eventWaitHandles[temp]))).Start();
            }
        }

        private void FindMaximumValueInPartition(IEnumerable<int> partition, EventWaitHandle waitHandle)
        {
            int maximumValue = int.MinValue;

            foreach (int currentValue in partition)
            {
                if (currentValue > maximumValue)
                {
                    maximumValue = currentValue;
                }
            }

            Monitor.Enter(locker);

            try
            {
                partitionsMaximumNumbers.Add(maximumValue);
            }
            finally
            {
                Monitor.Exit(locker);
            }

            waitHandle.Set();
        }

        private void RestartWithPartition(List<int> partition)
        {
            partitioner = new Partitioner(partitionSize, partition);
            partitionsMaximumNumbers = new List<int>();
        }
    }
}
