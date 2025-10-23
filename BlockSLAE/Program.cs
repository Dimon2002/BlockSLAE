// See https://aka.ms/new-console-template for more information

using BlockSLAE.IO;
using BlockSLAE.Preconditions;
using BlockSLAE.Smoothing;
using BlockSLAE.Solvers;
using Microsoft.Extensions.Logging;

const string basePath = @"E:\Projects\BlockSLAE\Input\";
const int slaeNumber = 1;

var pathCombined = Path.Combine(basePath, slaeNumber.ToString());

var equation = ComplexEquationBuilder.BuildEquation(pathCombined, new BinaryFileHelper());
var config = ComplexEquationBuilder.ReadSLAEConfig(pathCombined);

var logger = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
}).CreateLogger<COCGSolver>();

ISmoothingStrategy strategy = new ResidualSmoothing();

var solver = new COCGSolver(new ComplexDiagonalPreconditionerFactory(), strategy, logger, config);

_ = solver.Solve(equation);
