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

    public static int[] FindPrimesParallel(int limit)
    {
        bool[] isPrime = new bool[limit + 1];
        Parallel.For(2, limit + 1, (i) => isPrime[i] = true);

        int sqrtLimit = (int)Math.Sqrt(limit);
        Parallel.ForEach(Enumerable.Range(2, sqrtLimit + 1), (num) =>
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
        int limit = 999999999; // Hľadáme prvočísla do 1 milióna

        // Sekvenčné vyhľadávanie prvočísel
        var stopwatchSequential = Stopwatch.StartNew();
        var primesSequential = SieveOfEratosthenes.FindPrimesSequential(limit);
        stopwatchSequential.Stop();
        Console.WriteLine($"Sequential found {primesSequential.Length} primes in {stopwatchSequential.ElapsedMilliseconds} ms.");

        // Paralelné vyhľadávanie prvočísel
        var stopwatchParallel = Stopwatch.StartNew();
        var primesParallel = SieveOfEratosthenes.FindPrimesParallel(limit);
        stopwatchParallel.Stop();
        Console.WriteLine($"Parallel found {primesParallel.Length} primes in {stopwatchParallel.ElapsedMilliseconds} ms.");

        // Uloženie výsledkov do JSON
        var results = new
        {
            Limit = limit,
            SequentialTimeMs = stopwatchSequential.ElapsedMilliseconds,
            ParallelTimeMs = stopwatchParallel.ElapsedMilliseconds,
            PrimesFoundSequential = primesSequential.Length,
            PrimesFoundParallel = primesParallel.Length
        };

        string jsonString = JsonSerializer.Serialize(results);
        string outputPath = @"D:\School\PPS\Erates\output\results.json";
        await File.WriteAllTextAsync(outputPath, jsonString);

        Console.WriteLine($"Results saved to {outputPath}");
    }
}