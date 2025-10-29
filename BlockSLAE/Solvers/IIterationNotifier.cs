namespace BlockSLAE.Solvers;

public interface IIterationNotifier
{
    event Action<int, double, double> IterationCompleted;
}