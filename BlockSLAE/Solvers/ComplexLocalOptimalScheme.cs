using BlockSLAE.Preconditions;
using BlockSLAE.Storages;
using BlockSLAE.Storages.Structures;
using Microsoft.Extensions.Logging;

namespace BlockSLAE.Solvers;

public class ComplexLocalOptimalScheme : Method<SLAEConfig>
{
    private readonly ComplexDiagonalPreconditionerFactory _preconditionerFactory;

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
        ILogger<ComplexLocalOptimalScheme> logger,
        SLAEConfig config
    ) : base(config, logger)
    {
        _preconditionerFactory = factory;
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
        
        _r = _equation.RightSide - _equation.Matrix.MultiplyOn(_equation.Solution);
        _s = _preconditioner.MultiplyOn(_r);
        _p = _s.Clone();
        _a = _equation.Matrix.MultiplyOn(_p);
        _z = _a.Clone();
        _w = _preconditioner.MultiplyOn(_z);
        
        _r0Norm = _r.Norm;
    }

    private ComplexVector IterationProcess()
    {
        var solution = _equation.Solution;
        var fNorm = _equation.RightSide.Norm;

        var i = 1;
        for (; i < Config.MaxIterations && _r.Norm / fNorm >= Config.Epsilon; i++)
        {
            var alpha = _w.PseudoScalarProduct(_r) / _w.PseudoScalarProduct(_z);
            
            solution += _p.MultiplyOn(alpha);
            _r -= _z.MultiplyOn(alpha);
            _s -= _w.MultiplyOn(alpha);
            _a = _equation.Matrix.MultiplyOn(_s);

            var betta = -_w.PseudoScalarProduct(_a) / _w.PseudoScalarProduct(_z);
            
            _p = _s + _p.MultiplyOn(betta);
            _z = _a + _z.MultiplyOn(betta);
            _w = _preconditioner.MultiplyOn(_z);
            
            if (i % 200 == 0)
            {
                Logger.LogInformation("[{Iteration}] {relativeDiscrepancy:E15} / {Config.Discrepancy}", i, _r.Norm / fNorm, Config.Epsilon);
                Console.WriteLine($"[{nameof(ComplexLocalOptimalScheme)}:{i}] {_r.Norm / fNorm:E15} / {Config.Epsilon:E15}");
            }
        }
        
        var discrepancy = (_equation.RightSide - _equation.Matrix.MultiplyOn(solution)).Norm / _r0Norm;

        Logger.LogInformation("EndIteration {i} Discrepancy: {discrepancy:E8}", i, discrepancy);
        Console.WriteLine($"[{nameof(ComplexLocalOptimalScheme)}:{i}] Discrepancy: {discrepancy:E8}");

        return solution;
    }
}