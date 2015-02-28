using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StepFive
{
    public class MaximumNumber
    {
        private readonly int threadPoolSize;
        private readonly int partitionSize;
        private ConcurrentBag<int> results;
 
        public MaximumNumber(int threadPoolSize, int partitionSize)
        {
            this.threadPoolSize = threadPoolSize;
            this.partitionSize = partitionSize;
            results = new ConcurrentBag<int>();
        }


        public int GetFrom(List<int> initialCollection)
        {
            var numbers = new List<int>(initialCollection);

            while (true)
            {                                
                FindMaximumAsParallelFrom(numbers);

                if (results.Count == 1)
                {
                    int maximumNumber;
                    results.TryTake(out maximumNumber);

                    return maximumNumber;
                }

                numbers = results.ToList();
                results = new ConcurrentBag<int>();
            }                       
        }

        private void FindMaximumAsParallelFrom(List<int> numbers)
        {
            var rangePartitioner = Partitioner.Create(0, numbers.Count, partitionSize);            

            Parallel.ForEach(
                    rangePartitioner,
                    new ParallelOptions { MaxDegreeOfParallelism = threadPoolSize },
                    range =>
                    {
                        var partition = new List<int>();

                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            partition.Add(numbers[i]);
                        }

                        results.Add(partition.Max());
                    });
        }
    }
}
