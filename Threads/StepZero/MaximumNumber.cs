using System.Collections.Generic;

namespace StepZero
{
    public class MaximumNumber
    {
        public int GetFrom(List<int> numbers)
        {
            int maximumValue = int.MinValue;

            foreach (int currentNumber in numbers)
            {
                if (currentNumber > maximumValue)
                {
                    maximumValue = currentNumber;
                }
            }

            return maximumValue;
        }
    }
}
