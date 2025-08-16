using PromptMap.Core.Analysis;
using PromptMap.Core.Printing;
using Xunit;

namespace PromptMap.Tests;

public class DirectoryScanTests
{
    [Fact]
    public void FromDirectory_SimpleClass_EmitsMembers()
    {
        using var tmp = new TempDir();
        var code = """
        namespace Example;
        public class SampleClass
        {
            public string Name { get; set; }
            public void Run(int times) {}
        }
        """;
        File.WriteAllText(Path.Combine(tmp.Path, "SampleClass.cs"), code);

        var root = RoslynMapper.MapDirectory(tmp.Path, includePrivate: false, includeCtors: false, default);
        var output = TreePrinter.Print(root);

        Assert.Contains("SampleClass", output);
        Assert.Contains("Property string Name", output);
        Assert.Contains("Method void Run(int times)", output);
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pm-tests-" + Guid.NewGuid());
        public TempDir() => Directory.CreateDirectory(Path);
        public void Dispose() { try { Directory.Delete(Path, true); } catch { /* ignore */ } }
    }
}
