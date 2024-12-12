using System.Reflection;

namespace Parser.Tests;

[TestFixture]
public class Tests
{
    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var prefix = $"{assembly.GetName().Name}.Files";
            var resources = assembly.GetManifestResourceNames();

            foreach (var resource in resources.Where(r => r.StartsWith(prefix, StringComparison.Ordinal)))
            {
                yield return new TestCaseData(resource).SetName(resource.Split(".")[^2]);
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    [Parallelizable(ParallelScope.All)]
    public async Task ParserTest(string source)
    {
        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(source) 
                                 ?? throw new InvalidOperationException();

        using StreamReader reader = new(stream);

        Assert.That(Parser.TryParse(await reader.ReadToEndAsync(), out _, out _), Is.True);
    }
}