namespace PromptMap.Tests.Helpers;

public static class TestStringExtensions
{
    /// <summary>
    /// Normalizes line endings to "\n" and trims trailing newlines,
    /// so snapshots compare reliably across platforms.
    /// </summary>
    public static string NormalizeLines(this string s) =>
        s.Replace("\r\n", "\n").TrimEnd();
}
