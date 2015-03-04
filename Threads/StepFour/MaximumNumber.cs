using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StepFour
{
    public class MaximumNumber
    {
        private readonly int threadPoolSize;
        private readonly int partitionSize;

        private readonly List<Task<int>> activeTasks;
        private readonly ConcurrentQueue<List<int>> partitions;
        private ConcurrentQueue<int> results;

        public MaximumNumber(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;
            this.partitionSize = partitionSize;

            activeTasks = new List<Task<int>>();
            partitions = new ConcurrentQueue<List<int>>();
            results = new ConcurrentQueue<int>();

        }

        public int GetFrom(List<int> initialCollection)
        {
            CreatePartitions(initialCollection);
            CreateTasks();
            StartTasks();

            while (true)
            {
                int index = Task.WaitAny(activeTasks.ToArray());
                var task = activeTasks[index];
                var result = task.Result;
                if(result != 0)
                {
                    results.Enqueue(task.Result);
                }                

                if(partitions.Count > 0)
                {
                    activeTasks.Remove(task);
                    task = GetTask();
                    activeTasks.Add(task);
                    task.Start();
                }
                else
                {
                    activeTasks.Remove(task);
                }

                if (activeTasks.Count == 0 && results.Count == 1)
                {
                    return GetNextElement();
                }

                if (activeTasks.Count == 0)
                {                    
                    RestartWithParition(results.ToList());
                }
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
                activeTasks.Add(GetTask());
            }
        }

        private Task<int> GetTask()
        {
            return new Task<int>(FindMaximum);
        }

        private int FindMaximum()
        {
            List<int> partition = GetNextPartition();

            if (partition.Count > 0)
            {
               return partition.Max();
            }

            return 0;
        }

        private List<int> GetNextPartition()
        {
            List<int> partition;
            if (!partitions.TryDequeue(out partition))
            {
                partition = new List<int>();
            }

            return partition;
        }

        private void StartTasks()
        {
            foreach (Task<int> task in activeTasks)
            {
                task.Start();
            }
        }

        private int GetNextElement()
        {
            int currentValue;
            results.TryDequeue(out currentValue);

            return currentValue;
        }

        private void  RestartWithParition(List<int> partition)
        {
            CreatePartitions(partition);
            results = new ConcurrentQueue<int>();
            
            CreateTasks();
            StartTasks();
        }
    }
}