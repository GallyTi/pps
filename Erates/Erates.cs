using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

class SieveOfEratosthenes
{
    public static int[] FindPrimesSequential(int limit)
    {
        bool[] isPrime = new bool[limit + 1];
        for (int i = 2; i <= limit; i++)
        {
            isPrime[i] = true;
        }

        for (int num = 2; num * num <= limit; num++)
        {
            if (isPrime[num])
            {
                for (int multiple = num * num; multiple <= limit; multiple += num)
                {
                    isPrime[multiple] = false;
                }
            }
        }

        return Enumerable.Range(2, limit - 1).Where(i => isPrime[i]).ToArray();
    }
    public static int[] FindPrimesParallel(int limit, int numberOfThreads)
    {
        bool[] isPrime = new bool[limit + 1];
        Parallel.For(2, limit + 1, new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads }, i => isPrime[i] = true);

        int sqrtLimit = (int)Math.Sqrt(limit);
        Parallel.ForEach(Enumerable.Range(2, sqrtLimit + 1), new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads }, num =>
        {
            if (isPrime[num])
            {
                for (int multiple = num * num; multiple <= limit; multiple += num)
                {
                    isPrime[multiple] = false;
                }
            }
        });

        return Enumerable.Range(2, limit - 1).Where(i => isPrime[i]).ToArray();
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        int limit = 10000000; // Hľadáme prvočísla do 1 milióna

        // Sekvenčné vyhľadávanie prvočísel
        var stopwatchSequential = Stopwatch.StartNew();
        var primesSequential = SieveOfEratosthenes.FindPrimesSequential(limit);
        stopwatchSequential.Stop();
        Console.WriteLine($"Sequential found {primesSequential.Length} primes in {stopwatchSequential.ElapsedMilliseconds} ms.");

        // Paralelné vyhľadávanie prvočísel
        var stopwatchParallel2 = Stopwatch.StartNew();
        var noOfThreads = 2;
        var primesParallel2 = SieveOfEratosthenes.FindPrimesParallel(limit,noOfThreads);
        stopwatchParallel2.Stop();
        Console.WriteLine($"Parallel found {primesParallel2.Length} primes in {stopwatchParallel2.ElapsedMilliseconds} ms using {noOfThreads} threads");

        var stopwatchParallel4 = Stopwatch.StartNew();
        noOfThreads = 4;
        var primesParallel4 = SieveOfEratosthenes.FindPrimesParallel(limit, noOfThreads);
        stopwatchParallel4.Stop();
        Console.WriteLine($"Parallel found {primesParallel4.Length} primes in {stopwatchParallel4.ElapsedMilliseconds} ms using {noOfThreads} threads");

        var stopwatchParallel8 = Stopwatch.StartNew();
        noOfThreads = 8;
        var primesParallel8 = SieveOfEratosthenes.FindPrimesParallel(limit, noOfThreads);
        stopwatchParallel8.Stop();
        Console.WriteLine($"Parallel found {primesParallel8.Length} primes in {stopwatchParallel8.ElapsedMilliseconds} ms using {noOfThreads} threads");


        // Uloženie výsledkov do JSON
        var results = new
        {
            Limit = limit,
            SequentialTimeMs = stopwatchSequential.ElapsedMilliseconds,
            ParallelTimeMs2Threads = stopwatchParallel2.ElapsedMilliseconds,
            ParallelTimeMs4Threads = stopwatchParallel4.ElapsedMilliseconds,
            ParallelTimeMs8Threads = stopwatchParallel8.ElapsedMilliseconds,
            PrimesFoundSequential = primesSequential.Length,
            PrimesFoundParallelUsing2Threads = primesParallel2.Length,
            PrimesFoundParallelUsing4Threads = primesParallel4.Length,
            PrimesFoundParallelUsing8Threads = primesParallel8.Length
        };

        string jsonString = JsonSerializer.Serialize(results);
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "results.json");
        await File.WriteAllTextAsync(outputPath, jsonString);

        Console.WriteLine($"Results saved to {outputPath}");
    }
}