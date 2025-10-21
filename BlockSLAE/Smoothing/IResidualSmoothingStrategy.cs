using BlockSLAE.Storages;

namespace BlockSLAE.Smoothing;

public interface ISmoothingStrategy
{
    void Initialize(ComplexVector startSolution, ComplexVector startResidual);
    void Apply(ComplexVector currentSolution, ComplexVector currentResidual, BlockMatrix matrix);
    
    ComplexVector SmoothingSolution { get; }
    ComplexVector SmoothingResidual { get; }
}