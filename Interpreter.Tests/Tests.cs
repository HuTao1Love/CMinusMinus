namespace Interpreter.Tests;

[TestFixture]
public class Tests
{
    private static IEnumerable<TestCaseData> _sources = new DirectoryInfo("Files")
        .EnumerateFiles("*.cmm", SearchOption.AllDirectories)
        .Select(f => new TestCaseData(f));

    [Test]
    [TestCaseSource(nameof(_sources))]
    [Parallelizable(ParallelScope.All)]
    public void ParserTest(string source)
    {
        // var parser = new Cmm();
    }
}