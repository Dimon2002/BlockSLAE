using BlockSLAE.Storages;
using NUnit.Framework;

namespace BlockSLAE.Tests;

public class ComplexValueTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void RealValueShouldBeCorrect()
    {
        var complexValue = ComplexValue.Create(1d, 0d);

        var realValue = complexValue.Real;
        var imaginaryValue = complexValue.Imaginary;

        const double expectedReal = 1;
        const double expectedImaginary = 0;

        List<double> readOnlySpan = [realValue, imaginaryValue];
        List<double> expected = [expectedReal, expectedImaginary];

        Assert.That(readOnlySpan, Is.EqualTo(expected));
    }

    [Test]
    public void ImaginaryValueShouldBeCorrect()
    {
        var complexValue = ComplexValue.Create(0d, 15d);

        var realValue = complexValue.Real;
        var imaginaryValue = complexValue.Imaginary;

        const double expectedReal = 0;
        const double expectedImaginary = 15;

        List<double> readOnlySpan = [realValue, imaginaryValue];
        List<double> expected = [expectedReal, expectedImaginary];

        Assert.That(readOnlySpan, Is.EqualTo(expected));
    }

    [Test]
    public void ComplexValueShouldBeCorrect()
    {
        var complexValue = ComplexValue.Create(151d, 15d);

        var realValue = complexValue.Real;
        var imaginaryValue = complexValue.Imaginary;

        const double expectedReal = 151;
        const double expectedImaginary = 15;

        List<double> readOnlySpan = [realValue, imaginaryValue];
        List<double> expected = [expectedReal, expectedImaginary];

        Assert.That(readOnlySpan, Is.EqualTo(expected));
    }
    
    [Test]
    public void EmptyConstructorValueShouldBeCorrect()
    {
        var complexValue = new ComplexValue();

        var realValue = complexValue.Real;
        var imaginaryValue = complexValue.Imaginary;

        const double expectedReal = 0;
        const double expectedImaginary = 0;

        List<double> readOnlySpan = [realValue, imaginaryValue];
        List<double> expected = [expectedReal, expectedImaginary];

        Assert.That(readOnlySpan, Is.EqualTo(expected));
    }
}