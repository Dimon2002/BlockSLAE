using BlockSLAE.Storages;

namespace BlockSLAE.Preconditions;

public class ComplexDiagonalPreconditioner
{
    private readonly BlockMatrix _decomposedMatrix;

    public ComplexDiagonalPreconditioner(BlockMatrix matrix)
    {
        var inverseDiagonal = EvaluateInverseDiagonal(matrix);

        _decomposedMatrix = new BlockMatrix(
            inverseDiagonal.Values,
            [],
            matrix.DiagonalIndexes,
            [],
            new int[matrix.Size + 1],
            []
        );
    }

    public ComplexVector MultiplyOn(ComplexVector vector, ComplexVector? resultMemory = null)
    {
        resultMemory ??= ComplexVector.Create(vector.Length);

        return _decomposedMatrix.MultiplyOn(vector, resultMemory);
    }

    private static ComplexVector EvaluateInverseDiagonal(BlockMatrix matrix)
    {
        var n = matrix.Size;
        var resultMemory = ComplexVector.Create(matrix.Diagonal.Length);

        var offset = 0;
        for (var i = 0; i < n; ++i)
        {
            var block = matrix[i, i];
            var length = block.Length;

            var det = block[0] * block[0];
            if (block.Length == 2)
            {
                det += block[1] * block[1];
                resultMemory.Values[offset + 1] = -block[1] / det;
            }

            resultMemory.Values[offset] = block[0] / det;
            offset += length;
        }

        return resultMemory;
    }
}