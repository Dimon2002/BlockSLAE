using BlockSLAE.Storages;

namespace BlockSLAE.Smoothing;

public interface ISmoothingStrategy
{
    ComplexVector Solution { get; }
    ComplexVector Residual { get; }

    void Initialize(ComplexVector startSolution, ComplexVector startResidual);

    void Apply(ComplexVector currentSolution, ComplexVector currentResidual);
}