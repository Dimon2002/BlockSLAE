using BlockSLAE.Storages;

namespace BlockSLAE.Smoothing;

public class LackSmoothing : ISmoothingStrategy
{
    public ComplexVector Solution { get; private set; } = ComplexVector.None;

    public ComplexVector Residual { get; private set; } = ComplexVector.None;

    public void Initialize(ComplexVector startSolution, ComplexVector startResidual)
    {
        Solution = startSolution;
        Residual = startResidual;
    }

    public void Apply(ComplexVector currentSolution, ComplexVector currentResidual)
    {
    }
}