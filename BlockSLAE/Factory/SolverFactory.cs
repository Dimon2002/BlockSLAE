using System.Collections.Concurrent;
using System.Reflection;
using BlockSLAE.Smoothing;
using BlockSLAE.Solvers;
using BlockSLAE.Storages.Structures;
using BlockSLAE.Testing.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlockSLAE.Factory;

public static class SolverFactory
{
    private static readonly ConcurrentDictionary<string, Type> SolversByName = new();
    private static bool _initialized;

    public static ISLAESolver CreateSolver(
        string name,
        SLAEConfig config,
        ISmoothingStrategy smoothing,
        ILoggerFactory? loggerFactory = null)
    {
        EnsureInitialized();

        if (!SolversByName.TryGetValue(name, out var solverType))
            throw new InvalidOperationException($"Solver '{name}' not found or not registered.");

        loggerFactory ??= NullLoggerFactory.Instance;

        var ctor = solverType.GetConstructors()
            .FirstOrDefault(c =>
            {
                var p = c.GetParameters();
                return p.Length == 4
                       && typeof(ComplexDiagonalPreconditionerFactory).IsAssignableFrom(p[0].ParameterType)
                       && typeof(ISmoothingStrategy).IsAssignableFrom(p[1].ParameterType)
                       && typeof(ILogger).IsAssignableFrom(p[2].ParameterType)
                       && typeof(SLAEConfig).IsAssignableFrom(p[3].ParameterType);
            });

        if (ctor == null)
            throw new InvalidOperationException($"Solver '{name}' does not have a suitable constructor.");

        var loggerExtensionsType = typeof(LoggerFactoryExtensions);
        var createLoggerGeneric = loggerExtensionsType
            .GetMethods()
            .First(m => m is
                        {
                            Name: "CreateLogger",
                            IsGenericMethod: true
                        }
                        && m.GetParameters().Length == 1)
            .MakeGenericMethod(solverType);

        var typedLogger = createLoggerGeneric.Invoke(null, new object[] { loggerFactory });

        var solver = (ISLAESolver)ctor.Invoke(new object[]
        {
            new ComplexDiagonalPreconditionerFactory(),
            smoothing,
            typedLogger!,
            config
        });

        return solver;
    }


    public static string[] GetRegisteredSolverNames()
    {
        EnsureInitialized();
        return SolversByName.Keys.OrderBy(n => n).ToArray();
    }

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;

        var solverTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(SafeGetTypes)
            .Where(t => t.GetCustomAttribute<SolverAttribute>() != null)
            .ToArray();

        foreach (var type in solverTypes)
        {
            var attr = type.GetCustomAttribute<SolverAttribute>()!;
            SolversByName[attr.Name] = type;
        }
    }

    private static Type[] SafeGetTypes(Assembly asm)
    {
        try
        {
            return asm.GetTypes();
        }
        catch
        {
            return [];
        }
    }
}