using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ct.Core.Infrastructure;

/// <summary>
/// Centralizes the JSON <-> jsonb mapping so every genuinely-dynamic column
/// (spec section 113: attributes, exam_subject_order, baseline questionnaire
/// blocks, event_data) is serialized consistently.
/// </summary>
public static class JsonValueConverters
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static PropertyBuilder<Dictionary<string, object?>> HasJsonbDictionary(
        this PropertyBuilder<Dictionary<string, object?>> builder, string columnName)
    {
        var converter = new ValueConverter<Dictionary<string, object?>, string>(
            v => JsonSerializer.Serialize(v, Options),
            v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, Options) ?? new());

        var comparer = new ValueComparer<Dictionary<string, object?>>(
            (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
            v => JsonSerializer.Serialize(v, Options).GetHashCode(),
            v => JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(v, Options), Options) ?? new());

        builder.HasColumnName(columnName)
            .HasColumnType("jsonb")
            .HasConversion(converter, comparer)
            .HasDefaultValueSql("'{}'::jsonb");

        return builder;
    }

    public static PropertyBuilder<List<T>> HasJsonbList<T>(
        this PropertyBuilder<List<T>> builder, string columnName)
    {
        var converter = new ValueConverter<List<T>, string>(
            v => JsonSerializer.Serialize(v, Options),
            v => JsonSerializer.Deserialize<List<T>>(v, Options) ?? new());

        var comparer = new ValueComparer<List<T>>(
            (a, b) => JsonSerializer.Serialize(a, Options) == JsonSerializer.Serialize(b, Options),
            v => JsonSerializer.Serialize(v, Options).GetHashCode(),
            v => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(v, Options), Options) ?? new());

        builder.HasColumnName(columnName)
            .HasColumnType("jsonb")
            .HasConversion(converter, comparer)
            .HasDefaultValueSql("'[]'::jsonb");

        return builder;
    }
}
