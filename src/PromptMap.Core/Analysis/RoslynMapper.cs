#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PromptMap.Core.Analysis;

/// <summary>
/// Maps Roslyn objects (Solution/Project/Syntax) into a <see cref="Node"/> tree.
/// Core contains no MSBuild/VS workspace loading; callers pass Roslyn objects in.
/// </summary>
public static class RoslynMapper
{
    public static async Task<Node> MapSolutionAsync(Solution solution, bool includePrivate, bool includeCtors, CancellationToken ct)
    {
        var root = new Node(Path.GetFileNameWithoutExtension(solution.FilePath) ?? "Solution");

        foreach (var project in solution.Projects.Where(p => p.Language == LanguageNames.CSharp))
        {
            ct.ThrowIfCancellationRequested();
            var projNode = Get(root, project.Name);
            await AnalyzeProjectAsync(project, projNode, includePrivate, includeCtors, ct).ConfigureAwait(false);
        }

        return root;
    }

    public static async Task<Node> MapProjectAsync(Project project, bool includePrivate, bool includeCtors, CancellationToken ct)
    {
        var root = new Node(Path.GetFileNameWithoutExtension(project.FilePath) ?? project.Name);
        await AnalyzeProjectAsync(project, root, includePrivate, includeCtors, ct).ConfigureAwait(false);
        return root;
    }

    /// <summary>
    /// Directory scan mode (no MSBuild). Parses .cs files and maps by syntax
    /// with best-effort semantic info when available.
    /// </summary>
    public static Node MapDirectory(
        string dirPath, bool includePrivate, bool includeCtors, CancellationToken ct)
    {
        var root = new Node(new DirectoryInfo(dirPath).Name);
        var projNode = Get(root, "(Directory Scan)");

        var files = Directory.EnumerateFiles(dirPath, "*.cs", SearchOption.AllDirectories)
            .Where(p =>
                !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) &&
                !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                !p.Contains(Path.DirectorySeparatorChar + ".vs" + Path.DirectorySeparatorChar) &&
                !p.Contains(Path.DirectorySeparatorChar + "node_modules" + Path.DirectorySeparatorChar));

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            var text = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: ct);
            var rootNode = tree.GetRoot(ct);

            // Lightweight compilation for basic symbols; falls back to syntax-only if needed.
            var compilation = CSharpCompilation.Create("scan").AddSyntaxTrees(tree);
            var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);

            foreach (var typeDecl in rootNode.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(typeDecl, ct) as INamedTypeSymbol;
                var ns = symbol?.ContainingNamespace?.ToDisplayString() ?? GuessNamespace(typeDecl) ?? "<global>";
                var nsNode = Get(projNode, ns);
                var tNode = Get(nsNode, typeDecl.Identifier.Text);

                if (symbol is not null)
                    AppendMembers(tNode, symbol, includePrivate, includeCtors);
                else
                    AppendMembersSyntaxOnly(tNode, typeDecl, includePrivate, includeCtors);
            }
        }

        return root;
    }

    private static async Task AnalyzeProjectAsync(Project project, Node projNode, bool includePrivate, bool includeCtors, CancellationToken ct)
    {
        var compilation = await project.GetCompilationAsync(ct).ConfigureAwait(false);
        if (compilation is null) return;

        var typeMap = new ConcurrentDictionary<string, Node>();
        var nsMap = new ConcurrentDictionary<string, Node>();

        var docTasks = project.Documents
            .Where(d => d.SourceCodeKind == SourceCodeKind.Regular &&
                        (d.FilePath?.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ?? false))
            .Select(async doc =>
            {
                try
                {
                    var model = await doc.GetSemanticModelAsync(ct).ConfigureAwait(false);
                    if (model is null) return;

                    var syntax = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);
                    if (syntax is null) return;

                    foreach (var typeDecl in syntax.DescendantNodes().OfType<TypeDeclarationSyntax>())
                    {
                        var symbol = model.GetDeclaredSymbol(typeDecl, ct) as INamedTypeSymbol;
                        if (symbol is null) continue;

                        var ns = symbol.ContainingNamespace?.ToDisplayString() ?? "<global>";
                        var typeKey = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                        var nsNode = nsMap.GetOrAdd(ns, _ => Get(projNode, ns));
                        var tNode = typeMap.GetOrAdd(typeKey, _ => Get(nsNode, symbol.Name));

                        AppendMembers(tNode, symbol, includePrivate, includeCtors);
                    }
                }
                catch (OperationCanceledException) { /* fine */ }
                catch { /* swallow per-doc to keep walking; add verbose flag later */ }
            });

        await Task.WhenAll(docTasks).ConfigureAwait(false);
    }

    private static void AppendMembers(Node tNode, INamedTypeSymbol type, bool includePrivate, bool includeCtors)
    {
        foreach (var m in type.GetMembers())
        {
            switch (m)
            {
                case IMethodSymbol method:
                    if (method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise)
                        continue;
                    if (!includeCtors && method.MethodKind == MethodKind.Constructor) continue;
                    if (!includePrivate && !IsPublicish(method.DeclaredAccessibility)) continue;
                    tNode.Lines.Add(FormatMethod(method));
                    break;

                case IPropertySymbol prop:
                    if (!includePrivate && !IsPublicish(prop.DeclaredAccessibility)) continue;
                    tNode.Lines.Add(FormatProperty(prop));
                    break;
            }
        }
    }

    private static void AppendMembersSyntaxOnly(Node tNode, TypeDeclarationSyntax typeDecl, bool includePrivate, bool includeCtors)
    {
        foreach (var member in typeDecl.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax m:
                    tNode.Lines.Add($"Method {m.ReturnType} {m.Identifier}{m.ParameterList} [{GuessAccess(m.Modifiers)}]");
                    break;

                case ConstructorDeclarationSyntax c when includeCtors:
                    tNode.Lines.Add($"Ctor {c.Identifier}{c.ParameterList} [{GuessAccess(c.Modifiers)}]");
                    break;

                case PropertyDeclarationSyntax p:
                    tNode.Lines.Add($"Property {p.Type} {p.Identifier} {{ … }} [{GuessAccess(p.Modifiers)}]");
                    break;
            }
        }
    }

    private static bool IsPublicish(Accessibility a) =>
        a is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;

    private static string FormatMethod(IMethodSymbol m)
    {
        var access = m.DeclaredAccessibility.ToString().ToLowerInvariant();
        var ret = m.MethodKind == MethodKind.Constructor
            ? "ctor"
            : m.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        var name = m.MethodKind == MethodKind.Constructor ? m.ContainingType.Name : m.Name;

        var parms = string.Join(", ", m.Parameters.Select(
            p => $"{p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {p.Name}"));

        return $"Method {ret} {name}({parms}) [{access}]";
    }

    private static string FormatProperty(IPropertySymbol p)
    {
        var access = p.DeclaredAccessibility.ToString().ToLowerInvariant();
        var acc = $"{{{(p.GetMethod != null ? " get;" : string.Empty)}{(p.SetMethod != null ? " set;" : string.Empty)} }}";
        return $"Property {p.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)} {p.Name} {acc} [{access}]";
    }

    private static string? GuessNamespace(TypeDeclarationSyntax typeDecl) =>
        typeDecl.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();

    private static string GuessAccess(SyntaxTokenList mods)
    {
        if (mods.Any(SyntaxKind.PublicKeyword)) return "public";
        if (mods.Any(SyntaxKind.InternalKeyword)) return "internal";
        if (mods.Any(SyntaxKind.ProtectedKeyword) && mods.Any(SyntaxKind.InternalKeyword)) return "protected internal";
        if (mods.Any(SyntaxKind.ProtectedKeyword)) return "protected";
        if (mods.Any(SyntaxKind.PrivateKeyword)) return "private";
        return "private";
    }

    private static Node Get(Node parent, string name)
    {
        if (!parent.Children.TryGetValue(name, out var child))
        {
            child = new Node(name);
            parent.Children.Add(name, child);
        }
        return child;
    }
}
