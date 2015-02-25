using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            StartTasks();

            while (true)
            {
                if (numbers.Count == 1)
                {
                    return GetNextElement();
                }

                Thread.Sleep(1000);
            }
        }

        private void CreateInitialCollection(IEnumerable<int> initialCollection)
        {
            foreach (int value in initialCollection)
            {
                numbers.Enqueue(value);
            }
        }

        private void StartTasks()
        {
            for (int i = 0; i < threadPoolSize; i++)
            {
                StartTask();
            }   
        }

        private void StartTask()
        {
            if (numbers.Count > 1)
            {
                Task.Run(() => FindMaximum()).ContinueWith(t => StartTask());
            }
        }

        private void FindMaximum()
        {
            List<int> currentPartition = GetPartition();

            if (currentPartition.Count > 1)
            {
                numbers.Enqueue(currentPartition.Max());
            }
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