using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class MatrixMultiplier
{
    static void FillMatrixWithRandomNumbers(int[,] matrix, int maxValue)
    {
        Random rand = new Random();
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = rand.Next(maxValue);
            }
        }
    }

    static void FillMatrixWithRandomNumbers(float[,] matrix, int maxValue)
    {
        Random rand = new Random();
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = (float)(rand.NextDouble() * maxValue);
            }
        }
    }

    static void MultiplyMatrices(int[,] A, int[,] B, int[,] C, int threads)
    {
        int r1 = A.GetLength(0), c1 = A.GetLength(1), r2 = B.GetLength(0), c2 = B.GetLength(1);
        if (c1 != r2)
        {
            Console.WriteLine("Matrices cannot be multiplied");
            return;
        }

        ParallelOptions options = new ParallelOptions();
        options.MaxDegreeOfParallelism = threads;

        Parallel.For(0, r1, options, i =>
        {
            for (int j = 0; j < c2; j++)
            {
                int sum = 0;
                for (int k = 0; k < c1; k++)
                {
                    sum += A[i, k] * B[k, j];
                }
                C[i, j] = sum;
            }
        });
    }


    static void MultiplyMatrices(float[,] A, float[,] B, float[,] C, int threads)
    {
        int r1 = A.GetLength(0), c1 = A.GetLength(1), r2 = B.GetLength(0), c2 = B.GetLength(1);
        if (c1 != r2)
        {
            Console.WriteLine("Matrices cannot be multiplied");
            return;
        }

        ParallelOptions options = new ParallelOptions();
        options.MaxDegreeOfParallelism = threads;

        Parallel.For(0, r1, options, i =>
        {
            for (int j = 0; j < c2; j++)
            {
                C[i, j] = 0;
                for (int k = 0; k < c1; k++)
                {
                    C[i, j] += A[i, k] * B[k, j];
                }
            }
        });
    }

    static void WriteToJSON(TimeSpan elapsed, int rows, int columns, string method, int threads, StreamWriter outputFile, string dataType)
    {
        outputFile.WriteLine($"  \"{dataType} - {method} ({threads} threads)\": {{");
        outputFile.WriteLine($"    \"time\": \"{elapsed.TotalMilliseconds} ms\",");
        outputFile.WriteLine($"    \"matrix_dimensions\": [{rows}, {columns}]");
        outputFile.WriteLine("  },");
    }

    static void WriteToJSON(TimeSpan elapsed, float rows, int columns, string method, int threads, StreamWriter outputFile, string dataType)
    {
        outputFile.WriteLine($"  \"{dataType} - {method} ({threads} threads)\": {{");
        outputFile.WriteLine($"    \"time\": \"{elapsed.TotalMilliseconds} ms\",");
        outputFile.WriteLine($"    \"matrix_dimensions\": [{rows}, {columns}]");
        outputFile.WriteLine("  },");
    }


    static void Main(string[] args)
    {
        int maxValue = 50;
        int[] matrixDimensions = { 500, 1000, 2000 };
        int[] threadCounts = { 2, 4, 8 };

        using (StreamWriter outputFile = new StreamWriter("output/results.json"))
        {
            outputFile.WriteLine("{");
            outputFile.Flush();

            foreach (int n in matrixDimensions)
            {
                Console.WriteLine(String.Format("{0} dimensions", n));
                int[,] A_int = new int[n, n];
                int[,] B_int = new int[n, n];
                int[,] C_int = new int[n, n];

                float[,] A_float = new float[n, n];
                float[,] B_float = new float[n, n];
                float[,] C_float = new float[n, n];

                FillMatrixWithRandomNumbers(A_int, maxValue);
                FillMatrixWithRandomNumbers(B_int, maxValue);

                FillMatrixWithRandomNumbers(A_float, maxValue);
                FillMatrixWithRandomNumbers(B_float, maxValue);

                foreach (int threads in threadCounts)
                {
                    Console.WriteLine(String.Format("using {0} threads",threads));
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Console.WriteLine(String.Format("int"));
                    MultiplyMatrices(A_int, B_int, C_int, threads);

                    stopwatch.Stop();
                    TimeSpan elapsedInt = stopwatch.Elapsed;

                    WriteToJSON(elapsedInt, n, n, "parallel", threads, outputFile, "int");

                    Array.Clear(C_int, 0, C_int.Length);

                    stopwatch.Restart();
                    Console.WriteLine(String.Format("float"));
                    MultiplyMatrices(A_float, B_float, C_float, threads);

                    stopwatch.Stop();
                    TimeSpan elapsedFloat = stopwatch.Elapsed;

                    WriteToJSON(elapsedFloat, n, n, "parallel", threads, outputFile, "float");

                    Array.Clear(C_float, 0, C_float.Length);

                    outputFile.Flush();
                }
            }
            outputFile.WriteLine("}");
        }
    }
}
