using BlockSLAE.Preconditions;
using BlockSLAE.Smoothing;
using BlockSLAE.Storages;
using BlockSLAE.Storages.Structures;
using Microsoft.Extensions.Logging;

namespace BlockSLAE.Solvers;

public class COCGSolver : Method<SLAEConfig>, ISLAESolver
{
    private readonly ComplexDiagonalPreconditionerFactory _preconditionerFactory;
    private readonly ISmoothingStrategy _smoothingStrategy;

    private int _degreeOfParallelism = 1;
    
    private ComplexDiagonalPreconditioner _preconditioner = null!;
    private ComplexEquation _equation = null!;

    private double _r0Norm;

    private ComplexVector _r;
    private ComplexVector _z;
    private ComplexVector _p;

    private ComplexVector _rNext;
    private ComplexVector _zNext;
    private ComplexVector _pNext;

    public COCGSolver(
        ComplexDiagonalPreconditionerFactory factory,
        ISmoothingStrategy strategy,
        ILogger<COCGSolver> logger,
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
        _z = ComplexVector.Create(dimension, _degreeOfParallelism);

        _rNext = ComplexVector.Create(dimension, _degreeOfParallelism);
        _zNext = ComplexVector.Create(dimension, _degreeOfParallelism);
        _pNext = ComplexVector.Create(dimension, _degreeOfParallelism);

        _equation.RightSide.Subtract(_equation.Matrix.MultiplyOn(_equation.Solution), _r);
        _preconditioner.MultiplyOn(_r, _z);
        _p = _z.Clone();

        _r0Norm = _r.Norm;

        _smoothingStrategy.Initialize(_equation.Solution, _r);
    }

    private ComplexVector IterationProcess()
    {
        var solution = _equation.Solution;
        var fNorm = _equation.RightSide.Norm;

        var matrixByP = ComplexVector.Create(_equation.RightSide.Length, _degreeOfParallelism);
        var buffer = ComplexVector.Create(_equation.RightSide.Length, _degreeOfParallelism);

        Logger.LogInformation("{SolverName} started...\n\t\t\t\t\tOriginal  \t  |   \tSmoothed", nameof(COCGSolver));
        Console.WriteLine($"{nameof(COCGSolver)} started...\n\t\t\tOriginal       |   \tSmoothed");
        
        var i = 1;
        for (; i < Config.MaxIterations && _r.Norm / fNorm >= Config.Epsilon; i++)
        {
            _equation.Matrix.MultiplyOn(_p, matrixByP);

            var a = _r.PseudoScalarProduct(_z) / matrixByP.PseudoScalarProduct(_p);
            solution.Add(_p.MultiplyOn(a, buffer), solution);

            _r.Subtract(matrixByP.MultiplyOn(a, buffer), _rNext);
            _preconditioner.MultiplyOn(_rNext, _zNext);

            var b = _rNext.PseudoScalarProduct(_zNext) / _r.PseudoScalarProduct(_z);
            _zNext.Add(_p.MultiplyOn(b, buffer), _pNext);

            _smoothingStrategy.Apply(solution, _rNext);
            if (_smoothingStrategy.Residual.Norm / fNorm < Config.Epsilon)
            {
                solution.CopyFrom(_smoothingStrategy.Solution);
                _rNext.CopyFrom(_smoothingStrategy.Residual);

                break;
            }

            (_r, _rNext) = (_rNext, _r);
            (_z, _zNext) = (_zNext, _z);
            (_p, _pNext) = (_pNext, _p);

            if (i % 50 == 0)
            {
                var relativeNorm = _r.Norm / fNorm;
                var smoothedRelativeNorm = _smoothingStrategy.Residual.Norm / fNorm;
                
                Logger.LogInformation(
                    "[{Iteration}]  {original:E15} | {smoothed:E15} / {Discrepancy:E15}",
                    i, relativeNorm, smoothedRelativeNorm, Config.Epsilon);
                Console.WriteLine(
                    $"[{nameof(COCGSolver)}:{i}] {relativeNorm:E15} | {smoothedRelativeNorm:E15} / {Config.Epsilon:E15}");
            }
        }

        _equation.RightSide.Subtract(_equation.Matrix.MultiplyOn(solution, buffer), buffer);
        var discrepancy = buffer.Norm / _r0Norm;

        Logger.LogInformation("{Solver} finished. End Iteration {i} Discrepancy: {discrepancy:E8}", nameof(COCGSolver),
            i, discrepancy);
        Console.WriteLine($"[{nameof(COCGSolver)}:{i}] Discrepancy: {discrepancy:E8}");

        return _equation.Solution;
    }
}