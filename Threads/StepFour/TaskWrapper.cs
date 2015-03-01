using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StepFour
{
    public class TaskWrapper
    {
        public bool Finished { get; private set; }
        private readonly ConcurrentQueue<List<int>> partitions;
        private readonly List<int> results;

        public TaskWrapper(ConcurrentQueue<List<int>> partitions)
        {
            this.partitions = partitions;
            results = new List<int>();
        }

        public void Start()
        {
            if (partitions.Count > 0)
            {
                Task.Run(() => FindMaximum()).ContinueWith(t => Start());
                return;
            }

            Finished = true;
        }

        public List<int> GetResults()
        {
            return results;
        }

        private void FindMaximum()
        {
            List<int> partition = GetNextPartition();
            
            if (partition.Count > 0)
            {
                results.Add(partition.Max());
            }
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
    }
}
