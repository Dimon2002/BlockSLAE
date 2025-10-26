using System.Collections;
using System.Numerics;

namespace BlockSLAE.Storages;

public class ComplexVector : IEnumerable<double>
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

    public static ComplexVector None => new([]);

    public ComplexVector Clone()
    {
        return new ComplexVector(Values);
    }

    public void CopyFrom(ComplexVector other)
    {
        if (other.Length != Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }
       
        Array.Copy(other.Values, Values, Length);
    }

    public void Nullify()
    {
        Array.Clear(Values);
    }

    public ComplexVector MultiplyOn(Complex complexScalar, ComplexVector? resultMemory = null)
    {
        resultMemory ??= Clone();

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
        return PseudoScalarProduct(this, outerVector);
    }

    public void Add(ComplexVector other, ComplexVector? resultMemory = null)
    {
        LinearCombination(1, 1, other, resultMemory ?? Create(other.Length));
    }

    public void Subtract(ComplexVector other, ComplexVector? resultMemory = null)
    {
        LinearCombination(1, -1, other, resultMemory ?? Create(other.Length));
    }

    private void LinearCombination(int a, int b, ComplexVector other, ComplexVector resultMemory)
    {
        for (var i = 0; i < Length; i++)
        {
            resultMemory.Values[i] = a * Values[i] + b * other.Values[i];
        }
    }
    
    private static ComplexVector Conjugate(ComplexVector vector)
    {
        var conjugatedVector = vector.Clone();

        for (var i = 1; i < conjugatedVector.Length; i += 2)
        {
            conjugatedVector.Values[i] = -conjugatedVector.Values[i];
        }

        return new ComplexVector(conjugatedVector);
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
        return ScalarProduct(Conjugate(a), b);
    }

    public IEnumerator<double> GetEnumerator() => ((IEnumerable<double>)Values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}