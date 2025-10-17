namespace BlockSLAE.Storages;

public readonly struct ComplexValue
{
    private readonly double[] _values;

    public ComplexValue()
    {
        _values = [0];
    }
    
    private ComplexValue(double[] values)
    {
        _values = values;
    }

    public static ComplexValue Create(double real, double imaginary)
    {
        return imaginary == 0 ?
            new ComplexValue([real]) :
            new ComplexValue([real, imaginary]); 
    }
    
    public double Real => _values[0];
    public double Imaginary => _values.Length == 2 ? _values[1] : 0d;
    
}