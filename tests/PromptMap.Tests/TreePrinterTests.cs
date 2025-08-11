using PromptMap.Core;
using PromptMap.Core.Printing;
using PromptMap.Tests.Helpers;
using Xunit;

namespace PromptMap.Tests;

public class TreePrinterTests
{
    [Fact]
    public void Print_SimpleTree_MatchesExpected()
    {
        var root = new Node("Root");
        var proj = Ensure(root, "ProjectA");
        var ns = Ensure(proj, "My.Namespace");
        ns.Lines.Add("Property string Name { get; } [public]");
        ns.Lines.Add("Method void Foo() [public]");

        var text = TreePrinter.Print(root).NormalizeLines();

        var expected = """
        Root
         └─ ProjectA
             └─ My.Namespace
                 ├─ Property string Name { get; } [public]
                 ├─ Method void Foo() [public]
        """.NormalizeLines();

        Assert.Equal(expected, text);
    }

    private static Node Ensure(Node parent, string name)
    {
        if (!parent.Children.TryGetValue(name, out var child))
        {
            child = new Node(name);
            parent.Children.Add(name, child);
        }
        return child;
    }
}
