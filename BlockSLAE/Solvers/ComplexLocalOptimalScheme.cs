using BlockSLAE.Factory;
using BlockSLAE.Preconditions;
using BlockSLAE.Smoothing;
using BlockSLAE.Storages;
using BlockSLAE.Storages.Structures;
using BlockSLAE.Testing.Configs;
using Microsoft.Extensions.Logging;

namespace BlockSLAE.Solvers;

[Solver("ComplexLocalOptimalScheme")]
public class ComplexLocalOptimalScheme : Method<SLAEConfig>, ISLAESolver
{
    private readonly ComplexDiagonalPreconditionerFactory _preconditionerFactory;
    private readonly ISmoothingStrategy _smoothingStrategy;

    private int _degreeOfParallelism = 1;

    private ComplexDiagonalPreconditioner _preconditioner = null!;
    private ComplexEquation _equation = null!;

    private double _r0Norm;

    private ComplexVector _r;
    private ComplexVector _p;
    private ComplexVector _s;
    private ComplexVector _z;
    private ComplexVector _a;
    private ComplexVector _w;

    public event Action<int, double, double>? IterationCompleted;

    public ComplexLocalOptimalScheme(
        ComplexDiagonalPreconditionerFactory factory,
        ISmoothingStrategy strategy,
        ILogger<ComplexLocalOptimalScheme> logger,
        SLAEConfig config
    ) : base(config, logger)
    {
        _preconditionerFactory = factory;
        _smoothingStrategy = strategy;
    }

    public ComplexVector Solve(ComplexEquation equation)
    {
        InitializeStartValues(equation);

        return IterationProcess();
    }

    public ISLAESolver SetDegreeOfParallelism(int degreeOfParallelism)
    {
        _degreeOfParallelism = degreeOfParallelism;
        return this;
    }

    private void InitializeStartValues(ComplexEquation equation)
    {
        _preconditioner = _preconditionerFactory.CreatePreconditioner(equation.Matrix);
        _preconditioner.SetDegreeOfParallelism(_degreeOfParallelism);
        
        _equation = equation;
        _equation.Matrix.SetDegreeOfParallelism(_degreeOfParallelism);
        _equation.RightSide.SetDegreeOfParallelism(_degreeOfParallelism);
        _equation.Solution.SetDegreeOfParallelism(_degreeOfParallelism);

        var dimension = _equation.RightSide.Length;
        _r = ComplexVector.Create(dimension, _degreeOfParallelism);
        _s = ComplexVector.Create(dimension, _degreeOfParallelism);
        _a = ComplexVector.Create(dimension, _degreeOfParallelism);
        _w = ComplexVector.Create(dimension, _degreeOfParallelism);

        _equation.RightSide.Subtract(
            _equation.Matrix.MultiplyOn(_equation.Solution), _r);
        _preconditioner.MultiplyOn(_r, _s);
        _p = _s.Clone();
        _equation.Matrix.MultiplyOn(_p, _a);
        _z = _a.Clone();
        _preconditioner.MultiplyOn(_z, _w);

        _r0Norm = _r.Norm;

        _smoothingStrategy.Initialize(_equation.Solution, _r);
    }

    private ComplexVector IterationProcess()
    {
        var solution = _equation.Solution;
        var fNorm = _equation.RightSide.Norm;

        var buffer = ComplexVector.Create(_equation.RightSide.Length, _degreeOfParallelism);

        Logger.LogInformation("{SolverName} started...\n\t\t\t\t\tOriginal \t  | \tSmoothed",
            nameof(ComplexLocalOptimalScheme));
        Console.WriteLine($"{nameof(ComplexLocalOptimalScheme)} started...\n\t\t\t\t\tOriginal      | \t\tSmoothed");

        var i = 1;
        for (; i < Config.MaxIterations && _r.Norm / fNorm >= Config.Epsilon; i++)
        {
            var alpha = _w.PseudoScalarProduct(_r) / _w.PseudoScalarProduct(_z);
            solution.Add(_p.MultiplyOn(alpha, buffer), solution);

            _r.Subtract(_z.MultiplyOn(alpha, buffer), _r);
            _s.Subtract(_w.MultiplyOn(alpha, buffer), _s);
            _equation.Matrix.MultiplyOn(_s, _a);

            var betta = -_w.PseudoScalarProduct(_a) / _w.PseudoScalarProduct(_z);

            _s.Add(_p.MultiplyOn(betta, buffer), _p);
            _a.Add(_z.MultiplyOn(betta, buffer), _z);

            _preconditioner.MultiplyOn(_z, _w);

            _smoothingStrategy.Apply(solution, _r);
            var relativeNorm = _r.Norm / fNorm;
            var smoothedRelativeNorm = _smoothingStrategy.Residual.Norm / fNorm;

            IterationCompleted?.Invoke(i, relativeNorm, smoothedRelativeNorm);
            if (_smoothingStrategy.Residual.Norm / fNorm < Config.Epsilon)
            {
                solution.CopyFrom(_smoothingStrategy.Solution);
                break;
            }

            if (i % 50 == 0)
            {
                relativeNorm = _r.Norm / fNorm;
                smoothedRelativeNorm = _smoothingStrategy.Residual.Norm / fNorm;

                Logger.LogInformation(
                    "[{Iteration}]  {original:E15} | {smoothed:E15} / {Discrepancy:E15}",
                    i, relativeNorm, smoothedRelativeNorm, Config.Epsilon);
                Console.WriteLine(
                    $"[{nameof(ComplexLocalOptimalScheme)}:{i}] {relativeNorm:E15} | {smoothedRelativeNorm:E15} / {Config.Epsilon:E15}");
            }
        }

        _equation.RightSide.Subtract(_equation.Matrix.MultiplyOn(solution, buffer), buffer);
        var discrepancy = buffer.Norm / _r0Norm;

        IterationCompleted?.Invoke(i, discrepancy, 0);
        Logger.LogInformation("{Solver} finished. End Iteration {i} Discrepancy: {discrepancy:E8}",
            nameof(ComplexLocalOptimalScheme), i, discrepancy);
        Console.WriteLine($"[{nameof(ComplexLocalOptimalScheme)}:{i}] Discrepancy: {discrepancy:E8}");

        return _equation.Solution;
    }
}