using System.Collections.Generic;
using System.Threading;

namespace StepOne
{
    public class Task
    {
        public AutoResetEvent TaskFinished = new AutoResetEvent(false);
        private readonly AutoResetEvent waitForTask = new AutoResetEvent(false);

        private readonly Queue<List<int>> partitions;
        private readonly Queue<int> results; 

        public Task()
        {
            partitions = new Queue<List<int>>();
            results = new Queue<int>();
            StartThread();
        }

        public void AddPartition(List<int> partition)
        {
            partitions.Enqueue(partition);
            waitForTask.Set();
        }

        public List<int> GetResults()
        {
            var maximumNumbersFound = new List<int>();

            while (results.Count > 0)
            {
                maximumNumbersFound.Add(results.Dequeue());
            }
           
            return maximumNumbersFound;
        }

        private void StartThread()
        {
            var thread = new Thread(FindMaxmimumInPartition);
            thread.Start();
        }

        private void FindMaxmimumInPartition()
        {         
            while (true)
            {
                waitForTask.WaitOne();

                int maximumValue = int.MinValue;
                List<int> currentPartition = partitions.Dequeue();

                if (currentPartition.Count == 0)
                {
                    TaskFinished.Set();
                    return;
                }

                foreach (int value in currentPartition)
                {
                    if (value > maximumValue)
                    {
                        maximumValue = value;
                    }
                }

                results.Enqueue(maximumValue);

                TaskFinished.Set();
            }
        }
    }
}
