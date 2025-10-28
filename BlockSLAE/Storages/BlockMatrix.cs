namespace BlockSLAE.Storages;

public class BlockMatrix
{
    public int[] DiagonalIndexes { get; }

    public int[] RowIndex { get; }

    public int[] OffDiagonalIndexes { get; }

    public int[] ColumnIndex { get; }

    public double[] Diagonal { get; }

    public double[] Values { get; }

    public int Size => DiagonalIndexes.Length - 1;

    public ReadOnlySpan<double> this[int i, int j] => GetBlockData(i, j);

    public static BlockMatrix None => new BlockMatrix([], [], [], [], [], []);

    private int _degreeOfParallelism;
    
    public BlockMatrix(
        IEnumerable<double> di,
        IEnumerable<double> gg,
        IEnumerable<int> idi,
        IEnumerable<int> ijg,
        IEnumerable<int> ig,
        IEnumerable<int> jg,
        int degreeOfParallelism = 1)
    {
        Diagonal = di.ToArray();
        Values = gg.ToArray();
        DiagonalIndexes = idi.ToArray();
        OffDiagonalIndexes = ijg.ToArray();
        RowIndex = ig.ToArray();
        ColumnIndex = jg.ToArray();
        
        _degreeOfParallelism = degreeOfParallelism;
    }

    public BlockMatrix SetDegreeOfParallelism(int degreeOfParallelism)
    {
        _degreeOfParallelism =  degreeOfParallelism;
        return this;
    }
    
    public ComplexVector MultiplyOn(ComplexVector vector, ComplexVector? resultMemory = null)
    {
        resultMemory ??= ComplexVector.Create(vector.Length);
        resultMemory.Nullify();

        return BlockMatrixMultiply(vector, resultMemory);
    }

    public BlockMatrix Clone()
    {
        return new BlockMatrix(Diagonal, Values, DiagonalIndexes, OffDiagonalIndexes, RowIndex, ColumnIndex, _degreeOfParallelism);
    }

    private ComplexVector BlockMatrixMultiply(ComplexVector vector, ComplexVector resultMemory)
    {
        if (Size == -1)
        {
            return ComplexVector.None;
        }

        var x = vector.Clone().Values;
        var y = resultMemory.Values;

        var vectorLength = y.Length;
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _degreeOfParallelism
        };

        var threadLocalResults = new ThreadLocal<double[]>(() => new double[vectorLength], trackAllValues: true);

        Parallel.For(0, Size, parallelOptions, i =>
        {
            var yLocal = threadLocalResults.Value;
            var diagBlock = GetBlockData(i, i);
            var xBlock = x.AsSpan(i * 2, 2);
            var yBlock = yLocal.AsSpan(i * 2, 2);

            BlockMultiply(diagBlock, xBlock, yBlock);

            for (var j = RowIndex[i]; j < RowIndex[i + 1]; ++j)
            {
                var k = ColumnIndex[j];
                var offDiagBlock = GetBlockData(i, j);
                var xk = x.AsSpan(k * 2, 2);
                var yk = yLocal.AsSpan(k * 2, 2);

                BlockMultiply(offDiagBlock, xk, yBlock);
                BlockMultiply(offDiagBlock, xBlock, yk);
            }
        });

        foreach (var local in threadLocalResults.Values)
        {
            for (var idx = 0; idx < vectorLength; idx++)
                y[idx] += local[idx];
        }

        threadLocalResults.Dispose();

        return resultMemory;
    }

    private void BlockMultiply(ReadOnlySpan<double> a, ReadOnlySpan<double> x, Span<double> resultMemory)
    {
        resultMemory[0] += a[0] * x[0];
        resultMemory[1] += a[0] * x[1];

        if (a.Length == 2)
        {
            resultMemory[0] -= a[1] * x[1];
            resultMemory[1] += a[1] * x[0];
        }
    }

    private ReadOnlySpan<double> GetBlockData(int i, int j)
    {
        int currentBlockIndex;
        int length;

        if (i == j)
        {
            currentBlockIndex = DiagonalIndexes[i];
            length = GetDiagonalBlockSize(i);

            return Diagonal.AsSpan(currentBlockIndex, length);
        }

        currentBlockIndex = OffDiagonalIndexes[j];
        length = GetOffDiagonalBlockSize(j);

        return Values.AsSpan(currentBlockIndex, length);
    }

    private int GetDiagonalBlockSize(in int offset)
    {
        return DiagonalIndexes[offset + 1] - DiagonalIndexes[offset];
    }

    private int GetOffDiagonalBlockSize(in int offset)
    {
        return OffDiagonalIndexes[offset + 1] - OffDiagonalIndexes[offset];
    }
}