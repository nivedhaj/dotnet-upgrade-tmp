using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run --project src/StudentMigration.Roslyn -- <file-or-directory> [--output <directory>]");
    return 1;
}

string? input = null;
string? outputDirectory = null;
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--output" && i + 1 < args.Length)
    {
        outputDirectory = args[++i];
        continue;
    }
    if (input is null && !args[i].StartsWith("--")) input = args[i];
}

if (input is null)
{
    Console.Error.WriteLine("Input path is required.");
    return 1;
}

var path = Path.GetFullPath(input);
var files = File.Exists(path)
    ? new[] { path }
    : Directory.Exists(path) ? Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories) : Array.Empty<string>();

if (files.Length == 0)
{
    Console.Error.WriteLine($"No C# files found: {path}");
    return 2;
}

var report = new MigrationReport
{
    Tool = "ct.stu RavenDB -> PostgreSQL Roslyn Migration Analysis",
    GeneratedAtUtc = DateTime.UtcNow,
    Input = path,
    FileCount = files.Length
};

foreach (var file in files.OrderBy(x => x))
{
    var text = await File.ReadAllTextAsync(file);
    var tree = CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.LatestMajor));
    var root = await tree.GetRootAsync();
    var result = new FileAnalysis { File = Path.GetRelativePath(Directory.GetCurrentDirectory(), file) };

    foreach (var node in root.DescendantNodes())
    {
        var raven = node switch
        {
            UsingDirectiveSyntax u when u.Name.ToString().StartsWith("Raven.") => $"using {u.Name}",
            ObjectCreationExpressionSyntax o when o.Type.ToString().Contains("DocumentStore") => "new DocumentStore",
            MemberAccessExpressionSyntax m when IsRavenMember(m.Name.Identifier.Text) => m.Name.Identifier.Text,
            InvocationExpressionSyntax i when IsRavenCall(i.Expression.ToString()) => i.Expression.ToString(),
            _ => null
        };
        if (raven is null) continue;

        var line = tree.GetLineSpan(node.Span).StartLinePosition.Line + 1;
        result.Findings.Add(new MigrationFinding
        {
            Line = line,
            RavenApi = raven,
            Replacement = Replacement(raven),
            Confidence = Confidence(raven)
        });
    }

    result.Findings = result.Findings
        .GroupBy(x => new { x.Line, x.RavenApi, x.Replacement })
        .Select(x => x.First())
        .OrderBy(x => x.Line)
        .ToList();

    if (result.Findings.Count > 0)
        report.Files.Add(result);
}

report.DetectedMigrationPoints = report.Files.Sum(x => x.Findings.Count);

Console.WriteLine("ct.stu RavenDB -> PostgreSQL Roslyn Migration Analysis");
Console.WriteLine(new string('=', 62));
Console.WriteLine($"Input: {path}");
Console.WriteLine($"Files scanned: {files.Length}");
Console.WriteLine($"Files with migration points: {report.Files.Count}");
Console.WriteLine();

foreach (var file in report.Files)
{
    Console.WriteLine($"FILE: {file.File}");
    foreach (var finding in file.Findings)
    {
        Console.WriteLine($"  Line {finding.Line}: {finding.RavenApi}");
        Console.WriteLine($"    -> {finding.Replacement} [{finding.Confidence}]");
    }
    Console.WriteLine();
}

Console.WriteLine($"Detected migration points: {report.DetectedMigrationPoints}");

if (outputDirectory is not null)
{
    var output = Path.GetFullPath(outputDirectory);
    Directory.CreateDirectory(output);
    var jsonPath = Path.Combine(output, "migration-report.json");
    var markdownPath = Path.Combine(output, "migration-report.md");

    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(report, jsonOptions));
    await File.WriteAllTextAsync(markdownPath, ToMarkdown(report));

    Console.WriteLine();
    Console.WriteLine($"Reports written to: {output}");
    Console.WriteLine($"  JSON: {jsonPath}");
    Console.WriteLine($"  Markdown: {markdownPath}");
}

return 0;

static bool IsRavenMember(string name) => name is "Load" or "LoadAsync" or "Store" or "StoreAsync" or "SaveChanges" or "SaveChangesAsync" or "Query" or "QueryStatistics" or "Statistics" or "Search" or "Include";
static bool IsRavenCall(string text) => text.Contains("_session.") || text.Contains("session.") || text.Contains("store.OpenSession") || text.Contains("DocumentStore");
static string Replacement(string raven) => raven.Contains("Load") ? "IStudentRepository.GetByIdAsync(...)" : raven.Contains("Store") ? "IStudentRepository.AddAsync(...)" : raven.Contains("SaveChanges") ? "AppDbContext.SaveChangesAsync()" : raven.Contains("Query") ? "EF Core IQueryable<T>" : raven.Contains("Search") ? "PostgreSQL ILIKE / full-text search" : raven.Contains("Include") ? "EF Core Include / relational JOIN" : "Review for PostgreSQL equivalent";
static string Confidence(string raven) => raven.StartsWith("using Raven.") || raven.Contains("Load") || raven.Contains("Store") || raven.Contains("SaveChanges") ? "HIGH" : "MEDIUM";

static string ToMarkdown(MigrationReport report)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("# ct.stu RavenDB → PostgreSQL Migration Report");
    sb.AppendLine();
    sb.AppendLine($"- Generated (UTC): `{report.GeneratedAtUtc:O}`");
    sb.AppendLine($"- Input: `{report.Input}`");
    sb.AppendLine($"- Files scanned: **{report.FileCount}**");
    sb.AppendLine($"- Files with migration points: **{report.Files.Count}**");
    sb.AppendLine($"- Detected migration points: **{report.DetectedMigrationPoints}**");
    sb.AppendLine();

    foreach (var file in report.Files)
    {
        sb.AppendLine($"## `{file.File}`");
        sb.AppendLine();
        sb.AppendLine("| Line | RavenDB API | PostgreSQL replacement | Confidence |");
        sb.AppendLine("|---:|---|---|---|");
        foreach (var f in file.Findings)
            sb.AppendLine($"| {f.Line} | `{Escape(f.RavenApi)}` | `{Escape(f.Replacement)}` | {f.Confidence} |");
        sb.AppendLine();
    }

    sb.AppendLine("## General migration guidance");
    sb.AppendLine();
    sb.AppendLine("- `IDocumentSession.Load<T>` → repository `GetByIdAsync`");
    sb.AppendLine("- `Store` / `SaveChanges` → repository add/update + `SaveChangesAsync`");
    sb.AppendLine("- `Query<T>` / LINQ → EF Core `IQueryable<T>`");
    sb.AppendLine("- Raven `Search` → PostgreSQL `ILIKE` or full-text search");
    sb.AppendLine("- Raven indexes → PostgreSQL indexes / views");
    sb.AppendLine("- Raven `Include` / document graph → relational JOINs / EF Core `Include`");
    return sb.ToString();
}

static string Escape(string value) => value.Replace("|", "\\|").Replace("`", "\\`");

public sealed class MigrationReport
{
    public string Tool { get; set; } = "";
    public DateTime GeneratedAtUtc { get; set; }
    public string Input { get; set; } = "";
    public int FileCount { get; set; }
    public int DetectedMigrationPoints { get; set; }
    public List<FileAnalysis> Files { get; set; } = new();
}

public sealed class FileAnalysis
{
    public string File { get; set; } = "";
    public List<MigrationFinding> Findings { get; set; } = new();
}

public sealed class MigrationFinding
{
    public int Line { get; set; }
    public string RavenApi { get; set; } = "";
    public string Replacement { get; set; } = "";
    public string Confidence { get; set; } = "";
}
