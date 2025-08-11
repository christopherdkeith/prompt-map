#nullable enable
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace PromptMap.Cli;

/// <summary>
/// Opens .sln / .csproj with MSBuildWorkspace and guarantees disposal.
/// </summary>
internal static class WorkspaceLoader
{
    private static void EnsureMsBuildRegistered()
    {
        if (!MSBuildLocator.IsRegistered)
            MSBuildLocator.RegisterDefaults();
    }

    /// <summary>
    /// Opens a solution, runs the provided function, and disposes the workspace.
    /// </summary>
    public static async Task<T> WithSolutionAsync<T>(string solutionPath, Func<Solution, Task<T>> work, CancellationToken ct)
    {
        EnsureMsBuildRegistered();
        using var ws = MSBuildWorkspace.Create();
        var solution = await ws.OpenSolutionAsync(solutionPath, cancellationToken: ct).ConfigureAwait(false);
        return await work(solution).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens a project, runs the provided function, and disposes the workspace.
    /// </summary>
    public static async Task<T> WithProjectAsync<T>(string projectPath, Func<Project, Task<T>> work,CancellationToken ct)
    {
        EnsureMsBuildRegistered();
        using var ws = MSBuildWorkspace.Create();
        var project = await ws.OpenProjectAsync(projectPath, cancellationToken: ct).ConfigureAwait(false);
        return await work(project).ConfigureAwait(false);
    }
}
