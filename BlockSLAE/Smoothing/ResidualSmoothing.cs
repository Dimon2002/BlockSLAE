using System.Numerics;
using BlockSLAE.Storages;

namespace BlockSLAE.Smoothing;

public class ResidualSmoothing : ISmoothingStrategy
{
    public ComplexVector Solution { get; private set; } = ComplexVector.None;

    public ComplexVector Residual { get; private set; } = ComplexVector.None;

    public void Initialize(ComplexVector startSolution, ComplexVector startResidual)
    {
        Solution = startSolution.Clone();
        Residual = startResidual.Clone();
    }

    public void Apply(ComplexVector currentSolution, ComplexVector currentResidual)
    {
        var buffer = ComplexVector.Create(currentResidual.Length);
        currentResidual.Subtract(Residual, buffer);

        var numerator = Residual.ScalarProduct(buffer).Real;
        var denominator = buffer.ScalarProduct(buffer).Real;
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
        Solution.Add(currentSolution.MultiplyOn(complexEtta, buffer), Solution);

        Residual.MultiplyOn(complexOneMinusEtta, Residual);
        Residual.Add(currentResidual.MultiplyOn(complexEtta, buffer), Residual);
    }
}