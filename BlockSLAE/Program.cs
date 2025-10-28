// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using BlockSLAE.IO;
using BlockSLAE.Preconditions;
using BlockSLAE.Smoothing;
using BlockSLAE.Solvers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

const string basePath = @"F:\projects\BlockSLAU\";
const string input = @"Input\";
const int slaeNumber = 1;
int[] degreeOfParallelism = [1, 2, 4, Environment.ProcessorCount];

var pathCombined = Path.Combine(basePath, input, slaeNumber.ToString());

var equation = SLAEReader.BuildEquation(pathCombined, new BinaryFileHelper());
var config = SLAEReader.ReadSLAEConfig(pathCombined);

var logger = LoggerFactory.Create(builder =>
{
    builder
        //.AddConsole()
        .AddFile<BasicFormatter>(
            configuration =>
            {
                configuration.Directory = basePath;
                configuration.FileNamePrefix = $"slae_{slaeNumber}";
            },
            formatter =>
            {
                formatter.IncludePID = false;
                formatter.IncludeUser = false;
                formatter.CaptureScopes = false;
            })
        .SetMinimumLevel(LogLevel.Information);
}).CreateLogger<COCGSolver>();

var solver = new COCGSolver(new ComplexDiagonalPreconditionerFactory(), new ResidualSmoothing(), NullLogger<COCGSolver>.Instance, config);
solver.SetDegreeOfParallelism(degreeOfParallelism[3]);

var stopwatch = Stopwatch.StartNew();

_ = solver.Solve(equation);

stopwatch.Stop();

Console.WriteLine($"Время решения: {stopwatch.ElapsedMilliseconds} мс");

void LinesInfo()
{
    var n = equation.Matrix.Size;
    var rowWeights = new int[n];
    for (var i = 0; i < equation.Matrix.Size; i++)
    {
        rowWeights[i] = equation.Matrix.RowIndex[i + 1] - equation.Matrix.RowIndex[i];
    }

    var groupKeyValueCountPair = new Dictionary<int, int>();
    foreach (var groupElem in rowWeights.GroupBy(x => x))
    {
        var key = groupElem.Key;
        var count = groupElem.Count();
    
        groupKeyValueCountPair.Add(key, count);
    }
}