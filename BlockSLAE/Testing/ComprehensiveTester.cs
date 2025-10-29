using System.Diagnostics;
using BlockSLAE.Factory;
using BlockSLAE.IO;
using BlockSLAE.Smoothing;
using BlockSLAE.Testing.Configs;
using BlockSLAE.Testing.Structures;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlockSLAE.Testing;

public class ComprehensiveTester(string basePath)
{
    private readonly TestResultExporter _exporter = new(Path.Combine(basePath, "Results\\"));

    public void RunAllTests()
    {
        int[] slaeNumbers = [1, 2, 3, 4];
        int[] degreesOfParallelism = [1, 2, 4, Environment.ProcessorCount];

        var smoothingStrategies = new[]
        {
            new SmoothingConfig("ResidualSmoothing", new ResidualSmoothing()),
            new SmoothingConfig("NoSmoothing", new LackSmoothing())
        };

        var solverConfigs = SolverFactory.GetRegisteredSolverNames();

        foreach (var slaeNumber in slaeNumbers)
        {
            foreach (var solverConfig in solverConfigs)
            {
                foreach (var smoothing in smoothingStrategies)
                {
                    foreach (var threads in degreesOfParallelism)
                    {
                        RunSingleTest(slaeNumber, solverConfig, smoothing, threads);
                    }
                }
            }
        }
    }

    private void RunSingleTest(
        int slaeNumber,
        string solverName,
        SmoothingConfig smoothing,
        int threads)
    {
        Console.WriteLine($"Running test: SLAE#{slaeNumber}, {solverName}, {smoothing.Name}, {threads} threads");

        try
        {
            var inputPath = Path.Combine(basePath, "Input\\", slaeNumber.ToString());
            var equation = SLAEReader.BuildEquation(inputPath, new BinaryFileHelper());
            var config = SLAEReader.ReadSLAEConfig(inputPath);

            var solver = SolverFactory.CreateSolver(solverName, config, smoothing.Strategy, NullLoggerFactory.Instance);
            solver.SetDegreeOfParallelism(threads);

            var testRun = new TestInfo
            {
                MethodName = solverName,
                SlaeNumber = slaeNumber,
                ThreadsCount = threads,
                SmoothingMethod = smoothing.Name
            };

            solver.IterationCompleted += (iteration, residual, smoothedResidual) =>
            {
                testRun.Results.Add(new TestResult
                {
                    Iteration = iteration,
                    Residual = residual,
                    SmoothedResidual = smoothedResidual
                });
            };

            var stopwatch = Stopwatch.StartNew();
            _ = solver.Solve(equation);
            stopwatch.Stop();

            testRun.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            _exporter.ExportTestResult(testRun);

            Console.WriteLine($"Completed in {stopwatch.ElapsedMilliseconds} ms");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error in test SLAE#{slaeNumber}, {solverName}, {smoothing.Name}, {threads} threads");
            Console.ResetColor();

            LogErrorToFile(slaeNumber, solverName, smoothing.Name, threads, ex);
        }
    }
    
    private void LogErrorToFile(
        int slaeNumber,
        string solverName,
        string smoothingName,
        int threads,
        Exception ex)
    {
        try
        {
            var logDir = Path.Combine(basePath, "Results");
            Directory.CreateDirectory(logDir);

            var logFile = Path.Combine(logDir, "Errors.log");

            var logEntry =
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                $"SLAE#{slaeNumber}, Solver={solverName}, Smoothing={smoothingName}, Threads={threads}\n" +
                $"{ex}\n" +
                new string('-', 100) + "\n";

            File.AppendAllText(logFile, logEntry);
        }
        catch (Exception fileEx)
        {
            Console.WriteLine($"Failed to log error: {fileEx.Message}");
        }
    }
}