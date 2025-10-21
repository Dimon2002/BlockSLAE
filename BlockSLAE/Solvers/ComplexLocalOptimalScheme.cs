using BlockSLAE.Preconditions;
using BlockSLAE.Smoothing;
using BlockSLAE.Storages;
using BlockSLAE.Storages.Structures;
using Microsoft.Extensions.Logging;

namespace BlockSLAE.Solvers;

public class ComplexLocalOptimalScheme : Method<SLAEConfig>, ISLAESolver
{
    private readonly ComplexDiagonalPreconditionerFactory _preconditionerFactory;
    private readonly ISmoothingStrategy _smoothingStrategy;

    private ComplexDiagonalPreconditioner _preconditioner = null!;
    private ComplexEquation _equation = null!;

    private double _r0Norm;
    
    private ComplexVector _r;
    private ComplexVector _p;
    private ComplexVector _s;
    private ComplexVector _z;
    private ComplexVector _a;
    private ComplexVector _w;
    
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

    private void InitializeStartValues(ComplexEquation equation)
    {
        _preconditioner = _preconditionerFactory.CreatePreconditioner(equation.Matrix);
        _equation = equation;
        
        var dimension = _equation.RightSide.Length;
        _r = ComplexVector.Create(dimension);
        _s = ComplexVector.Create(dimension);
        _a = ComplexVector.Create(dimension);
        _w = ComplexVector.Create(dimension);
        
        _equation.RightSide.Subtract(_equation.Matrix.MultiplyOn(_equation.Solution), _r);
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

        var buffer = ComplexVector.Create(_equation.RightSide.Length);

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
            
            _smoothingStrategy.Apply(solution, _r, _equation.Matrix);
            solution.CopyFrom(_smoothingStrategy.SmoothingSolution);
            _r.CopyFrom(_smoothingStrategy.SmoothingResidual);
            
            if (i % 200 == 0)
            {
                Logger.LogInformation("[{Iteration}] {relativeDiscrepancy:E15} / {Config.Discrepancy}", i, _r.Norm / fNorm, Config.Epsilon);
                Console.WriteLine($"[{nameof(ComplexLocalOptimalScheme)}:{i}] {_r.Norm / fNorm:E15} / {Config.Epsilon:E15}");
            }
        }

        _equation.RightSide.Subtract(_equation.Matrix.MultiplyOn(solution, buffer), buffer);
        var discrepancy = buffer.Norm / _r0Norm;

        Logger.LogInformation("EndIteration {i} Discrepancy: {discrepancy:E8}", i, discrepancy);
        Console.WriteLine($"[{nameof(ComplexLocalOptimalScheme)}:{i}] Discrepancy: {discrepancy:E8}");

        return _equation.Solution;
    }
}