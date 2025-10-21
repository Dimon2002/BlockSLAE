using BlockSLAE.Storages;

namespace BlockSLAE.Smoothing;

public class ResidualSmoothing : ISmoothingStrategy
{
    public void Initialize(ComplexVector startSolution, ComplexVector startResidual)
    {
        throw new NotImplementedException();
    }

    public void Apply(ComplexVector currentSolution, ComplexVector currentResidual, BlockMatrix matrix)
    {
        throw new NotImplementedException();
    }

    public ComplexVector SmoothingSolution { get; }
    public ComplexVector SmoothingResidual { get; }
}