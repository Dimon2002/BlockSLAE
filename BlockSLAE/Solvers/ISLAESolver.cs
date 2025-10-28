using BlockSLAE.Storages;
using BlockSLAE.Storages.Structures;

namespace BlockSLAE.Solvers;

public interface ISLAESolver
{
    ComplexVector Solve(ComplexEquation  equation);

    ISLAESolver SetDegreeOfParallelism(int degreeOfParallelism);
}