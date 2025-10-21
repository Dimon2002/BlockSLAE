// See https://aka.ms/new-console-template for more information

using BlockSLAE.IO;
using BlockSLAE.Preconditions;
using BlockSLAE.Solvers;
using BlockSLAE.Storages;
using BlockSLAE.Storages.Structures;
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
}).CreateLogger<ComplexLocalOptimalScheme>();

var solver = new ComplexLocalOptimalScheme(new ComplexDiagonalPreconditionerFactory(), logger, config);

_ = solver.Solve(equation);
