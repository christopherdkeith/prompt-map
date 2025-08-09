#nullable enable
namespace PromptMap.Cli;

/// <summary>Tree node representing solution > project > namespace > type > members.</summary>
internal sealed class Node
{
    public string Name { get; }
    public SortedDictionary<string, Node> Children { get; } = new();
    public List<string> Lines { get; } = new();

    public Node(string name) => Name = name;
}
