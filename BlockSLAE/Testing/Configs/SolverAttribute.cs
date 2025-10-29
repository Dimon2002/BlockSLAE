namespace BlockSLAE.Testing.Configs;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SolverAttribute : Attribute
{
    public string Name { get; }

    public SolverAttribute(string name)
    {
        Name = name;
    }
}