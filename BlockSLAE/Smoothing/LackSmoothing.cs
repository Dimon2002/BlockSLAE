using BlockSLAE.Storages;

namespace BlockSLAE.Smoothing;

public class LackSmoothing : ISmoothingStrategy
{
    private ComplexVector _x = ComplexVector.None;
    private ComplexVector _r = ComplexVector.None;

    public void Initialize(ComplexVector startSolution, ComplexVector startResidual)
    {
        _x = startSolution;
        _r = startResidual;
    }

    public void Apply(ComplexVector currentSolution, ComplexVector currentResidual, BlockMatrix matrix)
    {
        _x = currentSolution;
        _r = currentResidual;
    }

    public ComplexVector SmoothingSolution => _x;
    
    public ComplexVector SmoothingResidual => _r;
}