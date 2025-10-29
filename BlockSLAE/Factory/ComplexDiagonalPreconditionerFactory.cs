using BlockSLAE.Preconditions;
using BlockSLAE.Storages;

namespace BlockSLAE.Factory;

public class ComplexDiagonalPreconditionerFactory
{
    public ComplexDiagonalPreconditioner CreatePreconditioner(BlockMatrix matrix)
    {
        return new ComplexDiagonalPreconditioner(matrix);
    }
}