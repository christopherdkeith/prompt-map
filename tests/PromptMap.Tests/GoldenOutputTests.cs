using PromptMap.Cli.Analysis;
using PromptMap.Cli.Printing;
using PromptMap.Tests.Helpers;
using Xunit;

namespace PromptMap.Tests;

public class GoldenOutputTests
{
    [Fact]
    public void DirectoryFixture_MatchesExpectedGolden()
    {
        var rootDir = TestPaths.RepoRoot();
        var fixtureDir = Path.Combine(rootDir, "tests", "PromptMap.Tests", "Fixtures");

        var root = RoslynWalker.FromDirectory(fixtureDir, includePrivate: false, includeCtors: false, default);
        var actual = TreePrinter.Print(root).NormalizeLines();

        var expectedPath = Path.Combine(fixtureDir, "ExpectedDirectoryScan.txt");
        var expected = File.ReadAllText(expectedPath).NormalizeLines();

        Assert.Equal(expected, actual);
    }
}
