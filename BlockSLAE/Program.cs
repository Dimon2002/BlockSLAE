// See https://aka.ms/new-console-template for more information

using BlockSLAE.IO;
using BlockSLAE.Preconditions;
using BlockSLAE.Solvers;
using BlockSLAE.Storages.Structures;
using Microsoft.Extensions.Logging;

const string basePath = @"E:\Projects\BlockSLAE\Input\";
const int slaeNumber = 1;

var pathCombined = Path.Combine(basePath, slaeNumber.ToString());

var equation = ComplexEquationBuilder.Build(pathCombined, new BinaryFileHelper());
var factory = new ComplexDiagonalPreconditionerFactory();
var config = new COCGConfig(30_000, 1.000000e-005);

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<COCGSolver>();

var solver = new COCGSolver(factory, logger, config);

_ = solver.Solve(equation);
