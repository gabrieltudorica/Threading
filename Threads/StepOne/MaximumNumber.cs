using System.Collections.Generic;
using System.Threading;

namespace StepOne
{
    public class MaximumNumber
    {
        private readonly object locker = new object();

        private readonly int threadPoolSize;
        private readonly int partitionSize;

        private static Queue<List<int>> inputPartitions;
        private static List<int> partitionsMaximumNumbers;

        private AutoResetEvent[] taskCompleted;
        private AutoResetEvent[] waitForTask;

        private Partitioner partitioner;

        public MaximumNumber(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;
            this.partitionSize = partitionSize;
            partitionsMaximumNumbers = new List<int>();
            inputPartitions = new Queue<List<int>>(threadPoolSize);
        }

        public int GetFrom(List<int> numbers)
        {
            StartThreads();

            partitioner = new Partitioner(partitionSize, numbers);

            while (true)
            {
                if (partitionsMaximumNumbers.Count == 1)
                {
                    ShutDownThreads();
                    break;
                }

                StartWork();

                if (partitioner.Partitions.Count == 0 && partitionsMaximumNumbers.Count > 1)
                {
                    partitioner = new Partitioner(partitionSize, partitionsMaximumNumbers);
                    partitionsMaximumNumbers = new List<int>();
                }
            }

            return partitionsMaximumNumbers[0];
        }

        private void ShutDownThreads()
        {
            for (int i = 0; i < threadPoolSize; i++)
            {
                inputPartitions.Enqueue(new List<int>());
                waitForTask[i].Set();
            }
        }

        private void StartWork()
        {
            for (int i = 0; i < threadPoolSize; i++)
            {
                AddNextPartition();
                waitForTask[i].Set();
            }

            WaitHandle.WaitAll(taskCompleted);
        }

        private void AddNextPartition()
        {
            if (partitioner.Partitions.Count == 0)
            {
                inputPartitions.Enqueue(new List<int>());
                return;
            }

            inputPartitions.Enqueue(partitioner.Partitions.Dequeue());
        }

        private void StartThreads()
        {
            waitForTask = new AutoResetEvent[threadPoolSize];
            taskCompleted = new AutoResetEvent[threadPoolSize];

            for (int i = 0; i < threadPoolSize; i++)
            {
                int temp = i;
                waitForTask[temp] = new AutoResetEvent(false);
                taskCompleted[temp] = new AutoResetEvent(false);

                var thread =
                    new Thread(
                        (() =>
                            FindMaximumValueInPartition(waitForTask[temp], taskCompleted[temp])));
                thread.Start();
            }
        }

        private void FindMaximumValueInPartition(AutoResetEvent waitForTask, AutoResetEvent taskCompleted)
        {
            int maximumValue = int.MinValue;

            while (true)
            {
                waitForTask.WaitOne();

                List<int> currentPartition = inputPartitions.Dequeue();

                if (currentPartition.Count == 0)
                {
                    taskCompleted.Set();
                    return;
                }

                foreach (int value in currentPartition)
                {
                    if (value > maximumValue)
                    {
                        maximumValue = value;
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

                taskCompleted.Set();
            }
        }
    }
}
