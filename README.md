# PromptMap

**PromptMap** is a .NET console application that scans a Visual Studio `.sln` or `.csproj` file (or a directory of `.cs` files) and generates a structured map of your solution or project — files, namespaces, classes, methods, and properties.

The generated text is designed to be **AI-friendly**, so you can paste it into ChatGPT (or any other coding assistant) to give it detailed context about your project before asking questions.

---

## Features

- Parse `.sln` or `.csproj` or directory recursively
- Map namespaces, classes, methods, and properties
- Options to include private members and constructors
- Output to a file or standard output
- AI-friendly format for better ChatGPT coding assistance
- Runs locally — no external services

---

## Installation

You can clone and build PromptMap yourself:

```bash
git clone https://github.com/christopherdkeith/PromptMap.git
cd PromptMap
dotnet build
```

---

## Usage

```bash
promptmap --solution <path-to.sln>    [options]
promptmap --project  <path-to.csproj> [options]
promptmap --dir      <path-to-folder> [options]
```

---

## Options


| Option              | Description                        |
| ------------------- | ---------------------------------- |
| `--solution <path>` | Path to `.sln` file                |
| `--project <path>`  | Path to `.csproj` file             |
| `--dir <path>`      | Path to directory to scan          |
| `--out <path>`      | Output file path (default: stdout) |
| `--include-private` | Include private members            |
| `--include-ctors`   | Include constructors               |
| `-h, --help`        | Show help                          |

---

## Example

Here’s PromptMap mapping itself:

```
PromptMap
 └─ PromptMap
     ├─ <global namespace>
     │   └─ Program
     │       ├─ Method Task<int> Main(string[] args) [public]
     ├─ PromptMap.Cli
     │   ├─ ArgParser
     │   │   ├─ Method Options Parse(string[] args) [public]
     │   │   ├─ Method void PrintHelp(bool error) [public]
     │   ├─ Node
     │   │   ├─ Property string Name { get; } [public]
     │   │   ├─ Property SortedDictionary<string, Node> Children { get; } [public]
     │   │   ├─ Property List<string> Lines { get; } [public]
     │   └─ Options
     │       ├─ Property string? SolutionPath { get; set; } [public]
     │       ├─ Property string? DirPath { get; set; } [public]
     │       ├─ Property string? OutPath { get; set; } [public]
     │       ├─ Property bool IncludePrivate { get; set; } [public]
     │       ├─ Property bool IncludeCtors { get; set; } [public]
     ├─ PromptMap.Cli.Analysis
     │   └─ RoslynWalker
     │       ├─ Method Task<Node> FromSolutionAsync(string solutionPath, bool includePrivate, bool includeCtors, CancellationToken ct) [public]
     │       ├─ Method Node FromDirectory(string dirPath, bool includePrivate, bool includeCtors, CancellationToken ct) [public]
     └─ PromptMap.Cli.Printing
         └─ TreePrinter
             ├─ Method string Print(Node root) [public]

```

## Why PromptMap?

When using ChatGPT (or any AI assistant) to work with your project, context matters. Instead of pasting individual files or trying to describe your project structure manually, PromptMap gives the AI a full structural overview in one compact, readable format.