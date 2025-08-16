#nullable enable
namespace PromptMap.Cli;

/// <summary>Command-line options for PromptMap.</summary>
internal sealed class Options
{
    public string? SolutionPath { get; init; }
    public string? ProjectPath { get; init; }
    public string? DirPath { get; init; }
    public string? OutPath { get; init; }
    public bool IncludePrivate { get; init; }
    public bool IncludeCtors { get; init; }
    public bool Verbose { get; init; }
}
