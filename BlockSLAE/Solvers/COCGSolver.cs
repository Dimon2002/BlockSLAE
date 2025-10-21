using BlockSLAE.Preconditions;
using BlockSLAE.Storages;
using BlockSLAE.Storages.Structures;
using Microsoft.Extensions.Logging;

namespace BlockSLAE.Solvers;

public class COCGSolver : Method<SLAEConfig>
{
    private readonly ComplexDiagonalPreconditionerFactory _preconditionerFactory;

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
        ILogger<COCGSolver> logger,
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
        _z = _preconditioner.MultiplyOn(_r);
        _p = _z.Clone();
        
        _r0Norm = _r.Norm;
    }

    private ComplexVector IterationProcess()
    {
        var solution = _equation.Solution;
        var fNorm = _equation.RightSide.Norm;

        var i = 1;
        for (; i < Config.MaxIterations && _r.Norm / fNorm >= Config.Epsilon; i++)
        {
            var matrixByP = _equation.Matrix.MultiplyOn(_p);

            var a = _r.PseudoScalarProduct(_z) / matrixByP.PseudoScalarProduct(_p);
            solution += _p.MultiplyOn(a);

            _rNext = _r - matrixByP.MultiplyOn(a);
            _zNext = _preconditioner.MultiplyOn(_rNext);

            var b = _rNext.PseudoScalarProduct(_zNext) / _r.PseudoScalarProduct(_z);
            _pNext = _zNext + _p.MultiplyOn(b);

            _r = _rNext.Clone();
            _z = _zNext.Clone();
            _p = _pNext.Clone();

            if (i % 200 == 0)
            {
                Logger.LogInformation("[{Iteration}] {relativeDiscrepancy:E15} / {Config.Discrepancy:E15}", i, _r.Norm / fNorm, Config.Epsilon);
                Console.WriteLine($"[{nameof(COCGSolver)}:{i}] {_r.Norm / fNorm:E15} / {Config.Epsilon:E15}");
            }
        }

        var discrepancy = (_equation.RightSide - _equation.Matrix.MultiplyOn(solution)).Norm / _r0Norm;
        
        Logger.LogInformation("EndIteration {i} Discrepancy: {discrepancy:E8}", i, discrepancy);
        Console.WriteLine($"[{nameof(COCGSolver)}:{i}] Discrepancy: {discrepancy:E8}");

        return solution;
    }
}