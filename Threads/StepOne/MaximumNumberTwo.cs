using System.Collections.Generic;
using System.Threading;

namespace StepOne
{
    public class MaximumNumberTwo
    {
        private readonly object locker = new object();

        private readonly int threadPoolSize;
        private readonly int partitionSize;

        private static List<int> candidates;
        private Task[] tasks;

        private Partitioner partitioner;

        public MaximumNumberTwo(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;
            this.partitionSize = partitionSize;
            candidates = new List<int>();

            StartTasks();
        }

        public int GetFrom(List<int> numbers)
        {
            partitioner = new Partitioner(partitionSize, numbers);

            while (true)
            {
                if (candidates.Count == 1)
                {
                    ShutDownThreads();
                    break;
                }

                AssignWork();

                if (partitioner.Partitions.Count == 0 && candidates.Count > 1)
                {
                    partitioner = new Partitioner(partitionSize, candidates);
                    candidates = new List<int>();
                }
            }

            return candidates[0];
        }

        private void StartTasks()
        {
            tasks = new Task[threadPoolSize];

            for (int i = 0; i < threadPoolSize; i++)
            {
                tasks[i] = new Task();
            }
        }

        private void ShutDownThreads()
        {
            foreach (Task task in tasks)
            {
                task.AddPartition(new List<int>());
                task.TaskFinished.WaitOne();
            }
        }

        private void AssignWork()
        {
            var activeTasks = new List<Task>();
            foreach (Task task in tasks)
            {
                if (partitioner.Partitions.Count > 0)
                {
                    task.AddPartition(GetPartiton());
                    activeTasks.Add(task);
                }
            }

            WaitTasksToFinish(activeTasks);    
            CollectResultsFrom(activeTasks);
        }

        private List<int> GetPartiton()
        {
            List<int> partition;

            Monitor.Enter(locker);
            try
            {
                partition = partitioner.Partitions.Dequeue();
            }
            finally 
            {                
                Monitor.Exit(locker);
            }

            return partition;
        }

        private void WaitTasksToFinish(IEnumerable<Task> tasks)
        {
            foreach (Task task in tasks)
            {
                task.TaskFinished.WaitOne();
            }
        }

        private void CollectResultsFrom(IEnumerable<Task> tasks)
        {
            foreach (Task task in tasks)
            {
                candidates.AddRange(task.GetResults());
            }
        }
    }
}
