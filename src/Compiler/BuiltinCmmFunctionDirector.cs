using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Compiler.Builtins;

namespace Compiler;

public static class BuiltinCmmFunctionDirector
{
    private static IReadOnlyDictionary<string, IBuiltin> _builtins;

    static BuiltinCmmFunctionDirector()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var interfaceType = typeof(IBuiltin);

        var implementations = currentAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && interfaceType.IsAssignableFrom(type));

        _builtins = implementations.Select(IBuiltin (t) => (IBuiltin)Activator.CreateInstance(t)!)
            .ToImmutableDictionary(b => b.Name);
    }

    public static bool TryGetBuiltin(string function, [NotNullWhen(true)] out IBuiltin? builtin)
        => _builtins.TryGetValue(function, out builtin);
}
