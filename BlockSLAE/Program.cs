// See https://aka.ms/new-console-template for more information

using BlockSLAE.IO;
using BlockSLAE.Preconditions;
using BlockSLAE.Smoothing;
using BlockSLAE.Solvers;
using Microsoft.Extensions.Logging;

const string basePath = @"F:\projects\BlockSLAU\";
const string input = @"Input\";
const int slaeNumber = 1;

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
}).CreateLogger<ComplexLocalOptimalScheme>();

var solver = new ComplexLocalOptimalScheme(new ComplexDiagonalPreconditionerFactory(), new ResidualSmoothing(), logger, config);

_ = solver.Solve(equation);
