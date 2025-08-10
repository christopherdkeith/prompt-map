#nullable enable
using PromptMap.Cli;
using PromptMap.Cli.Analysis;
using PromptMap.Cli.Printing;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var opt = ArgParser.Parse(args);

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            Node root;
            if (!string.IsNullOrWhiteSpace(opt.SolutionPath))
            {
                if (!File.Exists(opt.SolutionPath))
                {
                    Console.Error.WriteLine($"Solution not found: {opt.SolutionPath}");
                    return 2;
                }

                root = await RoslynWalker.FromSolutionAsync(
                    opt.SolutionPath!, opt.IncludePrivate, opt.IncludeCtors, cts.Token).ConfigureAwait(false);
            }
            else if (!string.IsNullOrWhiteSpace(opt.ProjectPath))
            {
                if (!File.Exists(opt.ProjectPath))
                {
                    Console.Error.WriteLine($"Project not found: {opt.ProjectPath}");
                    return 2;
                }

                root = await RoslynWalker.FromProjectAsync(
                    opt.ProjectPath!, opt.IncludePrivate, opt.IncludeCtors, cts.Token).ConfigureAwait(false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(opt.DirPath) || !Directory.Exists(opt.DirPath))
                {
                    Console.Error.WriteLine($"Directory not found: {opt.DirPath}");
                    return 2;
                }

                root = RoslynWalker.FromDirectory(
                    opt.DirPath!, opt.IncludePrivate, opt.IncludeCtors, cts.Token);
            }

            var text = TreePrinter.Print(root);

            if (!string.IsNullOrWhiteSpace(opt.OutPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(opt.OutPath))!);
                await File.WriteAllTextAsync(opt.OutPath!, text, cts.Token).ConfigureAwait(false);
                Console.WriteLine($"Wrote: {opt.OutPath}");
            }
            else
            {
                Console.WriteLine(text);
            }

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Operation cancelled.");
            return 130; // 128 + SIGINT
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            ArgParser.PrintHelp(error: true);
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }
    }
}
