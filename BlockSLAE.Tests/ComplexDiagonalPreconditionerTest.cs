using BlockSLAE.Factory;
using BlockSLAE.Preconditions;
using BlockSLAE.Storages;
using NUnit.Framework;

namespace BlockSLAE.Tests;

public class ComplexDiagonalPreconditionerTest
{
    private ComplexDiagonalPreconditioner _preconditioner = null!;

    [SetUp]
    public void Setup()
    {
        double[] di = [1, 2, 3, 5, 4];
        double[] gg = [];
        int[] ig = [0, 0, 0, 0];
        int[] jg = [];
        int[] idi = [0, 2, 3, 5];
        int[] ijg = [];

        var matrix = new BlockMatrix(di, gg, idi, ijg, ig, jg);
        var factory = new ComplexDiagonalPreconditionerFactory();
        _preconditioner = factory.CreatePreconditioner(matrix);
    }

    [Test]
    public void PreconditionerMultiplyOperationOnShouldBeCorrect()
    {
        var vector = new ComplexVector([1, 2, 3, 4, 41, 82]);
        var result = _preconditioner.MultiplyOn(vector).Values;

        var expected = new[] { 1, 0, 1, 4d / 3d, 13, 6 };
        
        Assert.That(result, Is.EqualTo(expected).Within(1e-10));
    }
}