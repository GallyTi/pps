using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class Fork
{
    public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
}

class Philosopher
{
    public int Id { get; }
    private Fork LeftFork, RightFork;

    public Philosopher(int id, Fork leftFork, Fork rightFork)
    {
        Id = id;
        LeftFork = leftFork;
        RightFork = rightFork;
    }

    public async Task ThinkAndEatAsync()
    {
        await Think();
        await EatAsync();
    }

    private Task Think()
    {
        Console.WriteLine($"Philosopher {Id} is thinking.");
        return Task.Delay(100); // Simulácia premýšľania
    }

    private async Task EatAsync()
    {
        Console.WriteLine($"Philosopher {Id} is trying to eat.");

        Fork firstFork = Id % 2 == 0 ? LeftFork : RightFork;
        Fork secondFork = Id % 2 == 0 ? RightFork : LeftFork;

        await firstFork.Semaphore.WaitAsync();
        await secondFork.Semaphore.WaitAsync();

        Console.WriteLine($"Philosopher {Id} is eating.");
        await Task.Delay(100); // Simulácia jedla

        firstFork.Semaphore.Release();
        secondFork.Semaphore.Release();

        Console.WriteLine($"Philosopher {Id} has finished eating and is now thinking again.");
    }

    // Sekvenčná verzia metódy Eat
    public void EatSequentially()
    {
        Think().Wait();

        Fork firstFork = Id % 2 == 0 ? LeftFork : RightFork;
        Fork secondFork = Id % 2 == 0 ? RightFork : LeftFork;

        firstFork.Semaphore.Wait();
        secondFork.Semaphore.Wait();

        Console.WriteLine($"Philosopher {Id} is eating sequentially.");
        Thread.Sleep(100); // Simulácia jedla

        firstFork.Semaphore.Release();
        secondFork.Semaphore.Release();

        Console.WriteLine($"Philosopher {Id} has finished eating sequentially and is now thinking again.");
    }
}

class DiningTable
{
    private List<Philosopher> Philosophers = new List<Philosopher>();
    private List<Fork> Forks = new List<Fork>();

    public DiningTable(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Forks.Add(new Fork());
        }

        for (int i = 0; i < count; i++)
        {
            var leftFork = Forks[i];
            var rightFork = Forks[(i + 1) % count];
            Philosophers.Add(new Philosopher(i, leftFork, rightFork));
        }
    }

    public async Task StartDiningAsync()
    {
        var eatingTasks = new List<Task>();
        foreach (var philosopher in Philosophers)
        {
            eatingTasks.Add(philosopher.ThinkAndEatAsync());
        }
        await Task.WhenAll(eatingTasks);
    }

    public void StartDiningSequentially()
    {
        foreach (var philosopher in Philosophers)
        {
            philosopher.EatSequentially();
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var table = new DiningTable(7); // Zmeníme počet filozofov na 5 pre jednoduchosť

        // Sekvenčné vykonávanie
        var sequentialStopwatch = Stopwatch.StartNew();
        table.StartDiningSequentially();
        sequentialStopwatch.Stop();
        Console.WriteLine($"Sequential dining completed in {sequentialStopwatch.ElapsedMilliseconds} ms.");

        // Paralelné vykonávanie
        var parallelStopwatch = Stopwatch.StartNew();
        await table.StartDiningAsync();
        parallelStopwatch.Stop();
        Console.WriteLine($"Parallel dining completed in {parallelStopwatch.ElapsedMilliseconds} ms.");

        // Uloženie výsledkov do JSON
        var results = new
        {
            SequentialTime = sequentialStopwatch.ElapsedMilliseconds,
            ParallelTime = parallelStopwatch.ElapsedMilliseconds
        };

        string jsonString = JsonSerializer.Serialize(results);
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "results.json");
        File.WriteAllText(outputPath, jsonString);

        Console.WriteLine($"Results saved to {outputPath}");
    }
}
