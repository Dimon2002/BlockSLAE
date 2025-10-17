using BlockSLAE;
using BlockSLAE.Storages;
using NUnit.Framework;

namespace BlockSLAE.Tests;

public class MatrixMultiplyTest
{
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void DefaultMultiplyOnVector1ShouldBeCorrect()
    {
        double[] di =  [1, 2, 3, 4, 5];
        double[] gg = [1, 7, 8];
        int[] idi = [0, 2, 4, 5];
        int[] ig = [0, 0, 0, 2];
        int[] jg = [0, 1];
        int[] ijg = [0, 1, 3];
        
        var matrix = new BlockMatrix(di, gg, idi, ijg, ig, jg);
        var complexVector = new ComplexVector([1, 1, 1, 1, 1, 1]);
        
        var result = matrix.MultiplyOn(complexVector).Values;
        
        var expectedVector =  new ComplexVector([0, 4, -2, 22, 5, 21]).Values;
        
        Assert.That(result, Is.EqualTo(expectedVector));
    }
    
    [Test]
    public void DiagonalMatrixMultiplyOnVector1ShouldBeCorrect()
    {
        double[] di =  [1, 2, 3, 4, 5];
        double[] gg = [];
        int[] idi = [0, 2, 4, 5];
        int[] ig = [0, 0, 0, 0];
        int[] jg = [];
        int[] ijg = [];
        
        var matrix = new BlockMatrix(di, gg, idi, ijg, ig, jg);
        var complexVector = new ComplexVector([1, 1, 1, 1, 1, 1]);
        
        var result = matrix.MultiplyOn(complexVector).Values;
        
        var expectedVector =  new ComplexVector([-1, 3, -1, 7, 5, 5]).Values;
        
        Assert.That(result, Is.EqualTo(expectedVector));
    }
}