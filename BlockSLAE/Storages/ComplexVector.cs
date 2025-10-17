using System.Numerics;

namespace BlockSLAE.Storages;

public class ComplexVector
{
    public double[] Values { get; }
    public int Length => Values.Length;

    public double Norm => double.Sqrt(ScalarProduct(this, this).Real);

    public ComplexVector(IEnumerable<double> values)
    {
        Values = values.ToArray();
    }

    public static ComplexVector Create(int length)
    {
        return new ComplexVector(new double[length]);
    }

    public static ComplexVector Create(IEnumerable<double> values)
    {
        return new ComplexVector(values);
    }

    public ComplexVector Clone()
    {
        return new ComplexVector(Values);
    }

    public static ComplexVector operator +(ComplexVector vectorLeft, ComplexVector vectorRight)
    {
        if (vectorLeft.Length != vectorRight.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        var resultMemory = Create(vectorLeft.Length);

        for (var i = 0; i < vectorLeft.Length; i++)
        {
            resultMemory.Values[i] = vectorLeft.Values[i] + vectorRight.Values[i];
        }

        return resultMemory;
    }

    public static ComplexVector operator -(ComplexVector vectorLeft, ComplexVector vectorRight)
    {
        if (vectorLeft.Length != vectorRight.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        var resultMemory = Create(vectorLeft.Length);

        for (var i = 0; i < vectorLeft.Length; i++)
        {
            resultMemory.Values[i] = vectorLeft.Values[i] - vectorRight.Values[i];
        }

        return resultMemory;
    }

    public ComplexVector MultiplyOn(Complex complexScalar)
    {
        var resultMemory = Clone();

        for (var i = 0; i < Length / 2; i++)
        {
            var z = new Complex(Values[i * 2], Values[i * 2 + 1]);
            var aByZ = complexScalar * z;

            resultMemory.Values[i * 2] = aByZ.Real;
            resultMemory.Values[i * 2 + 1] = aByZ.Imaginary;
        }
        
        return resultMemory;
    }

    public Complex ScalarProduct(ComplexVector outerVector)
    {
        return ScalarProduct(this, outerVector);
    }

    public Complex PseudoScalarProduct(ComplexVector outerVector)
    {
        return ScalarProduct(this, outerVector);
    }
    
    private static Complex ScalarProduct(ComplexVector a, ComplexVector b) // 2
    {
        if (a.Length % 2 != 0 || b.Length % 2 != 0)
        {
            throw new ArgumentException("Vectors must have an even length");
        }

        if (a.Length != b.Length)
        {
            throw new ArgumentException("Vector and result must have same length");
        }

        Complex sum = 0;
        for (var i = 0; i < a.Length / 2; ++i)
        {
            var aComplex = new Complex(a.Values[2 * i], -a.Values[2 * i + 1]);
            var bComplex = new Complex(b.Values[2 * i], b.Values[2 * i + 1]);

            sum += aComplex * bComplex;
        }

        return sum;
    }
    
    private static Complex PseudoScalarProduct(ComplexVector a, ComplexVector b) // 3
    {
        if (a.Length % 2 != 0 || b.Length % 2 != 0)
        {
            throw new ArgumentException("Vectors must have an even length");
        }

        if (a.Length != b.Length)
        {
            throw new ArgumentException("Vector and result must have same length");
        }

        Complex sum = 0;
        for (var i = 0; i < a.Length / 2; ++i)
        {
            var aComplex = new Complex(a.Values[2 * i], a.Values[2 * i + 1]);
            var bComplex = new Complex(b.Values[2 * i], b.Values[2 * i + 1]);

            sum += aComplex * bComplex;
        }

        return sum;
    }
}