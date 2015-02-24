using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StepTwo
{
    public class MaximumNumber
    {
        private readonly object resultsLocker = new object();
        private readonly object partitionLocker = new object();

        private readonly int threadPoolSize;
        private readonly int partitionSize;

        private static Semaphore semaphore;
        private static List<int> partitionsMaximumNumbers;        

        private AutoResetEvent[] waitForTasks;
        private AutoResetEvent[] tasksCompleted;
        
        private int numberOfConcurrentThreads = 4;

        private Partitioner partitioner;

        private bool executing = true;

        public MaximumNumber(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;
            this.partitionSize = partitionSize;
            semaphore = new Semaphore(numberOfConcurrentThreads, threadPoolSize);
            partitionsMaximumNumbers = new List<int>();
        }

        public int GetFrom(List<int> numbers)
        {
            partitioner = new Partitioner(partitionSize, numbers);
            StartThreads();

            while (true)
            {
                WaitHandle.WaitAll(tasksCompleted);

                if (partitionsMaximumNumbers.Count == 1)
                {
                    ShutDownThreads();
                    break;
                }                

                if (partitionsMaximumNumbers.Count > 1)
                {
                    partitioner = new Partitioner(partitionSize, partitionsMaximumNumbers);
                    partitionsMaximumNumbers = new List<int>();
                    ResumeThreads();
                }
            }

            return partitionsMaximumNumbers[0];
        }

        private void StartThreads()
        {
            waitForTasks = new AutoResetEvent[threadPoolSize];
            tasksCompleted = new AutoResetEvent[threadPoolSize];

            for (int i = 0; i < threadPoolSize; i++)
            {
                int temp = i;
                waitForTasks[temp] = new AutoResetEvent(false);
                tasksCompleted[temp] = new AutoResetEvent(false);

                var thread =
                    new Thread(
                        (() =>
                            FindMaximumValueInPartition(waitForTasks[temp], tasksCompleted[temp])));
                thread.Start();
            }
        }

        private void FindMaximumValueInPartition(AutoResetEvent waitForTask, AutoResetEvent taskCompleted)
        {
            while (executing)
            {
                semaphore.WaitOne();

                List<int> currentPartition;

                lock (partitionLocker)
                {
                    currentPartition = partitioner.GetNextPartition();                   
                }

                if (currentPartition == null)
                {
                    taskCompleted.Set();
                    //semaphore.Release();
                    waitForTask.WaitOne();
                    continue;
                }

                lock (resultsLocker)
                {
                    partitionsMaximumNumbers.Add(currentPartition.Max());
                }

                semaphore.Release();
            }
        }

        private void ShutDownThreads()
        {
            executing = false;
        }

        private void ResumeThreads()
        {
            foreach (AutoResetEvent resetEvent in waitForTasks)
            {
                resetEvent.Set();
            }
        }
    }
}