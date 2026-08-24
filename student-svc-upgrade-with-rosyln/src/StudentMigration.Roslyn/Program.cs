using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run --project src/StudentMigration.Roslyn -- <legacy-cs-file>");
    return 2;
}

var path = Path.GetFullPath(args[0]);
if (!File.Exists(path))
{
    Console.Error.WriteLine($"File not found: {path}");
    return 2;
}

var source = await File.ReadAllTextAsync(path);
var tree = CSharpSyntaxTree.ParseText(source);
var root = await tree.GetRootAsync();

var findings = new List<Finding>();

foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
{
    var name = invocation.Expression switch
    {
        MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
        IdentifierNameSyntax identifier => identifier.Identifier.Text,
        _ => invocation.Expression.ToString()
    };

    var raven = name switch
    {
        "Load" => ("RAVEN-LOAD", "Replace session.Load<T>(id) with repository.GetByIdAsync(id)."),
        "Store" => ("RAVEN-STORE", "Replace session.Store(entity) with repository.AddAsync(entity)."),
        "SaveChanges" => ("RAVEN-SAVE", "Replace session.SaveChanges() with unit-of-work/DbContext SaveChangesAsync()."),
        "Query" => ("RAVEN-QUERY", "Translate Raven Query<T>/index query to an explicit EF Core LINQ query."),
        "Search" => ("RAVEN-SEARCH", "Translate Raven Search() to PostgreSQL ILIKE/full-text search as appropriate."),
        "Statistics" => ("RAVEN-STATS", "Replace QueryStatistics.TotalResults with COUNT(*) or a window/count query."),
        "Skip" => ("QUERY-PAGING", "Keep Skip/Take but ensure deterministic SQL ordering."),
        "Take" => ("QUERY-PAGING", "Keep Skip/Take but ensure deterministic SQL ordering."),
        _ => (null, null)
    };

    if (raven.Item1 is not null)
    {
        var line = tree.GetLineSpan(invocation.Span).StartLinePosition.Line + 1;
        findings.Add(new Finding(line, raven.Item1, invocation.ToString(), raven.Item2!));
    }
}

foreach (var type in root.DescendantNodes().OfType<IdentifierNameSyntax>())
{
    if (type.Identifier.Text == "IDocumentSession")
    {
        var line = tree.GetLineSpan(type.Span).StartLinePosition.Line + 1;
        findings.Add(new Finding(
            line,
            "RAVEN-SESSION",
            "IDocumentSession",
            "Introduce IStudentRepository so Raven and PostgreSQL implementations can coexist during migration."));
    }
}

Console.WriteLine($"Roslyn migration bridge report: {path}");
Console.WriteLine(new string('-', 90));

foreach (var finding in findings.OrderBy(x => x.Line))
{
    Console.WriteLine($"L{finding.Line,-4} {finding.Code,-14} {finding.Expression}");
    Console.WriteLine($"      -> {finding.Recommendation}");
}

Console.WriteLine();
Console.WriteLine($"Findings: {findings.Count}");
Console.WriteLine("Next step: use the findings to implement/verify the PostgreSQL repository method.");
return 0;

record Finding(int Line, string Code, string Expression, string Recommendation);
