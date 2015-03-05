using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StepFour
{
    public class MaximumNumber
    {
        private readonly int threadPoolSize;

        private readonly List<Task<int>> activeTasks;
        private ConcurrentQueue<int> results;
        private readonly Partitions partitions;

        public MaximumNumber(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;

            activeTasks = new List<Task<int>>();
            results = new ConcurrentQueue<int>();
            partitions = new Partitions(partitionSize);
        }

        private void Setup(List<int> partition)
        {
            partitions.Generate(partition);            
            CreateTasks();
            activeTasks.ForEach(task=>task.Start());
        }

        public int GetFrom(List<int> initialCollection)
        {
            Setup(initialCollection);

            while (true)
            {
                Task<int> task = GetCompletedTask();
                results.Enqueue(task.Result);
                activeTasks.Remove(task);

                if (partitions.AnyLeft())
                {             
                    ReplaceTask();
                }                

                if (IsFinished())
                {
                    return results.Single();
                }

                if (activeTasks.Count == 0)
                {
                    Setup(results.ToList());
                    results = new ConcurrentQueue<int>();                    
                }
            }
        }

        private Task<int> GetCompletedTask()
        {
            int index = Task.WaitAny(activeTasks.ToArray());
            var task = activeTasks[index];
            return task;
        }

        private bool IsFinished()
        {
            return activeTasks.Count == 0 && results.Count == 1;
        }

        private void ReplaceTask()
        {
            Task<int> task = GetTask(partitions.GetNext());
            activeTasks.Add(task);
            task.Start();
        }
       
        private void CreateTasks()
        {
            for (int i = 0; i < threadPoolSize; i++)
            {
                if (partitions.AnyLeft())
                {
                    activeTasks.Add(GetTask(partitions.GetNext()));
                }
            }
        }

        private Task<int> GetTask(List<int> partition)
        {
            return new Task<int>(partition.Max);
        }
    }
}