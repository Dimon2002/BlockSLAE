using System.Numerics;
using BlockSLAE.Storages;

namespace BlockSLAE.Smoothing;

public class ResidualSmoothing : ISmoothingStrategy
{
    private ComplexVector _y = ComplexVector.None;
    private ComplexVector _s = ComplexVector.None;

    public void Initialize(ComplexVector startSolution, ComplexVector startResidual)
    {
        _y = startSolution.Clone();
        _s = startResidual.Clone();
    }

    public void Apply(ComplexVector currentSolution, ComplexVector currentResidual, BlockMatrix matrix)
    {
        var buffer = ComplexVector.Create(currentResidual.Length);
        currentResidual.Subtract(_s, buffer);

        var numerator = _s.PseudoScalarProduct(buffer).Real;
        var denominator = buffer.PseudoScalarProduct(buffer).Real;
        var etta = -numerator / denominator;

        etta = etta switch
        {
            < 0 => 0,
            > 1 => 1,
            _ => etta
        };

        var complexOneMinusEtta = new Complex(1 - etta, 0);
        var complexEtta = new Complex(etta, 0);

        _y.MultiplyOn(complexOneMinusEtta, _y);
        _y.Add(currentSolution.MultiplyOn(complexEtta, buffer), _y);

        _s.MultiplyOn(complexOneMinusEtta, _s);
        _s.Add(currentResidual.MultiplyOn(complexEtta, buffer), _s);
    }

    public ComplexVector SmoothingSolution => _y;

    public ComplexVector SmoothingResidual => _s;
}