﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Threading
{
    public class Program
    {
        private static Stopwatch watch;
        private static Random random;

        static void Main(string[] args)
        {
            watch = new Stopwatch();
            random = new Random(555);

            List<int> randomNumbers = GenerateRandomNumbers(random.Next(1000000, 2000000));

            StepZero(randomNumbers);
            //StepOne(randomNumbers);
            //StepTwo(randomNumbers);
            //StepThree(randomNumbers);
            StepFour(randomNumbers);
            //StepFive(randomNumbers);

            Console.Read();
        }

        private static List<int> GenerateRandomNumbers(int count)
        {
            var numbers = new List<int>();

            Console.WriteLine("Generating {0} random numbers...", count);
            watch.Start();

            for (int i = 0; i < count; i++)
            {
                numbers.Add(random.Next(0, 99999));
            }

            watch.Stop();
            Console.WriteLine("Finished generating numbers. Generation took {0} seconds", watch.Elapsed.Seconds);

            return numbers;
        }

        private static void StepZero(List<int> numbers)
        {
            watch.Reset();

            Console.WriteLine();
            Console.WriteLine("Finding maximum value using StepZero setup...");
            watch.Start();

            var stepZero = new StepZero.MaximumNumber();
            int maximumNumber = stepZero.GetFrom(numbers);

            watch.Stop();
            Console.WriteLine("Finished finding maximum value in {0} miliseconds", watch.Elapsed.Milliseconds);

            Console.WriteLine("Found maximum value is {0}", maximumNumber);
        }

        private static void StepOne(List<int> numbers)
        {
            watch.Reset();

            Console.WriteLine();
            Console.WriteLine("Finding maximum value using StepOne setup...");
            watch.Start();

            var bla = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9};

            var maximumNumber = new StepOne.MaximumNumberTwo(3, 3598);
            int maximulValue = maximumNumber.GetFrom(numbers);

            watch.Stop();
            Console.WriteLine("Finished finding maximum value in {0} miliseconds", watch.Elapsed.Milliseconds);

            Console.WriteLine("Found maximum value is {0}", maximulValue);
        }

        private static void StepTwo(List<int> numbers)
        {
            watch.Reset();

            Console.WriteLine();
            Console.WriteLine("Finding maximum value using StepTwo setup...");
            watch.Start();

            var maximumNumber = new StepTwo.MaximumNumber(8, 300);
            int maximumValue = maximumNumber.GetFrom(numbers);

            watch.Stop();
            Console.WriteLine("Finished finding maximum value in {0} miliseconds", watch.Elapsed.Milliseconds);

            Console.WriteLine("Found maximum value is {0}", maximumValue);
        }

        private static void StepThree(List<int> numbers)
        {
            watch.Reset();

            Console.WriteLine();
            Console.WriteLine("Finding maximum value using StepThree setup...");
            watch.Start();

            var maximumNumber = new StepThree.MaximumNumber(2, 500);
            int maximumValue = maximumNumber.GetFrom(numbers);

            watch.Stop();
            Console.WriteLine("Finished finding maximum value in {0} miliseconds", watch.Elapsed.Milliseconds);

            Console.WriteLine("Found maximum value is {0}", maximumValue);
        }

        private static void StepFour(List<int> numbers)
        {
            watch.Reset();

            Console.WriteLine();
            Console.WriteLine("Finding maximum value using StepFour setup...");
            watch.Start();

            var maximumNumber = new StepFour.MaximumNumber(2, 500);
            int maximumValue = maximumNumber.GetFrom(numbers);

            watch.Stop();
            Console.WriteLine("Finished finding maximum value in {0} miliseconds", watch.Elapsed.Milliseconds);

            Console.WriteLine("Found maximum value is {0}", maximumValue);
        }

        private static void StepFive(List<int> numbers)
        {
            watch.Reset();

            Console.WriteLine();
            Console.WriteLine("Finding maximum value using StepFive setup...");
            watch.Start();

            var maximumNumber = new StepFive.MaximumNumber(2, 500);
            int maximumValue = maximumNumber.GetFrom(numbers);

            watch.Stop();
            Console.WriteLine("Finished finding maximum value in {0} miliseconds", watch.Elapsed.Milliseconds);

            Console.WriteLine("Found maximum value is {0}", maximumValue);
        }
    }
}
