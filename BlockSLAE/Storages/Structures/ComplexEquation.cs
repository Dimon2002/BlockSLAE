namespace BlockSLAE.Storages.Structures;

public record ComplexEquation(BlockMatrix Matrix, ComplexVector Solution, ComplexVector RightSide)
{
    public static ComplexEquation None => new ComplexEquation(BlockMatrix.None, ComplexVector.None, ComplexVector.None);
}