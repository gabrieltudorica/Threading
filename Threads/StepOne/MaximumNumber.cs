using System.Collections.Generic;
using System.Threading;

namespace StepOne
{
    public class MaximumNumber
    {
        private readonly object locker = new object();

        private readonly int threadPoolSize;
        private readonly int partitionSize;

        private static Queue<List<int>> workingPartitions;
        private static List<int> partitionsMaximumNumbers;

        private AutoResetEvent[] taskCompleted;
        private AutoResetEvent[] waitForTask;

        private Partitioner partitioner;

        public MaximumNumber(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;
            this.partitionSize = partitionSize;
            partitionsMaximumNumbers = new List<int>();
            workingPartitions = new Queue<List<int>>(threadPoolSize);
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
                workingPartitions.Enqueue(new List<int>());
                waitForTask[i].Set();
            }
        }

        private void StartWork()
        {
            var activeThreads = new List<AutoResetEvent>();

            for (int i = 0; i < threadPoolSize; i++)
            {
                if (partitioner.Partitions.Count > 0)
                {
                    AddPartition();
                    activeThreads.Add(taskCompleted[i]);
                    waitForTask[i].Set();
                }                
            }

            WaitHandle.WaitAll(activeThreads.ToArray());
        }

        private void AddPartition()
        {
            Monitor.Enter(locker);
            try
            {
                workingPartitions.Enqueue(partitioner.Partitions.Dequeue());
            }
            finally
            {
                Monitor.Exit(locker);
            }
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
                //Console.WriteLine("Waiting for work");
                waitForTask.WaitOne();
                //Console.WriteLine("Started work");


                List<int> currentPartition = GetNextPartition();
                if (currentPartition.Count == 0)
                {
                    //Console.WriteLine("count is zero, exiting thread");
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

                AddResult(maximumValue);

                //Console.WriteLine("finished");
                taskCompleted.Set();
            }
        }

        private List<int> GetNextPartition()
        {
            List<int> currentPartition;

            Monitor.Enter(locker);
            try
            {
                currentPartition = workingPartitions.Dequeue();
            }
            finally
            {
                Monitor.Exit(locker);
            }

            return currentPartition;
        }

        private void AddResult(int maximumValue)
        {
            //Console.WriteLine("found maximum value in partition " + maximumValue);
            Monitor.Enter(locker);
            try
            {
                partitionsMaximumNumbers.Add(maximumValue);
            }
            finally
            {
                Monitor.Exit(locker);
            }
        }
    }
}