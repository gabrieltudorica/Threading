using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace StepFour
{
    public class MaximumNumber
    {
        private readonly int threadPoolSize;
        private readonly int partitionSize;

        private readonly List<TaskWrapper> activeTasks;
        private readonly ConcurrentQueue<List<int>> partitions;
        private ConcurrentQueue<int> results;

        public MaximumNumber(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;
            this.partitionSize = partitionSize;

            activeTasks = new List<TaskWrapper>();
            partitions = new ConcurrentQueue<List<int>>();
            results = new ConcurrentQueue<int>();

        }

        public int GetFrom(List<int> initialCollection)
        {
            CreatePartitions(initialCollection);

            while (true)
            {
                CreateTasks();
                StartTasks();                
                WaitForTasks();
                CollectResults();
                activeTasks.Clear();

                if (results.Count == 1)
                {
                    return GetNextElement();
                }

                CreatePartitions(results.ToList());
                results = new ConcurrentQueue<int>();
            }
        }

        private void CreatePartitions(List<int> initialCollection)
        {
            var partitioner = Partitioner.Create(0, initialCollection.Count, partitionSize);

            foreach (Tuple<int, int> range in partitioner.GetDynamicPartitions())
            {
                CreatePartition(initialCollection, range);
            }
        }

        private void CreatePartition(List<int> initialCollection, Tuple<int, int> range)
        {
            var partition = new List<int>();

            for (int i = range.Item1; i < range.Item2; i++)
            {
                partition.Add(initialCollection[i]);
            }

            partitions.Enqueue(partition);
        }

        private void CreateTasks()
        {
            for (int i = 0; i < threadPoolSize; i++)
            {
                var task = new TaskWrapper(partitions);
                activeTasks.Add(task);
            }
        }

        private void StartTasks()
        {
            foreach (TaskWrapper taskWrapper in activeTasks)
            {
                taskWrapper.Start();
            }
        }

        private void WaitForTasks()
        {
            bool allTasksFinished = false;

            while (!allTasksFinished)
            {
                allTasksFinished = true;
                
                foreach (TaskWrapper taskWrapper in activeTasks)
                {
                    allTasksFinished &= taskWrapper.Finished;
                }
            }
        }

        private void CollectResults()
        {
            foreach (TaskWrapper taskWrapper in activeTasks)
            {
                AddTaskResults(taskWrapper.GetResults());
            }
        }

        private void AddTaskResults(IEnumerable<int> taskResults)
        {
            foreach (int result in taskResults)
            {
                results.Enqueue(result); 
            }            
        }

        private int GetNextElement()
        {
            int currentValue;
            results.TryDequeue(out currentValue);

            return currentValue;
        }
    }
}