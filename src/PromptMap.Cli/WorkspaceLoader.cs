#nullable enable
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace PromptMap.Cli;

internal static class WorkspaceLoader
{
    internal static Action<string>? Log { get; set; }

    private static void EnsureMsBuildRegistered()
    {
        if (MSBuildLocator.IsRegistered) return;

        var instances = MSBuildLocator.QueryVisualStudioInstances()
                                      .OrderByDescending(i => i.Version)
                                      .ToArray();
        if (instances.Length > 0)
            MSBuildLocator.RegisterInstance(instances[0]);
        else
            MSBuildLocator.RegisterDefaults();
    }

    private static MSBuildWorkspace CreateWorkspace()
    {
        var ws = MSBuildWorkspace.Create();
        ws.WorkspaceFailed += (_, e) =>
        {
            var msg = $"[MSBuild:{e.Diagnostic.Kind}] {e.Diagnostic.Message}";
            if (Log != null) Log(msg);
        };
        return ws;
    }

    public static async Task<T> WithSolutionAsync<T>(string solutionPath, Func<Solution, Task<T>> work, CancellationToken ct)
    {
        EnsureMsBuildRegistered();
        using var ws = CreateWorkspace();
        var sln = await ws.OpenSolutionAsync(solutionPath, cancellationToken: ct).ConfigureAwait(false);
        return await work(sln).ConfigureAwait(false);
    }

    public static async Task<T> WithProjectAsync<T>(string projectPath, Func<Project, Task<T>> work, CancellationToken ct)
    {
        EnsureMsBuildRegistered();
        using var ws = CreateWorkspace();
        var proj = await ws.OpenProjectAsync(projectPath, cancellationToken: ct).ConfigureAwait(false);
        return await work(proj).ConfigureAwait(false);
    }
}
