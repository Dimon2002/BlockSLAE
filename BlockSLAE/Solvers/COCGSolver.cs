using BlockSLAE.Preconditions;
using BlockSLAE.Storages;
using Microsoft.Extensions.Logging;

namespace BlockSLAE.Solvers;

public class COCGSolver : Method<COCGConfig>
{
    private readonly ComplexDiagonalPreconditionerFactory _preconditionerFactory;

    private ComplexDiagonalPreconditioner _preconditioner = null!;
    private ComplexEquation<BlockMatrix> _equation = null!;

    private ComplexVector _r;
    private ComplexVector _z;
    private ComplexVector _p;
    
    private ComplexVector _rNext;
    private ComplexVector _zNext;
    private ComplexVector _pNext;
    
    public COCGSolver(
        ComplexDiagonalPreconditionerFactory factory,
        ILogger<COCGSolver> logger,
        COCGConfig config
    ) : base(config, logger)
    {
        _preconditionerFactory = factory;
    }

    public ComplexVector Solve(ComplexEquation<BlockMatrix> equation)
    {
        InitializeStartValues(equation);

        return IterationProcess();
    }

    private void InitializeStartValues(ComplexEquation<BlockMatrix> equation)
    {
        _preconditioner = _preconditionerFactory.CreatePreconditioner(equation.Matrix);
        _equation = equation;

        _r = _equation.RightSide - _equation.Matrix.MultiplyOn(_equation.Solution);
        _z = _preconditioner.MultiplyOn(_r);
        _p = _z.Clone();
    }

    private ComplexVector IterationProcess()
    {
        var solution = _equation.Solution;
        
        for (var i = 1; i < Config.MaxIterations && _r.Norm / _equation.RightSide.Norm > Config.Epsilon; i++)
        {
            var a = _r.PseudoScalarProduct(_z) / _equation.Matrix.MultiplyOn(_p).PseudoScalarProduct(_p);
            solution += _p.MultiplyOn(a);

            _rNext = _r - _equation.Matrix.MultiplyOn(_p).MultiplyOn(a);
            _zNext = _preconditioner.MultiplyOn(_rNext);

            var b = _rNext.PseudoScalarProduct(_zNext) / _r.PseudoScalarProduct(_z);
            _pNext = _zNext + _p.MultiplyOn(b);
            
            _r =  _rNext.Clone();
            _z =  _zNext.Clone();
            _p =  _pNext.Clone();
        }
        
        return solution;
    }
}

public readonly record struct COCGConfig(int MaxIterations, double Epsilon);

public record ComplexEquation<TMatrix>(TMatrix Matrix, ComplexVector Solution, ComplexVector RightSide);