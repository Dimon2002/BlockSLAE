using BlockSLAE.Storages;

namespace BlockSLAE.Preconditions;

public class ComplexDiagonalPreconditionerFactory
{
    public ComplexDiagonalPreconditioner CreatePreconditioner(BlockMatrix matrix)
    {
        return new ComplexDiagonalPreconditioner(matrix);
    }
}