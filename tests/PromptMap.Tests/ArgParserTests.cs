using PromptMap.Cli;
using Xunit;

namespace PromptMap.Tests;

public class ArgParserTests
{
    [Fact]
    public void Parse_MissingValue_Throws()
    {
        Assert.Throws<ArgumentException>(() => ArgParser.Parse(new[] { "--solution" }));
    }

    [Fact]
    public void Parse_Project_Works()
    {
        var opt = ArgParser.Parse(new[] { "--project", "My.csproj", "--out", "map.txt" });
        Assert.Equal("My.csproj", opt.ProjectPath);
        Assert.Null(opt.SolutionPath);
        Assert.Null(opt.DirPath);
    }

    [Fact]
    public void Parse_OnlyOneInput_Required()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ArgParser.Parse(new[] { "--solution", "a.sln", "--project", "b.csproj" }));
        Assert.Contains("Use exactly one of --solution, --project, or --dir.", ex.Message);
    }

    [Fact]
    public void Parse_RequiresSolutionOrDir()
    {
        var ex = Assert.Throws<ArgumentException>(() => ArgParser.Parse(Array.Empty<string>()));
        Assert.Contains("Use exactly one of --solution, --project, or --dir.", ex.Message);
    }

    [Fact]
    public void Parse_SolutionAndOut_Works()
    {
        var opt = ArgParser.Parse(new[]
        {
            "--solution", "My.sln",
            "--out", "map.txt",
            "--include-private",
            "--include-ctors"
        });

        Assert.Equal("My.sln", opt.SolutionPath);
        Assert.Equal("map.txt", opt.OutPath);
        Assert.True(opt.IncludePrivate);
        Assert.True(opt.IncludeCtors);
        Assert.Null(opt.DirPath);
    }
}
