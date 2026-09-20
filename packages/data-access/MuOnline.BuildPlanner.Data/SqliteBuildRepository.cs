using System.Text.Json;
using Microsoft.Data.Sqlite;
using MuOnline.BuildPlanner.Application.Builds;

namespace MuOnline.BuildPlanner.Data;

public sealed class SqliteBuildRepository : IBuildRepository
{
    private const string UpsertSql = """
        INSERT INTO builds (
            id,
            schema_version,
            ruleset_id,
            ruleset_version,
            dataset_version,
            dataset_hash,
            engine_version,
            character_class_id,
            evolution_id,
            payload_json
        )
        VALUES (
            $id,
            $schemaVersion,
            $rulesetId,
            $rulesetVersion,
            $datasetVersion,
            $datasetHash,
            $engineVersion,
            $characterClassId,
            $evolutionId,
            $payloadJson
        )
        ON CONFLICT(id) DO UPDATE SET
            schema_version = excluded.schema_version,
            ruleset_id = excluded.ruleset_id,
            ruleset_version = excluded.ruleset_version,
            dataset_version = excluded.dataset_version,
            dataset_hash = excluded.dataset_hash,
            engine_version = excluded.engine_version,
            character_class_id = excluded.character_class_id,
            evolution_id = excluded.evolution_id,
            payload_json = excluded.payload_json;
        """;

    private const string LoadSql = """
        SELECT payload_json
        FROM builds
        WHERE id = $id;
        """;

    private const string ListSql = """
        SELECT payload_json
        FROM builds
        ORDER BY id;
        """;

    private readonly string connectionString;
    private readonly SqliteWriteContentionPolicy writePolicy;

    public SqliteBuildRepository(
        string connectionString,
        SqliteWriteContentionPolicy writePolicy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(writePolicy);
        this.connectionString = connectionString;
        this.writePolicy = writePolicy;
    }

    public Task SaveAsync(
        CharacterBuild build,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(build);
        cancellationToken.ThrowIfCancellationRequested();
        var payload = JsonSerializer.Serialize(build);

        using var connection = OpenConnection();
        try
        {
            writePolicy.Execute(
                connection,
                (writeConnection, transaction) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    using var command = writeConnection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = UpsertSql;
                    command.Parameters.AddWithValue("$id", build.Id);
                    command.Parameters.AddWithValue("$schemaVersion", build.SchemaVersion);
                    command.Parameters.AddWithValue("$rulesetId", build.Ruleset.Id);
                    command.Parameters.AddWithValue("$rulesetVersion", build.Ruleset.Version);
                    command.Parameters.AddWithValue("$datasetVersion", build.Dataset.Version);
                    command.Parameters.AddWithValue("$datasetHash", build.Dataset.Hash);
                    command.Parameters.AddWithValue("$engineVersion", build.EngineVersion);
                    command.Parameters.AddWithValue("$characterClassId", build.CharacterClassId);
                    command.Parameters.AddWithValue("$evolutionId", build.EvolutionId);
                    command.Parameters.AddWithValue("$payloadJson", payload);
                    command.ExecuteNonQuery();
                });
        }
        catch (SqliteWriteContentionException exception)
        {
            throw new BuildException(
                BuildErrorCodes.WriteConflict,
                $"Build '{build.Id}' could not be saved because SQLite remained locked.",
                exception);
        }

        return Task.CompletedTask;
    }

    public Task<CharacterBuild?> LoadAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = LoadSql;
        command.Parameters.AddWithValue("$id", id);
        var payload = command.ExecuteScalar() as string;

        if (payload is null)
        {
            return Task.FromResult<CharacterBuild?>(null);
        }

        var build = JsonSerializer.Deserialize<CharacterBuild>(payload)
            ?? throw new JsonException($"Stored build '{id}' deserialized to null.");
        return Task.FromResult<CharacterBuild?>(build);
    }

    public Task<IReadOnlyList<CharacterBuildSummary>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var summaries = new List<CharacterBuildSummary>();
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = ListSql;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var payload = reader.GetString(0);
            var build = JsonSerializer.Deserialize<CharacterBuild>(payload)
                ?? throw new JsonException("A stored build deserialized to null while listing.");
            summaries.Add(
                new CharacterBuildSummary(
                    build.Id,
                    build.SchemaVersion,
                    build.CharacterClassId,
                    build.EvolutionId,
                    build.Level,
                    build.ResetCount,
                    build.PointsPerReset,
                    build.Dataset.Version));
        }

        return Task.FromResult<IReadOnlyList<CharacterBuildSummary>>(summaries);
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }
}