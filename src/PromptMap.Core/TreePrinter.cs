#nullable enable
using System.Text;

namespace PromptMap.Cli.Printing;

/// <summary>Renders a <see cref="Node"/> tree into the ASCII layout used in examples.</summary>
internal static class TreePrinter
{
    public static string Print(Node root)
    {
        var sb = new StringBuilder();
        PrintNode(root, sb, prefix: "");
        return sb.ToString();
    }

    private static void PrintNode(Node node, StringBuilder sb, string prefix)
    {
        if (!string.IsNullOrEmpty(node.Name)) sb.AppendLine(node.Name);

        foreach (var line in node.Lines)
            sb.AppendLine(prefix + " ├─ " + line);

        int idx = 0; int count = node.Children.Count;
        foreach (var kv in node.Children)
        {
            bool last = ++idx == count;
            sb.AppendLine(prefix + (last ? " └─ " : " ├─ ") + kv.Key);
            PrintChildren(kv.Value, sb, prefix + (last ? "    " : " │  "));
        }
    }

    private static void PrintChildren(Node node, StringBuilder sb, string prefix)
    {
        foreach (var line in node.Lines)
            sb.AppendLine(prefix + " ├─ " + line);

        int idx = 0; int count = node.Children.Count;
        foreach (var kv in node.Children)
        {
            bool last = ++idx == count;
            sb.AppendLine(prefix + (last ? " └─ " : " ├─ ") + kv.Key);
            PrintChildren(kv.Value, sb, prefix + (last ? "    " : " │  "));
        }
    }
}
