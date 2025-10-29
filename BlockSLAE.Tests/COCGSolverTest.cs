using BlockSLAE.Factory;
using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using BlockSLAE.Storages;
using BlockSLAE.Solvers;
using BlockSLAE.Smoothing;
using BlockSLAE.Storages.Structures;

namespace BlockSLAE.Tests;

[TestFixture]
public class COCGSolverTests
{
    private const double Tolerance = 1e-10;
    private ComplexDiagonalPreconditionerFactory _preconditionerFactory = null!;
    private ISmoothingStrategy _strategy = null!;
    private SLAEConfig _config;

    [SetUp]
    public void Setup()
    {
        _preconditionerFactory = new ComplexDiagonalPreconditionerFactory();
        _config = new SLAEConfig
        {
            MaxIterations = 1_000,
            Epsilon = 1e-10
        };
    }

    [Test(Description = """
                        | (1, 2) (8, 4)  (0, 0)  |       | (1, 1) |
                        | (8, 4) (3, 0)  (1, -4) | * x = | (1, 1) |
                        | (0, 0) (1, -4) (5, 4)  |       | (1, 1) |
                        """)]
    public void DefaultSlaeLackSmoothingShouldBeCorrect()
    {
        double[] di = [1, 2, 3, 5, 4];
        double[] gg = [8, 4, 1, -4];
        int[] ig = [0, 0, 1, 2];
        int[] jg = [0, 1];
        int[] idi = [0, 2, 3, 5];
        int[] ijg = [0, 2, 4];
        double[] bVector = [1, 1, 1, 1, 1, 1];

        var matrix = new BlockMatrix(di, gg, idi, ijg, ig, jg);
        var right = new ComplexVector(bVector);
        var initialSolution = ComplexVector.Create(bVector.Length);

        var equation = new ComplexEquation(matrix, initialSolution, right);

        _strategy = new LackSmoothing();

        var solver = new COCGSolver(
            _preconditionerFactory,
            _strategy,
            new NullLogger<COCGSolver>(),
            _config);

        var result = solver.Solve(equation).Values;

        double[] expected =
        [
            75.0 / 1037.0,
            215.0 / 1037.0,
            864.0 / 5185.0,
            -12.0 / 5185.0,
            81.0 / 305.0,
            37.0 / 305.0
        ];

        Assert.That(result, Is.EqualTo(expected).Within(Tolerance));
    }

    [Test(Description = """
                        | (1, 2) (0, 0) (0, 0) |       | (1, 2)   |
                        | (0, 0) (3, 0) (0, 0) | * x = | (3, 3)   |
                        | (0, 0) (0, 0) (5, 4) |       | (41, 82) |
                        """)]
    public void DiagonalSlaeLackSmoothingShouldBeCorrect()
    {
        double[] di = [1, 2, 3, 5, 4];
        double[] gg = [];
        int[] ig = [0, 0, 0, 0];
        int[] jg = [];
        int[] idi = [0, 2, 3, 5];
        int[] ijg = [];
        double[] bVector = [1, 2, 3, 3, 41, 82];

        var matrix = new BlockMatrix(di, gg, idi, ijg, ig, jg);
        var right = new ComplexVector(bVector);
        var initialSolution = ComplexVector.Create(bVector.Length);

        var equation = new ComplexEquation(matrix, initialSolution, right);

        _strategy = new LackSmoothing();

        var solver = new COCGSolver(
            _preconditionerFactory,
            _strategy,
            new NullLogger<COCGSolver>(),
            _config);

        var result = solver.Solve(equation).Values;

        double[] expected =
        [
            1, 0, 1, 1, 13, 6
        ];

        Assert.That(result, Is.EqualTo(expected).Within(Tolerance));
    }

    [Test(Description = """
                        | (1, 2) (8, 4)  (0, 0)  |       | (1, 1) |
                        | (8, 4) (3, 0)  (1, -4) | * x = | (1, 1) |
                        | (0, 0) (1, -4) (5, 4)  |       | (1, 1) |
                        """)]
    public void DefaultSlaeResidualSmoothingShouldBeCorrect()
    {
        double[] di = [1, 2, 3, 5, 4];
        double[] gg = [8, 4, 1, -4];
        int[] ig = [0, 0, 1, 2];
        int[] jg = [0, 1];
        int[] idi = [0, 2, 3, 5];
        int[] ijg = [0, 2, 4];
        double[] bVector = [1, 1, 1, 1, 1, 1];

        var matrix = new BlockMatrix(di, gg, idi, ijg, ig, jg);
        var right = new ComplexVector(bVector);
        var initialSolution = ComplexVector.Create(bVector.Length);

        var equation = new ComplexEquation(matrix, initialSolution, right);

        _strategy = new ResidualSmoothing();

        var solver = new COCGSolver(
            _preconditionerFactory,
            _strategy,
            new NullLogger<COCGSolver>(),
            _config);

        var result = solver.Solve(equation).Values;

        double[] expected =
        [
            75.0 / 1037.0,
            215.0 / 1037.0,
            864.0 / 5185.0,
            -12.0 / 5185.0,
            81.0 / 305.0,
            37.0 / 305.0
        ];

        Assert.That(result, Is.EqualTo(expected).Within(Tolerance));
    }

    [Test(Description = """
                        | (1, 2) (0, 0) (0, 0) |       | (1, 2)   |
                        | (0, 0) (3, 0) (0, 0) | * x = | (3, 3)   |
                        | (0, 0) (0, 0) (5, 4) |       | (41, 82) |
                        """)]
    public void DiagonalSlaeResidualSmoothingShouldBeCorrect()
    {
        double[] di = [1, 2, 3, 5, 4];
        double[] gg = [];
        int[] ig = [0, 0, 0, 0];
        int[] jg = [];
        int[] idi = [0, 2, 3, 5];
        int[] ijg = [];
        double[] bVector = [1, 2, 3, 3, 41, 82];

        var matrix = new BlockMatrix(di, gg, idi, ijg, ig, jg);
        var right = new ComplexVector(bVector);
        var initialSolution = ComplexVector.Create(bVector.Length);

        var equation = new ComplexEquation(matrix, initialSolution, right);

        _strategy = new ResidualSmoothing();

        var solver = new COCGSolver(
            _preconditionerFactory,
            _strategy,
            new NullLogger<COCGSolver>(),
            _config);

        var result = solver.Solve(equation).Values;

        double[] expected =
        [
            1, 0, 1, 1, 13, 6
        ];

        Assert.That(result, Is.EqualTo(expected).Within(Tolerance));
    }
}