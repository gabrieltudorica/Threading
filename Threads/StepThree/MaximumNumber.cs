using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StepThree
{
    public class MaximumNumber
    {
        private readonly int threadPoolSize;
        private readonly int partitionSize;       
        private readonly ConcurrentQueue<int> numbers;

        public MaximumNumber(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;
            this.partitionSize = partitionSize;
            numbers = new ConcurrentQueue<int>();
        }

        public int GetFrom(List<int> initialCollection)
        {
            CreateInitialCollection(initialCollection);

            while (true)
            {
                if (numbers.Count == 1)
                {
                    return GetNextElement();
                }

                var activeTasks = new List<Task>();

                for (int i = 0; i < threadPoolSize; i++)
                {
                    Task task = GetTask();         
                    activeTasks.Add(task);
                    task.Start();
                }

                Task.WaitAll(activeTasks.ToArray());                
            }
        }

        private void CreateInitialCollection(IEnumerable<int> initialCollection)
        {
            foreach (int value in initialCollection)
            {
                numbers.Enqueue(value);
            }
        }

        private Task GetTask()
        {
            var task = new Task(() =>
            {
                List<int> currentPartition = GetPartition();
                
                if (currentPartition.Count > 1)
                {
                    numbers.Enqueue(currentPartition.Max());
                }
            });

            return task;
        }

        private List<int> GetPartition()
        {
            var partition = new List<int>();

            for (int i = 0; i < partitionSize; i++)
            {
                if (numbers.Count == 1)
                {
                    break;
                }

                partition.Add(GetNextElement());
            }

            return partition;
        }

        private int GetNextElement()
        {
            int currentValue;
            numbers.TryDequeue(out currentValue);

            return currentValue;
        }
    }
}