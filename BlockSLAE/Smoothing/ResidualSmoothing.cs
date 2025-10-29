using System.Numerics;
using BlockSLAE.Storages;

namespace BlockSLAE.Smoothing;

public class ResidualSmoothing : ISmoothingStrategy
{
    public ComplexVector Solution { get; private set; } = ComplexVector.None;

    public ComplexVector Residual { get; private set; } = ComplexVector.None;

    private ComplexVector _buffer = ComplexVector.None;
    
    public void Initialize(ComplexVector startSolution, ComplexVector startResidual)
    {
        Solution = startSolution.Clone();
        Residual = startResidual.Clone();

        _buffer = startResidual.Clone();
        _buffer.Nullify();
    }

    public void Apply(ComplexVector currentSolution, ComplexVector currentResidual)
    {
        currentResidual.Subtract(Residual, _buffer);

        var numerator = Residual.ScalarProduct(_buffer).Real;
        var denominator = _buffer.ScalarProduct(_buffer).Real;
        var etta = -numerator / denominator;

        etta = etta switch
        {
            < 0 => 0,
            > 1 => 1,
            _ => etta
        };

        var complexOneMinusEtta = new Complex(1 - etta, 0);
        var complexEtta = new Complex(etta, 0);

        Solution.MultiplyOn(complexOneMinusEtta, Solution);
        Solution.Add(currentSolution.MultiplyOn(complexEtta, _buffer), Solution);

        Residual.MultiplyOn(complexOneMinusEtta, Residual);
        Residual.Add(currentResidual.MultiplyOn(complexEtta, _buffer), Residual);
    }
}