#nullable enable
using System.Collections.Generic;

namespace PromptMap.Core;

/// <summary>Tree node representing solution > project > namespace > type > members.</summary>
public sealed class Node
{
    public string Name { get; }
    public SortedDictionary<string, Node> Children { get; } = [];
    public List<string> Lines { get; } = [];

    public Node(string name) => Name = name;
}
