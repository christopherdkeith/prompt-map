#nullable enable
namespace PromptMap.Cli;

/// <summary>Minimal argument parser (no external deps) with help text.</summary>
internal static class ArgParser
{
    public static Options Parse(string[] args)
    {
        string? sol = null, dir = null, outp = null;
        bool includePrivate = false, includeCtors = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--solution": sol = Next(args, ref i); break;
                case "--dir": dir = Next(args, ref i); break;
                case "--out": outp = Next(args, ref i); break;
                case "--include-private": includePrivate = true; break;
                case "--include-ctors": includeCtors = true; break;
                case "-h":
                case "--help":
                    PrintHelp(); Environment.Exit(0); break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        if (sol is null && dir is null)
            throw new ArgumentException("Either --solution or --dir is required.");

        return new Options
        {
            SolutionPath = sol,
            DirPath = dir,
            OutPath = outp,
            IncludePrivate = includePrivate,
            IncludeCtors = includeCtors
        };
    }

    public static void PrintHelp(bool error = false)
    {
        var text = """
PromptMap — Map .NET solutions/directories into AI-friendly text.

Usage:
  promptmap --solution <path-to.sln> [--out map.txt] [--include-private] [--include-ctors]
  promptmap --dir <path-to-folder>  [--out map.txt] [--include-private] [--include-ctors]

Options:
  --solution           Path to a .sln file
  --dir                Directory to scan (recursive)
  --out                Output file path (defaults to stdout)
  --include-private    Include private members
  --include-ctors      Include constructors
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
