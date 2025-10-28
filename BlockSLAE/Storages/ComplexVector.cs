using System.Collections;
using System.Numerics;

namespace BlockSLAE.Storages;

public class ComplexVector : IEnumerable<double>
{
    public double[] Values { get; }
    public int Length => Values.Length;

    public double Norm => double.Sqrt(ScalarProduct(this, this, _degreeOfParallelism).Real);

    private int _degreeOfParallelism;

    public ComplexVector(IEnumerable<double> values, int degreeOfParallelism = 1)
    {
        Values = values.ToArray();
        _degreeOfParallelism = degreeOfParallelism;
    }

    public static ComplexVector Create(int length, int degreeOfParallelism = 1)
    {
        return new ComplexVector(new double[length], degreeOfParallelism);
    }

    public static ComplexVector Create(IEnumerable<double> values, int degreeOfParallelism = 1)
    {
        return new ComplexVector(values, degreeOfParallelism);
    }

    public static ComplexVector None => new([]);

    public ComplexVector Clone()
    {
        return new ComplexVector(Values, _degreeOfParallelism);
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

    public ComplexVector SetDegreeOfParallelism(int degreeOfParallelism)
    {
        _degreeOfParallelism = degreeOfParallelism;
        return this;
    }
    
    public ComplexVector MultiplyOn(Complex complexScalar, ComplexVector? resultMemory = null)
    {
        resultMemory ??= Clone();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _degreeOfParallelism
        };

        Parallel.For(0, Length / 2, parallelOptions, i =>
        {
            var z = new Complex(Values[i * 2], Values[i * 2 + 1]);
            var aByZ = complexScalar * z;
            
            resultMemory.Values[i * 2] = aByZ.Real;
            resultMemory.Values[i * 2 + 1] = aByZ.Imaginary;
        });

        return resultMemory;
    }

    public Complex ScalarProduct(ComplexVector outerVector)
    {
        return ScalarProduct(this, outerVector, _degreeOfParallelism);
    }

    public Complex PseudoScalarProduct(ComplexVector outerVector)
    {
        return PseudoScalarProduct(this, outerVector, _degreeOfParallelism);
    }

    public void Add(ComplexVector right, ComplexVector? resultMemory = null)
    {
        LinearCombination(1, 1, this, right, resultMemory ?? Create(right.Length));
    }

    public void Subtract(ComplexVector right, ComplexVector? resultMemory = null)
    {
        LinearCombination(1, -1, this, right, resultMemory ?? Create(right.Length));
    }

    private void LinearCombination(int a, int b, ComplexVector left, ComplexVector right, ComplexVector resultMemory)
    {
        if (left.Length != right.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _degreeOfParallelism
        };

        Parallel.For(0, Length, parallelOptions,
            i => resultMemory.Values[i] = a * left.Values[i] + b * right.Values[i]);
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

    private static Complex ScalarProduct(ComplexVector a, ComplexVector b, int degreeOfParallelism) // 2
    {
        if (a.Length % 2 != 0 || b.Length % 2 != 0)
        {
            throw new ArgumentException("Vectors must have an even length");
        }

        if (a.Length != b.Length)
        {
            throw new ArgumentException("Vector and result must have same length");
        }

        var size = a.Length / 2;
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = degreeOfParallelism
        };
        var threadLocalResults = new ThreadLocal<Complex>(() => Complex.Zero, trackAllValues: true);

        Parallel.For(0, size, parallelOptions, i =>
        {
            var aComplex = new Complex(a.Values[2 * i], -a.Values[2 * i + 1]);
            var bComplex = new Complex(b.Values[2 * i], b.Values[2 * i + 1]);
            threadLocalResults.Value += aComplex * bComplex;
        });

        return threadLocalResults.Values.Aggregate<Complex, Complex>(0, (current, local) => current + local);
    }

    private static Complex PseudoScalarProduct(ComplexVector a, ComplexVector b, int degreeOfParallelism) // 3
    {
        return ScalarProduct(Conjugate(a), b, degreeOfParallelism);
    }

    public IEnumerator<double> GetEnumerator() => ((IEnumerable<double>)Values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}