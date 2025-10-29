using BlockSLAE.Testing.Structures;

namespace BlockSLAE.Testing;

public class TestResultExporter
{
    private readonly string _baseOutputPath;

    public TestResultExporter(string baseOutputPath)
    {
        _baseOutputPath = baseOutputPath;

        if (!Directory.Exists(_baseOutputPath))
        {
            Directory.CreateDirectory(_baseOutputPath);
        }
    }

    public void ExportTestResult(TestInfo test)
    {
        var slaeFolder = Path.Combine(_baseOutputPath, test.SlaeNumber.ToString());
        if (!Directory.Exists(slaeFolder))
        {
            Directory.CreateDirectory(slaeFolder);
        }

        var solverFolder = Path.Combine(slaeFolder, test.MethodName);
        if (!Directory.Exists(solverFolder))
        {
            Directory.CreateDirectory(solverFolder);
        }

        var smoothingFolder = Path.Combine(solverFolder, test.SmoothingMethod);
        if (!Directory.Exists(smoothingFolder))
        {
            Directory.CreateDirectory(smoothingFolder);
        }

        var fileName = $"{test.ThreadsCount} threads.txt";
        var filePath = Path.Combine(smoothingFolder, fileName);

        using var writer = new StreamWriter(filePath);

        var last = test.Results.Last();
        
        writer.WriteLine($"Method: {test.MethodName}");
        writer.WriteLine($"SLAE Number: {test.SlaeNumber}");
        writer.WriteLine($"Threads: {test.ThreadsCount}");
        writer.WriteLine($"Smoothing: {test.SmoothingMethod}");
        writer.WriteLine($"Total time: {test.ElapsedMilliseconds} ms");
        writer.WriteLine($"Total iterations: {test.Results.Count}");
        writer.WriteLine();
        writer.WriteLine($"End iteration: {last.Iteration + 1} \t\t ||f - Ax^j|| / ||r^0||: {last.Residual}");
        writer.WriteLine();
        writer.WriteLine("Iteration\tResidual\t\tSmoothedResidual");
        writer.WriteLine("---------\t---------\t\t---------------");

        foreach (var result in test.Results.SkipLast(1))
        {
            writer.WriteLine($"{result.Iteration}\t\t{result.Residual:E15}\t{result.SmoothedResidual:E15}");
        }
        writer.WriteLine($"End iteration: {last.Iteration + 1} \t\t ||f - Ax^j|| / ||r^0||: {last.Residual}");
    }
}