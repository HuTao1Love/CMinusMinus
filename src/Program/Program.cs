using System.CommandLine;
using Interpreter;
using Interpreter.Optimizer;

namespace Program;

public static class Program
{
    private static readonly Argument<string> _file = new("code", "File with code");

    private static readonly Option<string> _optimizations = new(
        ["-o", "--optimizations"], () => string.Empty, "Optimizations to compile the code")
    {
    };

    private static readonly Option<bool> _writeInfo = new(["-i", "--info", "--information"], "Write information about compile & run time");

    private static readonly IReadOnlyDictionary<char, IOptimizer> _optimizers =
        new Dictionary<char, IOptimizer>()
        {
            { 'c', new ConstFoldingOptimizer() },
        };

    public static int Main(string[] args)
    {
        var compile = new Command("compile", "Only compile code from selected file")
        {
            _file,
            _optimizations,
            _writeInfo,
        };
        var run = new Command("run", "Only run code from selected compiled file")
        {
            _file,
            _writeInfo,
        };

        var root = new RootCommand
        {
            compile,
            run,
            _file,
            _optimizations,
            _writeInfo,
        };

        compile.SetHandler(
            (file, writeInfo) => Compile(file, writeInfo),
            _file,
            _writeInfo);

        run.SetHandler(
            (file, optimizations, writeInfo) => Run(file, optimizations, writeInfo),
            _file,
            _optimizations,
            _writeInfo);

        root.SetHandler(
            (file, optimizations, writeInfo) =>
            {
                if (!Compile(file, writeInfo)) return;

                Run(file, optimizations, writeInfo);
            },
            _file,
            _optimizations,
            _writeInfo);

        return root.Invoke(args);
    }

    private static bool Compile(string file, bool writeInfo)
    {
        var output = writeInfo ? Console.Out : TextWriter.Null;

        output.WriteLine("Compilation...");
        try
        {
            var start = DateTime.Now;
            Compiler.CodeCompiler.Compile(file);
            var end = DateTime.Now - start;
            Console.WriteLine($"Compilation success. Time: {end.TotalMilliseconds}ms");

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Compilation problem: {e}");
            return false;
        }
    }

    private static bool Run(string file, string optimizations, bool writeInfo = true)
    {
        var output = writeInfo ? Console.Out : TextWriter.Null;
        var compiled = file.EndsWith(".cmmbin", StringComparison.InvariantCulture) ? file : $"{file}.cmmbin";
        output.WriteLine("Execute run");

        var invalidOptimizations = optimizations.Where(o => !_optimizers.ContainsKey(o)).Aggregate(string.Empty, (s, c) => s + c);
        if (invalidOptimizations.Length != 0)
        {
            Console.WriteLine($"Invalid optimizations: {invalidOptimizations}. They were ignored.");
        }

        var optimizers = optimizations.Where(o => _optimizers.ContainsKey(o)).Select(o => _optimizers[o]);
        output.WriteLine("Running...\n===============");

        try
        {
            var start = DateTime.Now;
            var vm = new VirtualMachine(output, optimizers);
            vm.Run(compiled);
            var end = DateTime.Now - start;
            output.WriteLine("===============\n");
            Console.WriteLine($"Run success. Time: {end.TotalMilliseconds}ms");

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Run problem: {e}");
            return false;
        }
    }
}
