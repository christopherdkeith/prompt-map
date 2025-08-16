#nullable enable
using System.Text;

namespace PromptMap.Cli;

/// <summary>Minimal argument parser (no external deps) with help text.</summary>
internal static class ArgParser
{
    public static Options Parse(string[] args)
    {
        string? sol = null;
        string? proj = null;
        string? dir = null;
        string? outp = null;
        bool includePrivate = false;
        bool includeCtors = false;
        bool verbose = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--solution": sol = Next(args, ref i); break;
                case "--project": proj = Next(args, ref i); break;
                case "--dir": dir = Next(args, ref i); break;
                case "--out": outp = Next(args, ref i); break;
                case "--include-private": includePrivate = true; break;
                case "--include-ctors": includeCtors = true; break;
                case "--verbose":
                case "-v":
                    verbose = true; break;
                case "-h":
                case "--help":
                    PrintHelp(); Environment.Exit(0); break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        var specified = new[] { sol, proj, dir }.Count(x => !string.IsNullOrWhiteSpace(x));
        if (specified != 1)
            throw new ArgumentException("Use exactly one of --solution, --project, or --dir.");

        return new Options
        {
            SolutionPath = sol,
            ProjectPath = proj,
            DirPath = dir,
            OutPath = outp,
            IncludePrivate = includePrivate,
            IncludeCtors = includeCtors,
            Verbose = verbose
        };
    }

    public static void PrintHelp(bool error = false)
    {
        var text = """
PromptMap — Map .NET solutions/projects/directories into AI-friendly text.

Usage:
  promptmap --solution <path-to.sln>   [--out map.txt] [--include-private] [--include-ctors]
  promptmap --project  <path-to.csproj>[--out map.txt] [--include-private] [--include-ctors]
  promptmap --dir      <path-to-folder>[--out map.txt] [--include-private] [--include-ctors]

Options:
  --solution           Path to a .sln file
  --project            Path to a .csproj file
  --dir                Directory to scan (recursive)
  --out                Output file path (defaults to stdout)
  --include-private    Include private members
  --include-ctors      Include constructors
  --verbose, -v        Print MSBuild workspace diagnostics
  -h, --help           Show help and exit
""";
        (error ? Console.Error : Console.Out).WriteLine(text);
    }

    private static string Next(string[] a, ref int i)
    {
        if (i + 1 >= a.Length) throw new ArgumentException($"Missing value for {a[i]}");
        return a[++i];
    }
}
