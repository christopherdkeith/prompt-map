using System.Reflection;

namespace PromptMap.Tests.Helpers;

public static class TestPaths
{
    /// <summary>
    /// Attempts to find the repo root by walking up from the test assembly location
    /// until it finds a solution file or .git folder.
    /// </summary>
    public static string RepoRoot()
    {
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        for (int i = 0; i < 10 && dir is not null; i++)
        {
            if (File.Exists(Path.Combine(dir, "PromptMap.sln")) ||
                Directory.Exists(Path.Combine(dir, ".git")))
            {
                return dir;
            }
            dir = Directory.GetParent(dir)?.FullName!;
        }

        // Fallback: current working directory (useful in some runners)
        return Directory.GetCurrentDirectory();
    }
}
