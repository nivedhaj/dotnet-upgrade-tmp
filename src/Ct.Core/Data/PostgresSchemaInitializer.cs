using Npgsql;
using Microsoft.Extensions.Configuration;

namespace ct._Core.Data;

public static class PostgresSchemaInitializer
{
    public static void Apply(IConfiguration configuration, string schemaPath)
    {
        if (!File.Exists(schemaPath))
            throw new FileNotFoundException("PostgreSQL schema file was not found.", schemaPath);

        var connectionString = configuration.GetConnectionString("Campus")
            ?? configuration["Postgres:ConnectionString"]
            ?? throw new InvalidOperationException("Postgres connection string 'Campus' is not configured.");

        var sql = File.ReadAllText(schemaPath);
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
